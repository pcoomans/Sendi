using System;

namespace Messenger.Messages
{
    public interface IMessage
    {
        int MsgSequenceNr { get; }
        IMessageComponent Sender { set; get; }
        int GetMessageTypeId();
        int GetMessageId();

        string ToString();
    }

    public interface IMessageData
    {
        int GetMessageId();
    }
}
