using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Messenger.Messages.LogMessage
{
    public enum EnmLogLevel
    {
        info,
        debug,
    }

    [DataContract]
    public class LogData : IMessageData
    {
        [DataMember]
        public EnmLogLevel LogLevel { get; private set; }

        [DataMember]
        public string LogString { get; private set; }

        public LogData(EnmLogLevel logLevel, string logString)
        {
            this.LogLevel = logLevel;
            this.LogString = logString;
        }
        public int GetMessageId()
        {
            return 1;
        }

        public override string ToString()
        {
            return $"LogData: LogLevel={this.LogLevel}/LogString={this.LogString}";
        }

    }

    public class LogMessage : Message<LogData>
    {
        public LogMessage(LogData data) : base(data)
        { }
    }
}
