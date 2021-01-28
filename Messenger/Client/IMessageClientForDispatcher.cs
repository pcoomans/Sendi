using Messenger.Dispatcher;
using Messenger.Messages;

namespace Messenger.Client
{
    public interface IMessageClientForDispatcher : IMessageComponent
    {
        bool GetReceiveOwnMessages();

        void DropMessage(IMessage message);
        void SetDispatcher(MessageDispatcher dispatcher);
    }

}