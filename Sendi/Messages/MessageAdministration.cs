using System;
using System.Runtime.Serialization;

namespace Sendi.Messages
{
	public class MessageAdministration : MarshalByRefObject
    {
        #region Properties

        [DataMember]
        public int MessageTypeId { get; private set; }

        [DataMember]
        public int MsgSequenceNr { get; private set; }

        [DataMember]
        public IMessageComponent Sender { get; set; }

        public int GetMessageTypeId()
        {
            return MessageTypeId; 
        }

		#endregion

		/// <summary>
		/// parameterless constructor is needed to do XMl serialization and deserialization
		/// </summary>
		protected MessageAdministration()
        {
            Sender = null;
            MsgSequenceNr = 0;
        }

        public MessageAdministration(int messageTypeId)
        {
            Sender = null;
            MessageTypeId = messageTypeId;
            MsgSequenceNr = Util.Utilities.GetNextMsgSequenceNr();
        }

    }
}
