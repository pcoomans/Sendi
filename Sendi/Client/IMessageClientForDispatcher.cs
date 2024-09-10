using Sendi.Dispatcher;
using Sendi.Messages;

namespace Sendi.Client
{
    public interface IMessageClientForDispatcher : IMessageComponent
    {
        bool GetReceiveOwnMessages();

        void DropMessage(AbstractMessage message);
        void SetDispatcher(MessageDispatcher dispatcher);
    }

}