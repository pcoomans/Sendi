using System.Runtime.Serialization;

namespace Sendi.Messages.SystemLogMessage
{
	public class SystemLogMessage : AbstractMessage
	{
        [DataMember]
        public string LogString { get; private set; }

        public SystemLogMessage(string logString)
        {
            this.LogString = logString;
        }

        public override string ToString()
        {
            return $"SystemLogMessage: LogString={this.LogString}";
        }
    }

}
