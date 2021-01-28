using Messenger.Messages;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Messenger.History
{
    public class MessageHistory : Dictionary<int, MessageHistoryOneMessageType>
    {
        public void Add(IMessage m)
        {
            MessageHistoryOneMessageType omth;
            if (!this.TryGetValue(m.GetMessageTypeId(), out omth))
            {
                omth = new MessageHistoryOneMessageType();
                this[m.GetMessageTypeId()] = omth;
            }

            omth[m.GetMessageId()] = m;
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
