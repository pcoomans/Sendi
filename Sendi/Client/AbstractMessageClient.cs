//*************************************************************************
// Class Name:   AbstractMessageClient
// Description:  Base class for classes that have to incorporate 
//               message client behavior.
//*************************************************************************

using Sendi.Dispatcher;
using Sendi.Messages;
using Sendi.Messages.SystemCommand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Sendi.Client
{
	/// <summary>
	/// <c>AbstractMessageClient</c> is the base class for message clients.
	/// To be used in combination with <c>MessageDispatcher</c>
	/// </summary>
	public abstract class AbstractMessageClient : IMessageClient, IMessageClientForDispatcher
    {


        #region Constants
        const int MAX_MUTEX_WAIT_TIME = 5000;
        #endregion

        #region Properties
        private String _refName = "";
        public String refName
        {
            get
            {
                return this._refName;
            }
        }

        /// <summary>
        /// <c>EnmFilterMode</c> defines these filter modes:
        /// </summary>
        public enum EnmFilterMode : int {
            /// <summary>Accepts no messages, e.g. if the client only needs to send messages </summary>
            Closed,
            /// <summary>Accepts the message types defined in the filter </summary>
            Filter };
        public EnmFilterMode selectedFilterMode = EnmFilterMode.Filter;
        public bool receiveOwnMessages = false;

        // filter settings
        //private List<int> acceptedMsgTypeIds = new List<int>();
        protected Dictionary<int, MessageReceived> acceptedMsgTypeIds = new Dictionary<int, MessageReceived>();

        //
        private List<AbstractMessage> lstDroppedMessages = new List<AbstractMessage>();
        protected AutoResetEvent eventMsgDropped = new AutoResetEvent(false);
        private MessageDispatcher msgDispatcher = null;

        private bool disposed = false;
        #endregion

        #region Constructor
        protected AbstractMessageClient(String refname)
        {
            this._refName = refname;
        }
        #endregion

        /// <summary>
        /// Function to be used by the <c>MessageDispatcher</c> to deliver a message to the <c>MessageClient</c>.
        /// </summary>
        /// <param name="msg">Any message object that implements the <c>IMessage</c> interface.</param>
        public virtual void DropMessage(AbstractMessage msg)
        {
            // check if filter allows this kind of message
            bool accept;
            if (this.selectedFilterMode == EnmFilterMode.Closed)
                accept = false;
            else
                accept = true;

            // add message to the list
            if (accept)
            {
                bool acquiredLock = false;
                try
                {
                    Monitor.Enter(lstDroppedMessages, ref acquiredLock);

                    lstDroppedMessages.Add(msg);
                }
                finally
                {
                    if (acquiredLock)
                        Monitor.Exit(lstDroppedMessages);
                }
                eventMsgDropped.Set();
            }
        }

        /// <summary>
        /// Function to be used by <c>MessageDispatcher</c> to make itself known to the <c>MessageClient</c>.
        /// The <c>MessageClient</c> needs this to be able to send messages.
        /// </summary>
        /// <param name="dispatcher">The <c>MessageDispatcher</c> instance </param>
        public virtual void SetDispatcher(MessageDispatcher dispatcher)
        {
            this.msgDispatcher = dispatcher;
        }
        public virtual MessageDispatcher GetMessageDispatcher()
        {
            return this.msgDispatcher;
        }

        protected virtual void SubscribeToDispatcher()
        {
            // check injected properties
            if (this.msgDispatcher == null)
            {
                SendiException qe = new SendiException(
                    "AbstractMessageClient -> SubscribeToDispatcher: reference to 'messageDispatcher' is undefined.",
                    9999);
                throw (qe);
            }

            this.msgDispatcher.Attach(this);
        }
        protected virtual void DetachFromDispatcher()
        {
            if (this.msgDispatcher != null)
            {
                this.msgDispatcher.Detach(this);
            }
        }

        public abstract void Start();
        public abstract void Stop();

        /// <summary>
        /// This function is used by a <c>MessageClient</c> instance to distribute 
        /// messages to all other <c>MessageClient</c> instances.
        /// </summary>
        /// <param name="msg">Any message object that implements the <c>IMessage</c> interface.</param>
        public virtual void SendMessage(AbstractMessage msg)
        {
            if (this.msgDispatcher != null)
            {
                if (msg.MessageConfig.Sender == null)
                    msg.MessageConfig.Sender = this;
                this.msgDispatcher.SendMessage(msg, this);
            }
        }

        public virtual void SetFilterMode(EnmFilterMode filterMode)
        {
            this.selectedFilterMode = filterMode;
        }

        public virtual void SetReceiveOwnMessages(bool receiveOwnMessages)
        {
            this.receiveOwnMessages = receiveOwnMessages;
        }
        public bool GetReceiveOwnMessages()
        {
            return this.receiveOwnMessages;
        }

        public virtual void ClearFilter()
        {
            this.acceptedMsgTypeIds.Clear();
        }
        public virtual void AddMsgTypeToFilter(AbstractMessage msgExample, MessageReceived refHandleMessageFunction)
        {
            int messageTypeId = msgExample.MessageConfig.GetMessageTypeId();
            this.acceptedMsgTypeIds[messageTypeId] = refHandleMessageFunction;
        }
        public virtual void AddMsgTypeToFilter(Type msgType, MessageReceived refHandleMessageFunction)
        {
            //            object result = Activator.CreateInstance(msgType);

            // CALL STATIC CONSTRUCTIOR ??
            object result = Activator.CreateInstance(msgType);
            if( result is AbstractMessage msg)
            {
			    int _messageTypeId = msg.MessageConfig.GetMessageTypeId();
			    this.acceptedMsgTypeIds[_messageTypeId] = refHandleMessageFunction;

            }

            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(msgType.TypeHandle);

            FieldInfo info = msgType.GetField("MessageTypeId", BindingFlags.Static);
            int messageTypeId = (int)info.GetValue(null);

		}

		//public virtual void AddMsgTypeToFilter(Type msgType)
		//{
		//    //            object result = Activator.CreateInstance(msgType);

		//    // CALL STATIC CONSTRUCTIOR ??
		//    //object result = Activator.CreateInstance(msgType);

		//    System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(msgType.TypeHandle);

		//    FieldInfo info = msgType.GetField("MessageTypeId", BindingFlags.Static);
		//    int messageTypeId = (int)info.GetValue(null);

		//    if (!this.acceptedMsgTypeIds.Contains(messageTypeId))
		//    {
		//        this.acceptedMsgTypeIds.Add(messageTypeId);
		//    }
		//}
		public virtual void RemoveMsgTypeFromFilter(Type msgType)
        {
            FieldInfo info = msgType.GetField("MessageTypeId", BindingFlags.Static);
            int messageTypeId = (int)info.GetValue(null);

            if (this.acceptedMsgTypeIds.ContainsKey(messageTypeId))
            {
                this.acceptedMsgTypeIds.Remove(messageTypeId);
            }
        }
        public virtual int GetNrOfMessageTypesInFilter()
        {
            return this.acceptedMsgTypeIds.Count;
        }

        protected int GetNrOfDroppedMessages()
        {
            int count = 0;
            bool acquiredLock = false;
            try
            {
                Monitor.Enter(this.lstDroppedMessages, ref acquiredLock);
                count = this.lstDroppedMessages.Count;
            }
            finally
            {
                if(acquiredLock)
                    Monitor.Exit(this.lstDroppedMessages);
            }
            return count;
        }
        protected AbstractMessage GetNextMessage()
        {
			AbstractMessage msg = null;
            bool acquiredLock = false;
            try
            {
                Monitor.Enter(this.lstDroppedMessages, ref acquiredLock);

                if (this.lstDroppedMessages.Count > 0)
                {
                    msg = this.lstDroppedMessages.First();
                    this.lstDroppedMessages.RemoveAt(0);
                }
            }
            finally
            {
                if (acquiredLock)
                    Monitor.Exit(this.lstDroppedMessages);
            }
            return msg;
        }
        public void GetBufferedMessagesOfType(AbstractMessage msgExample)
        {
            int messageTypeId = msgExample.MessageConfig.GetMessageTypeId();

            this.msgDispatcher.SendMessage(
                new SystemCommandMessage(
                        Messages.SystemCommandMessage.EnmSystemCommands.CMD_RESEND_ALL_MSGS_OF_TYPE,
                        "MessageDispatcher",
                        messageTypeId.ToString()), 
                this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                    if(this.eventMsgDropped!=null)
                        this.eventMsgDropped.Dispose();
                }

                // Dispose unmanaged resources.
                // ...

                disposed = true;
            }
        }
        ~AbstractMessageClient()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
