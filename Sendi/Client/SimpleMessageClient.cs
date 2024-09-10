//*************************************************************************
// Project:      Sendi
// Class Name:   SimpleMessageClient
// Description:  This class is a message client based on AbstractMessageClient
//               
//*************************************************************************


using Sendi.Messages;
using System;
using System.Threading;

namespace Sendi.Client
{
    /// <summary>
    /// This class is a message clients based on AbstractMessageClient
    ///  to be used by clients that can not inherit from AbstractMessageClient.
    /// </summary>
    public class SimpleMessageClient : AbstractMessageClient
    {
        private bool disposed = false;

        //Threading properties
        private Thread lThread;
        private ManualResetEvent EventStop = new ManualResetEvent(false);
        private ManualResetEvent EventStopped = new ManualResetEvent(false);
        private bool stopRunning;

      

        public SimpleMessageClient(String pRefname)
            : base(pRefname)
        {
        }

        public override void Start()
        {
            if (this.acceptedMsgTypeIds.Count==0 && this.selectedFilterMode == EnmFilterMode.Filter)
            {
                throw new SendiException(
                    "SimpleMessageClient -> Start: Since FilterMode is not closed, there should be at least one accepted message type to be received.",
                    660);
            }

            stopRunning = false;

            this.EventStop.Reset();
            this.EventStopped.Reset();

            this.lThread = new Thread(new ThreadStart(ThrStart));
            this.lThread.IsBackground = true;
            this.lThread.Name = String.Format("SimpleMessageClient - {0}", this.refName);
            this.lThread.Start();

            this.SubscribeToDispatcher();
        }

        public override void Stop()
        {
            this.DetachFromDispatcher();

            EventStop.Set();
            // wait for a condition / some time ??
            EventStopped.WaitOne(1000);
        }

        private void ThrStart()
        {

            while (!stopRunning)
            {
                // do task actions
                if (eventMsgDropped.WaitOne(100))
                {
                    ProcessReceivedMessages();
                }

                // check if thread has to stop
                if (EventStop.WaitOne(0))
                {
                    stopRunning = true;
                }
            }
            EventStopped.Set();
        }

        private void ProcessReceivedMessages()
        {
			AbstractMessage m = GetNextMessage();
            while (m != null)
            {
                if (this.acceptedMsgTypeIds.TryGetValue(m.MessageConfig.GetMessageTypeId(), out MessageReceived refToMsgReceivedFunction))
                {
                    if (refToMsgReceivedFunction != null)
                        refToMsgReceivedFunction(m);
                }
                //
                m = GetNextMessage();
            }
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any managed objects here. 
                //
                if (this.EventStop != null)
                    EventStop.Dispose();
                if (this.EventStopped != null)
                    EventStopped.Dispose();
            }

            // Free any unmanaged objects here. 
            //

            disposed = true;

            base.Dispose(disposing);
        }

    }
}
