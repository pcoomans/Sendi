using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Sendi.Messages.ErrorMessage
{
    public enum EnmErrors
    {

    }
    public enum EnmExceptionLevel
    {
        Undefined = 0,
        Error = 1,
        Fatal = 2
    }

    [DataContract]
    public class ErrorMessage : AbstractMessage
    {
		[DataMember]
		public EnmErrors Error { get; private set; }

		[DataMember]
		public EnmExceptionLevel ExceptionLevel { get; private set; }

		[DataMember]
		public string ErrorInfo { get; private set; }

		public ErrorMessage(EnmErrors Error, EnmExceptionLevel exceptionLevel, string errorInfo)
		{
			this.Error = Error;
			this.ExceptionLevel = ExceptionLevel;
			this.ErrorInfo = errorInfo;
		}

		public override string ToString()
		{
			return $"ErrorMessage: Error={this.Error}/ExceptionLevel={this.ExceptionLevel}/ErrorInfo={this.ErrorInfo}";
		}
	}
}
