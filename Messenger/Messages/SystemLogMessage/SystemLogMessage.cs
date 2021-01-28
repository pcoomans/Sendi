using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Messenger.Messages.SystemLogMessage
{
    public class SystemLogData : IMessageData
    {
        [DataMember]
        public string LogString { get; private set; }

        public SystemLogData(string logString)
        {
            this.LogString = logString;
        }

        public override string ToString()
        {
            return $"SystemLogData: LogString={this.LogString}";
        }

        public int GetMessageId()
        {
            return 1;
        }
    }

    public class SystemLogMessage : Message<SystemLogData>
    {
        public SystemLogMessage(SystemLogData data) : base(data)
        { }
    }
}
