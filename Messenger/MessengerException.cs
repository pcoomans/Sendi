//*************************************************************************
// Project:      Messenger
// Class Name:   MessengerException
// Description:  This is a specific Messenger Exception, used throughout the 
//               project. It is used to distinguish Messenger  
//               exceptions from the general .NET exceptions.
//*************************************************************************

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Messenger
{
    /// <summary>
    /// This is a specific Messenger exception, used throughout the 
    /// Messenger source code. It is used to distinguish Messenger exceptions 
    /// from the general .NET exceptions.
    /// </summary>
    [Serializable]
    public class MessengerException : Exception
    {
        public enum EnmErrorLevel
        {
            Undefined,
            Error,
            Fatal
        }

        public Int32 exceptionId;
        public EnmErrorLevel errorLevel;

        /// <summary>
        /// Default exception without any data
        /// </summary>
        public MessengerException()
            : base()
        {
            this.exceptionId = 0;
            this.errorLevel = EnmErrorLevel.Undefined;
        }

        /// <summary>
        /// Qlsa Exception with string message
        /// </summary>
        /// <param name="message"></param>
        public MessengerException(string message, Int32 exceptionId, EnmErrorLevel errorLevel = EnmErrorLevel.Fatal)
            : base(message)
        {
            this.exceptionId = exceptionId;
            this.errorLevel = errorLevel;
        }

        /// <summary>
        /// Qlsa Exception, includes innerexception 
        /// message string and stack trace.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public MessengerException(string message, Int32 exceptionId, Exception innerException, EnmErrorLevel errorLevel = EnmErrorLevel.Fatal)
            : base(message, innerException)
        {
            this.exceptionId = exceptionId;
            this.errorLevel = errorLevel;
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected MessengerException(SerializationInfo info,StreamingContext context)
            : base(info, context)
        {
            this.exceptionId = (int)info.GetValue("MessengerException.exceptionId", typeof(int));
            this.errorLevel = (EnmErrorLevel)info.GetValue("MessengerException.errorLevel", typeof(EnmErrorLevel));
        }

        public override string ToString()
        {
            if (this.InnerException==null)
                return String.Format("Messenger Exception(id:{0}) {1} / errorLevel:{2}", this.exceptionId, this.Message, this.errorLevel.ToString());
            else
                return String.Format("Messenger Exception(id:{0}) {1} / innerException: {2} / errorLevel:{3}", this.exceptionId, this.Message, this.InnerException.ToString(), this.errorLevel.ToString());
        }
    }
}
