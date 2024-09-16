//*************************************************************************
// Project:      Sendi
// Class Name:   MessageDispatcher
// Description:  This class is the central unit in the internal messaging system.
//               Clients based on AbstractMessageClient can connect to send
//               and receive messages
//*************************************************************************

using Sendi.Client;
using Sendi.History;
using Sendi.Messages;
using Sendi.Messages.SystemCommand;
using Sendi.Messages.SystemCommandMessage;
using Sendi.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;

namespace Sendi.Dispatcher
{
    public class MessageDispatcher : MarshalByRefObject, IMessageComponent, IDisposable
    {
        #region Singleton related

        private static readonly MessageDispatcher messageDispatcher = new MessageDispatcher();
        public static MessageDispatcher GetMessageDispatcher()
        {
            return messageDispatcher;
        }

		#endregion

		#region StatsDataChanged Event

		public event EventHandler<StatsDataChangedEventArgs> StatsDataChanged;

		protected virtual void OnStatsDataChanged()
		{
			EventHandler<StatsDataChangedEventArgs> handler = StatsDataChanged;
			if (handler != null)
			{
				handler(this, new StatsDataChangedEventArgs(sendiStats));
			}
		}

		#endregion

		#region Delayed StatsDataChanged Event

		/// <summary>
		/// Use this function to limit the nr of events to max 10 per second
		/// Stick to the standard 'OnStatsDataChanged' to fire the event immediately
		/// </summary>
		protected virtual void OnStatsDataChanged_Delayed()
        {
			EventHandler<StatsDataChangedEventArgs> handler = StatsDataChanged;
			if (handler != null && !timer.Enabled)
            {
				timer.Enabled = true;
			}
		}

        private System.Timers.Timer timer;
		public void TimerElapsedEventHandler(object sender, ElapsedEventArgs e)
        {
			timer.Enabled = false;
            OnStatsDataChanged();
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

        private List<AbstractMessage> lstDroppedMessages = new List<AbstractMessage>();
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

		private bool _disposed = false;

		private MessageDispatcher()
        {
            sendiStats = new SendiStats();

			timer = new System.Timers.Timer(200);
			timer.Elapsed += TimerElapsedEventHandler;
            timer.Enabled = false;
		}

		public virtual void SendMessage(AbstractMessage msg, IMessageComponent sender)
        {
            if (msg.MessageConfig.Sender == null)
                msg.MessageConfig.Sender = sender;

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
                sendiStats.TotalNrAttachedClients++;

                if (this.detailedLoggingActive)
                    this.SysLog(String.Format("Attached msg client: {0}",msgClient.refName));

                OnStatsDataChanged();
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
				sendiStats.TotalNrAttachedClients--;

				this.SysLog(String.Format("Detached msg client: {0}", msgClient.refName));

				OnStatsDataChanged();
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

        private AbstractMessage GetNextMessage()
        {
			AbstractMessage msg = null;
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

            if (string.Compare(msg.TargetRefname,"*",true)==0)
            {
                handleMsg = true;
                forwardMsg = true;
            }
            else if (string.Compare(msg.TargetRefname, this.refName, true) == 0)
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
                switch (msg.Cmd)
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
                        this.GetBufferedMessagesOfType(int.Parse(msg.CmdInfo));
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

			AbstractMessage m = this.GetNextMessage();
            while (m != null)
            {
                // add message to history list
                this.messageHistory.Add(m);
                this.sendiStats.TotalNrReceivedMessages++;
                OnStatsDataChanged_Delayed();
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

        private void DistributeMessageToAllClients(AbstractMessage m)
        {
            bool acquiredLock = false; 
            try
            {
                Monitor.Enter(lstClientsLockObject, ref acquiredLock);

                foreach (IMessageClientForDispatcher msgClient in _msgClients)
                {
                    if (msgClient.GetReceiveOwnMessages() || !Object.ReferenceEquals(m.MessageConfig.Sender, msgClient))
                    {
                        msgClient.DropMessage(m);
						this.sendiStats.TotalNrSentMessages++;
                        OnStatsDataChanged_Delayed();
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
                foreach (KeyValuePair<int, AbstractMessage> kvp in omth)
                {
                    this.DistributeMessageToAllClients(kvp.Value);
                }
            }
        }

        private SendiStats sendiStats;

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

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
			    if (disposing)
			    {
				    // TODO: dispose managed state (managed objects).
			    }

			    // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
			    // TODO: set large fields to null.

                if(timer!=null)
                {
					timer.Dispose();
                    timer = null;
				}

			_disposed = true;
			}
		}

		~MessageDispatcher()
		{
			Dispose(false);
		}
	}

    public class StatsDataChangedEventArgs
    {
		public int TotalNrAttachedClients { get; private set; }
		public int TotalNrReceivedMessages { get; private set; }
		public int TotalNrSentMessages { get; private set; }

        public StatsDataChangedEventArgs(SendiStats sendiStats)
        {
            TotalNrAttachedClients = sendiStats.TotalNrAttachedClients;
            TotalNrReceivedMessages = sendiStats.TotalNrReceivedMessages;
            TotalNrSentMessages = sendiStats.TotalNrSentMessages;
        }
	}

}
