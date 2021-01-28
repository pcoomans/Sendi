using System;
using System.Runtime.Serialization;

namespace Messenger.Messages
{
    [DataContract]
    public abstract class Message<TMessageData> : MarshalByRefObject, IMessage
        where TMessageData : IMessageData
    {
        #region Properties

        [DataMember]
        public readonly static int MessageTypeId;

        public int GetMessageTypeId()
        {
            return Message<TMessageData>.MessageTypeId;
        }
        public int GetMessageId()
        {
            return this.messageData.GetMessageId();
        }

        [DataMember]
        private IMessageComponent sender;
        public IMessageComponent Sender
        {
            set { sender = value; }
            get { return sender; }
        }

        [DataMember]
        private int msgSequenceNr;
        public int MsgSequenceNr
        {
            private set { msgSequenceNr = value; }
            get { return msgSequenceNr; }
        }

        [DataMember]
        private TMessageData messageData;
        public TMessageData MessageData
        {
            private set { this.messageData = value; }
            get { return this.messageData; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// For a class using generics, the static constructor will be called once for each unique 
        /// combination of generic arguments. This way we can automatically give every type a unique id.
        /// (The idea is that comparing an int goes faster then comparing a type)
        /// </summary>
        static Message()
        {
            MessageTypeId = Messenger.Util.Utilities.CreateNewMsgTypeNr();
        }

        /// <summary>
        /// parameterless consructor is needed to do XMl serialization and deserialization
        /// </summary>
        protected Message()
        {
            this.sender = null;
            this.msgSequenceNr = 0;
            this.MessageData = default(TMessageData);
        }
        protected Message(TMessageData messageData)
        {
            this.sender = null;
            this.msgSequenceNr = Messenger.Util.Utilities.GetNextMsgSequenceNr();
            this.MessageData = messageData;
        }

        #endregion

        #region Public Functions

        public override String ToString()
        {
            return $"Message (msgTypeId={Message<TMessageData>.MessageTypeId}/msgSequenceNr={this.msgSequenceNr}): {this.messageData.ToString()}";
        }

        #endregion
    }
}
