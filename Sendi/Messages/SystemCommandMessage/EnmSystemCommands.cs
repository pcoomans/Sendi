using System.Runtime.Serialization;

namespace Sendi.Messages.SystemCommandMessage
{
    [DataContract]
    public enum EnmSystemCommands : int
    {
        CMD_UNDEFINED,

        CMD_START_DETAILED_LOGGING,
        CMD_STOP_DETAILED_LOGGING,

        CMD_DISABLE_TIMEOUT_CHECKS,
        CMD_ENABLE_TIMEOUT_CHECKS,

        CMD_RESEND_ALL_MSGS_OF_TYPE,
    }
}
