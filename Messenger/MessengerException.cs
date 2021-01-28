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
    public enum IndAppErr
    {
        ERROR_XML_FILE_NOT_FOUND = 1001,
        ERROR_XML_NODE_NOT_EXIST,
        ERROR_XML_WRONG_CONFIGURATION_REFERENCE,
        ERROR_XML_MISSING_CONFIGURATION_ITEM,
        ERROR_XML_WRONG_PARAMETER_TYPE,
        ERORR_BUTTON_INITIALISE_MESSAGEFACTORY_UNDEFINED = 3220,
        ERORR_BUTTON_INITIALISE_IODRIVER_UNDEFINED,
        ERORR_BUTTON_INITIALISE_BUTTONCONFIGURATION_UNDEFINED,
        ERORR_BUTTON_INITIALISE_MESSAGEDISPATCHER_UNDEFINED,
        ERROR_MATLAB_DEBUG_ENVIRONMENT_INITIALISATION = 5130,
        ERROR_MATLAB_DEBUG_ENVIRONMENT_ADDING_PATHS,
        ERROR_MATLAB_DEBUG_ENVIRONMENT_SET_PATH,
        ERROR_MATLAB_DEBUG_ENVIRONMENT_PATH_NOT_EXIST,
        ERROR_MATLAB_DEBUG_ENVIRONMENT_CANNOT_CLOSE,
        ERROR_ANALYSER_INITIALIZATION_MATLAB_ENVIRONMENT = 6000,
        ERROR_ANALYSER_CLOSED_MATLAB_FAILED,
        ERROR_ANALYSER_QLSA_INITIALISE,
        ERROR_ANALYSER_SETDEBUGLEVEL,
        ERROR_ANALYSER_DEINITIALISE,
        ERROR_ANALYSER_ISINITIALCALIBRATIONDONE,
        ERROR_ANALYSER_INITIALISEMEASUREMENT,
        ERROR_ANALYSER_SETDARK,
        ERROR_ANALYSER_ANALYSE,
        ERROR_ANALYSER_GETCONCENTRATION_CELLTYPE,
        ERROR_ANALYSER_GETCONCENTRATION_CHARTYPE,
        ERROR_ANALYSER_GETCONCENTRATION_DOUBLETYPE,
        ERROR_ANALYSER_GETCONCENTRATION_DOUBLETYPE_PARAM2,
        ERROR_ANALYSER_INITIALCALIBRATE,
        ERROR_ANALYSER_RECALIBRATE,
        ERROR_ANALYSER_SAVECALIBRATION,
        ERROR_ANALYSER_LIGHTLEVEL,
        ERROR_ANALYSER_PREBURNSPARKUNITON,
        ERROR_ANALYSER_SHOWCALIBRATIONCURVE,
        ERROR_ANALYSER_SHOWCALIBRATIONCURVEFORSTEPANDELEMENT,
        ERROR_ANALYSER_EXPORTMEASUREMENT,
        ERROR_ANALYSER_GETVERSIONINFO,
        ERROR_WRONG_XML_PARAMETER_VALUE = 10000,
        ERROR_INVALID_MANIPULATOR_SEQUENCE = 10100,
        ERROR_COMMAND_MSG_CANNOT_GENERATED_NON_EXISTING_COMMANDTYPE = 90000,
        ERROR_COMMAND_MSG_CANNOT_GENERATED_UNDEFINED_ENUM_COMMANDS,
        ERROR_EVENT_MSG_CANNOT_GENERATED_UNDEFINED_ENUM_EVENT,
        ERROR_STATUS_MSG_CANNOT_GENERATED_UNDEFINED_ENUM_STATUS,
        ERROR_LOGGER_DIDNOT_WRITE_TO_LOG_FILE = 99001,
        ERROR_LOGGER_DIDNOT_WRITE_TO_ANY_LOG_FILE,
        ERROR_LOGGER_HAS_NO_MESSAGE_CLIENT,
        ERROR_LOGGER_HAS_NO_MESSAGE_DISPATCHER,
        ERROR_LOGGER_IS_NOT_INITIALISED,
        ERROR_SEQUENCESCONFIGURATION_MESSAGEDISPATCHER_NOT_DEFINED,
        ERROR_SEQUENCESCONFIGURATION_NOT_INITIALISED,
        ERROR_SEQUENCERUNNER_EXCEPTION,
        ERROR_SPARKUNIT_WRONG_PARAMETER_TYPE
    }

    /// <summary>
    /// This is a specific IndAppComponents exception, used throughout the 
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
        /// Qlsa Exception with string message
        /// </summary>
        /// <param name="message"></param>
        public MessengerException(string message, IndAppErr exceptionId, EnmErrorLevel errorLevel = EnmErrorLevel.Fatal)
            : base(message)
        {
            this.exceptionId = (Int32)exceptionId;
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

        /// <summary>
        /// Qlsa Exception, includes innerexception 
        /// message string and stack trace.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public MessengerException(string message, IndAppErr exceptionId, Exception innerException, EnmErrorLevel errorLevel = EnmErrorLevel.Fatal)
            : base(message, innerException)
        {
            this.exceptionId = (Int32)exceptionId;
            this.errorLevel = errorLevel;
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected MessengerException(SerializationInfo info,StreamingContext context)
            : base(info, context)
        {
            this.exceptionId = (int)info.GetValue("MessengerException.exceptionId", typeof(int));
            this.errorLevel = (EnmErrorLevel)info.GetValue("MessengerException.errorLevel", typeof(EnmErrorLevel));
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("MessengerException.exceptionId", this.exceptionId);
            info.AddValue("MessengerException.errorLevel", this.errorLevel, typeof(EnmErrorLevel));
        }

        public override string ToString()
        {
            if (this.InnerException==null)
                return String.Format("IndAppComponents Exception(id:{0}) {1} / errorLevel:{2}", this.exceptionId, this.Message, this.errorLevel.ToString());
            else
                return String.Format("IndAppComponents Exception(id:{0}) {1} / innerException: {2} / errorLevel:{3}", this.exceptionId, this.Message, this.InnerException.ToString(), this.errorLevel.ToString());
        }
    }
}
