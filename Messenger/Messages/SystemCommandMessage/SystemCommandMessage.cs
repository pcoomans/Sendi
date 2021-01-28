//*************************************************************************
// Project:      Messenger
// Class Name:   SystemCommandMessage
// Description:  This message type is used to send commands to the messaging system components
//               
//*************************************************************************

using Messenger.Messages.SystemCommandMessage;
using System;
using System.Runtime.Serialization;

namespace Messenger.Messages.SystemCommand
{
    [DataContract]
    public class SystemCommandData : IMessageData
    {
        [DataMember]
        public EnmSystemCommands Cmd { get; private set; }

        [DataMember]
        public string TargetRefname { get; private set; }

        [DataMember]
        public string CmdInfo { get; private set; }

        public SystemCommandData(EnmSystemCommands cmd, string targetRefname)
        {
            this.Cmd = cmd;
            this.TargetRefname = targetRefname;
            this.CmdInfo = null;
        }
        public SystemCommandData(EnmSystemCommands cmd, string targetRefname, string cmdInfo)
        {
            this.Cmd = cmd;
            this.TargetRefname = targetRefname;
            this.CmdInfo = cmdInfo;
        }

        public override string ToString()
        {
            return $"SystemCommandData: Cmd={this.Cmd}/TargetRefname={this.TargetRefname}/CmdInfo={this.CmdInfo}";
        }

        public int GetMessageId()
        {
            return (int)this.Cmd;
        }
    }

    [DataContract]
    public class SystemCommandMessage : Message<SystemCommandData>
    {
        public SystemCommandMessage(SystemCommandData data) : base(data)
        {  }
    }
}
