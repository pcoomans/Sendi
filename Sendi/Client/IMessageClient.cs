
using Sendi.Messages;
using System;

namespace Sendi.Client
{
    /// <summary>
    /// Interface to be used by message clients connecting to the MessageDispatcher
    /// </summary>
    public interface IMessageClient : IDisposable
    {
        /// <summary>
        /// Forward a message to send to the dispatcher
        /// </summary>
        /// <param name="msg"></param>
        void SendMessage(IMessage msg);

        /// <summary>
        /// Set mode to open or close a filter 
        /// </summary>
        /// <param name="filterMode"></param>
        void SetFilterMode(Client.AbstractMessageClient.EnmFilterMode filterMode);

        /// <summary>
        /// Clear list of selected MsgTypes from filter
        /// </summary>
        /// <param name="msgType"></param>
        void ClearFilter();

        /// <summary>
        /// Add a MsgType to the filter, to start receiving it
        /// </summary>
        /// <param name="msgType"></param>
        //void AddMsgTypeToFilter(Type msgType);
        void AddMsgTypeToFilter(IMessage msgExample, MessageReceived refToFunctionToHandleMessages);

        /// <summary>
        /// Remove a MsgType from the filter, to receive it no longer 
        /// </summary>
        /// <param name="msgType"></param>
        void RemoveMsgTypeFromFilter(Type msgType);

        /// <summary>
        /// Start the messageClient, it wil run in its own thread
        /// </summary>
        void Start();

        /// <summary>
        /// Stop the messageClient 
        /// </summary>
        void Stop();

        /// <summary>
        /// Get all buffered messages of a specific type, e.g. to initialise a new created window
        /// </summary>
        /// <param name="msgType"></param>
        //void GetBufferedMessagesOfType(Type msgType);
        void GetBufferedMessagesOfType(IMessage msgExample);

    }
}
