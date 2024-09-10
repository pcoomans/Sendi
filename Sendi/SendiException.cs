//*************************************************************************
// Project:      Sendi
// Class Name:   SendiException
// Description:  This is a specific Sendi Exception, used throughout the 
//               project. It is used to distinguish Sendi  
//               exceptions from the general .NET exceptions.
//*************************************************************************

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Sendi
{
    /// <summary>
    /// This is a specific Sendi exception, used throughout the 
    /// Sendi source code. It is used to distinguish Sendi exceptions 
    /// from the general .NET exceptions.
    /// </summary>
    [Serializable]
    public class SendiException : Exception
    {
        public enum EnmErrorLevel
        {
            Undefined,
            Error,
            Fatal
        }

        public Int32 ExceptionId { get; private set; }
        public EnmErrorLevel ErrorLevel { get; private set; }   

        /// <summary>
        /// Default Sendi Exception without any data
        /// </summary>
        public SendiException()
            : base()
        {
            ExceptionId = 0;
            ErrorLevel = EnmErrorLevel.Undefined;
        }

        /// <summary>
        /// Sendi Exception with string message
        /// </summary>
        /// <param name="message"></param>
        public SendiException(string message, Int32 exceptionId, EnmErrorLevel errorLevel = EnmErrorLevel.Fatal)
            : base(message)
        {
            ExceptionId = exceptionId;
            ErrorLevel = errorLevel;
        }

        /// <summary>
        /// Sendi Exception, includes InnerException 
        /// message string and stack trace.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public SendiException(string message, Int32 exceptionId, Exception innerException, EnmErrorLevel errorLevel = EnmErrorLevel.Fatal)
            : base(message, innerException)
        {
            ExceptionId = exceptionId;
            ErrorLevel = errorLevel;
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected SendiException(SerializationInfo info,StreamingContext context)
            : base(info, context)
        {
            ExceptionId = (int)info.GetValue("SendiException.exceptionId", typeof(int));
            ErrorLevel = (EnmErrorLevel)info.GetValue("SendiException.errorLevel", typeof(EnmErrorLevel));
        }

        public override string ToString()
        {
            if (InnerException==null)
                return String.Format($"Sendi Exception(id:{ExceptionId}) {Message} / errorLevel:{ErrorLevel.ToString()}");
            else
                return String.Format($"Sendi Exception(id:{ExceptionId}) {Message} / innerException: {InnerException.ToString()} / errorLevel:{ErrorLevel.ToString()}");
        }
    }
}
