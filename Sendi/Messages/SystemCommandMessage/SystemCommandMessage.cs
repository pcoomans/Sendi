//*************************************************************************
// Project:      Sendi
// Class Name:   SystemCommandMessage
// Description:  This message type is used to send commands to the messaging system components
//               
//*************************************************************************

using Sendi.Messages.SystemCommandMessage;
using System.Runtime.Serialization;

namespace Sendi.Messages.SystemCommand
{
	[DataContract]
    public class SystemCommandMessage : AbstractMessage
    {
        [DataMember]
        public EnmSystemCommands Cmd { get; private set; }

        [DataMember]
        public string TargetRefname { get; private set; }

        [DataMember]
        public string CmdInfo { get; private set; }

        public SystemCommandMessage(EnmSystemCommands cmd, string targetRefname)
        {
            this.Cmd = cmd;
            this.TargetRefname = targetRefname;
            this.CmdInfo = null;
        }
        public SystemCommandMessage(EnmSystemCommands cmd, string targetRefname, string cmdInfo)
        {
            this.Cmd = cmd;
            this.TargetRefname = targetRefname;
            this.CmdInfo = cmdInfo;
        }

        public override string ToString()
        {
            return $"SystemCommandMessage: Cmd={this.Cmd}/TargetRefname={this.TargetRefname}/CmdInfo={this.CmdInfo}";
        }
    }

}
