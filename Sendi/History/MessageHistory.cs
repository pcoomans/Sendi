using Sendi.Messages;
using System.Collections.Generic;

namespace Sendi.History
{
	public class MessageHistory : Dictionary<int, MessageHistoryOneMessageType>
    {
        public void Add(AbstractMessage m)
        {
            MessageHistoryOneMessageType omth;
            if (!this.TryGetValue(m.MessageConfig.GetMessageTypeId(), out omth))
            {
                omth = new MessageHistoryOneMessageType();
                this[m.MessageConfig.GetMessageTypeId()] = omth;
            }

            omth[m.MessageConfig.MsgSequenceNr] = m;
        }

        public MessageHistoryOneMessageType GetMessageListOfType(int messageTypeId)
        {
            if (this.TryGetValue(messageTypeId, out MessageHistoryOneMessageType oneMessageTypeHistory))
                return oneMessageTypeHistory;
            else
                return null;
        }
    }
}
