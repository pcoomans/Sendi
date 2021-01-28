using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Messenger.Messages.ErrorMessage
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
    public class ErrorData : IMessageData
    {
        [DataMember]
        public EnmErrors Error { get; private set; }

        [DataMember]
        public EnmExceptionLevel ExceptionLevel { get; private set; }

        [DataMember]
        public string ErrorInfo { get; private set; }

        public ErrorData(EnmErrors Error, EnmExceptionLevel exceptionLevel, string errorInfo)
        {
            this.Error = Error;
            this.ExceptionLevel = ExceptionLevel;
            this.ErrorInfo = errorInfo;
        }
        public int GetMessageId()
        {
            return (int)this.Error;
        }

        public override string ToString()
        {
            return $"ErrorData: Error={this.Error}/ExceptionLevel={this.ExceptionLevel}/ErrorInfo={this.ErrorInfo}";
        }

    }

    public class ErrorMessage : Message<ErrorData>
    {
        public ErrorMessage(ErrorData data) : base(data)
        { }
    }
}
