using System.Runtime.Serialization;

namespace Sendi.Messages.LogMessage
{
	public enum EnmLogLevel
    {
        info,
        debug,
    }

    [DataContract]
	public class LogMessage : AbstractMessage
	{
		[DataMember]
		public EnmLogLevel LogLevel { get; private set; }

		[DataMember]
		public string LogString { get; private set; }

		public LogMessage(EnmLogLevel logLevel, string logString)
		{
			this.LogLevel = logLevel;
			this.LogString = logString;
		}

		public override string ToString()
		{
			return $"LogMessage: LogLevel={this.LogLevel}/LogString={this.LogString}";
		}
	}
}
