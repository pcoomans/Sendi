//*************************************************************************
// Project:      Messenger
// Class Name:   MessageDispatcher
// Description:  This class is the central unit in the internal messaging system.
//               Clients based on AbstractMessageClientt can connect to send
//               and receive messages
//*************************************************************************

using Messenger.Client;
using Messenger.History;
using Messenger.Messages;
using Messenger.Messages.SystemCommand;
using Messenger.Messages.SystemCommandMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Messenger.Dispatcher
{
    public class MessageDispatcher : MarshalByRefObject, IMessageComponent
    {
        #region Singleton related

        private static MessageDispatcher messageDispatcher = new MessageDispatcher();
        public static MessageDispatcher GetMessageDispatcher()
        {
            return messageDispatcher;
        }

        #endregion

        private List<IMessageClientForDispatcher> _msgClients = new List<IMessageClientForDispatcher>();

        public override Object InitializeLifetimeService()
        {
            return null;
        }

        private String _refName = "MessageDispatcher";
        public String refName
        {
            get
            {
                return this._refName;
            }
        }

        private Boolean detailedLoggingActive = false;

        private Int32 lastUsedSequenceNr = 0;
        private Int32 GetNextSequenceNr()
        {
            this.lastUsedSequenceNr++;
            if (this.lastUsedSequenceNr > 100000)
            {
                this.lastUsedSequenceNr = 1;
            }
            return this.lastUsedSequenceNr;
        }

        private List<IMessage> lstDroppedMessages = new List<IMessage>();
        protected AutoResetEvent eventMsgDropped = new AutoResetEvent(false);

        //Threading properties
        private Thread lThread;
        //        public bool Active;
        private ManualResetEvent EventInitialised = new ManualResetEvent(false);
        private ManualResetEvent EventStop = new ManualResetEvent(false);
        private ManualResetEvent EventStopped = new ManualResetEvent(false);
        private bool stopRunning;

        private object lstDroppedMessagesLockObject = new object();
        private object lstClientsLockObject = new object();

        const int MAX_MUTEX_WAIT_TIME = 5000;

        private MessageDispatcher()
        {

        }

        public virtual void SendMessage(IMessage msg, IMessageComponent sender)
        {
            if (msg.Sender == null)
                msg.Sender = sender;

            bool acquiredLock = false;
            try
            {
                if (this.detailedLoggingActive)
                    this.SysLog(String.Format("Received message to send ({0})", msg.ToString()));

                Monitor.Enter(lstDroppedMessagesLockObject, ref acquiredLock);
                lstDroppedMessages.Add(msg);
            }
            finally
            {
                if (acquiredLock)
                    Monitor.Exit(lstDroppedMessagesLockObject);
            }
            eventMsgDropped.Set();
        }

        public virtual void Attach(IMessageClientForDispatcher msgClient)
        {
            bool acquiredLock = false;
            try
            {
                Monitor.Enter(lstClientsLockObject, ref acquiredLock);
                _msgClients.Add(msgClient);
                msgClient.SetDispatcher(this);

                if (this.detailedLoggingActive)
                    this.SysLog(String.Format("Attached msg client: {0}",msgClient.refName));
            }
            finally
            {
                if (acquiredLock)
                    Monitor.Exit(lstClientsLockObject);
            }
        }

        public virtual void Detach(IMessageClientForDispatcher msgClient)
        {
            bool acquiredLock = false; 
            try
            {
                Monitor.Enter(lstClientsLockObject, ref acquiredLock);

                msgClient.SetDispatcher(null);
                _msgClients.Remove(msgClient);

                this.SysLog(String.Format("Detached msg client: {0}", msgClient.refName));
            }
            finally
            {
                if (acquiredLock)
                    Monitor.Exit(lstClientsLockObject);
            }
        }

        public void Start(Boolean waitUntilInitialised, Int32 maxWaitTimeMs)
        {
            stopRunning = false;

            this.EventInitialised.Reset();
            this.EventStop.Reset();
            this.EventStopped.Reset();

            this.lThread = new Thread(new ThreadStart(ThrStart));
            this.lThread.Name = "MessageDispatcher";
            this.lThread.IsBackground = true;
            this.lThread.Start();

            if (waitUntilInitialised)
            {
                EventInitialised.WaitOne(maxWaitTimeMs);
            }
        }

        public virtual void Stop()
        {
            //stopRunning = true;
            EventStop.Set();

            // wait for a condition / some time ??
            EventStopped.WaitOne(1000);
        }

        private void ThrStart()
        {
            this.EventInitialised.Set();

            while (!this.stopRunning)
            {
                // do task actions
                if (this.eventMsgDropped.WaitOne(100))
                {
                    this.ProcessReceivedMessages();
                }

                // check if thread has to stop
                if (this.EventStop.WaitOne(0))
                {
                    this.stopRunning = true;
                }
            }
            this.EventStopped.Set();
        }

        private IMessage GetNextMessage()
        {
            IMessage msg = null;
            bool acquiredLock = false;
            try
            {
                Monitor.Enter(this.lstDroppedMessagesLockObject, ref acquiredLock);

                if (this.lstDroppedMessages.Count > 0)
                {
                    msg = this.lstDroppedMessages.First();
                    this.lstDroppedMessages.RemoveAt(0);
                }
            }
            finally
            {
                if (acquiredLock)
                    Monitor.Exit(this.lstDroppedMessagesLockObject);
            }
            return msg;
        }

        private Boolean HandleSystemCommandMessage(SystemCommandMessage msg)
        {
            Boolean handleMsg = false;
            Boolean forwardMsg = false;

            if (string.Compare(msg.MessageData.TargetRefname,"*",true)==0)
            {
                handleMsg = true;
                forwardMsg = true;
            }
            else if (string.Compare(msg.MessageData.TargetRefname, this.refName, true) == 0)
            {
                handleMsg = true;
                forwardMsg = false;
            }
            else 
            {
                handleMsg = false;
                forwardMsg = true;
            }

            if (handleMsg)
            {
                switch (msg.MessageData.Cmd)
                {
                    case EnmSystemCommands.CMD_START_DETAILED_LOGGING:
                        this.detailedLoggingActive = true;
                        this.SysLog("START detailed logging");
                        this.SysLogCurrentClientList();
                        break;
                    case EnmSystemCommands.CMD_STOP_DETAILED_LOGGING:
                        this.SysLog("STOP detailed logging");
                        this.detailedLoggingActive = false;
                        break;
                    case EnmSystemCommands.CMD_RESEND_ALL_MSGS_OF_TYPE:
                        this.GetBufferedMessagesOfType(int.Parse(msg.MessageData.CmdInfo));
                        break;
                }
            }

            return forwardMsg;
        }

        private void SysLog(String strToLog)
        {
        //    if (this.detailedLoggingActive)
        //    {
        //        LogMessage msg = new LogMessage(
        //            EnmLogLevel.info, 
        //            String.Format("SYSLOG ({0}) {1}",this.refName,strToLog));
        //        MsgDataStruct mds = new MsgDataStruct(msg, this, this.GetNextSequenceNr());
        //        this.DistributeMessageToAllClients(mds);
        //    }
        }

        private void SysLogCurrentClientList()
        {
            this.SysLog("current msg client list:");

            bool acquiredLock = false;
            try
            {
                Monitor.Enter(lstClientsLockObject, ref acquiredLock);

                int nr = 0;
                foreach (IMessageClientForDispatcher msgClient in _msgClients)
                {
                    nr++;
                    this.SysLog(String.Format("msg client {0}: {1}", nr, msgClient.refName));
                }
            }
            finally
            {
                if (acquiredLock)
                    Monitor.Exit(lstClientsLockObject);
            }
        }

        private MessageHistory messageHistory = new MessageHistory();

        private void ProcessReceivedMessages()
        {
            Boolean forwardMessage;

            IMessage m = this.GetNextMessage();
            while (m != null)
            {
                // add message to history list
                this.messageHistory.Add(m);
                //
                forwardMessage = true;
                if (m is SystemCommandMessage scm)
                {
                    forwardMessage = this.HandleSystemCommandMessage(scm);
                }
                if (forwardMessage)
                {
                    this.DistributeMessageToAllClients(m);
                }
                //
                m = this.GetNextMessage();
            }
        }

        private void DistributeMessageToAllClients(IMessage m)
        {
            bool acquiredLock = false; 
            try
            {
                Monitor.Enter(lstClientsLockObject, ref acquiredLock);

                foreach (IMessageClientForDispatcher msgClient in _msgClients)
                {
                    if (msgClient.GetReceiveOwnMessages() || !Object.ReferenceEquals(m.Sender, msgClient))
                    {
                        msgClient.DropMessage(m);
                    }
                }
            }
            finally
            {
                if (acquiredLock)
                    Monitor.Exit(lstClientsLockObject);
            }
        }

        private void GetBufferedMessagesOfType(int msgTypeId)
        {
            MessageHistoryOneMessageType omth = this.messageHistory.GetMessageListOfType(msgTypeId);
            if(omth!=null)
            {
                foreach (KeyValuePair<int, IMessage> kvp in omth)
                {
                    this.DistributeMessageToAllClients(kvp.Value);
                }
            }
        }

        public virtual void Close()
        {
            bool acquiredLock = false;            
            try
            {
                Monitor.Enter(lstClientsLockObject, ref acquiredLock);

                foreach (IMessageClientForDispatcher msgClient in _msgClients)
                {
                    msgClient.SetDispatcher(null);
                }
                _msgClients.Clear();
            }
            finally
            {
                if (acquiredLock)
                    Monitor.Exit(lstClientsLockObject);
            }
        }
    }
}
