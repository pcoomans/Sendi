using Sendi.Messages;
using System.Windows.Media;

namespace SendiDemo1.MessageTypes
{

	public class MsgColorData : IMessageData
	{
		public int GetMessageId()
		{
			return (int)EnmMessageTypeId.MsgColor;
		}

		public Brush Brush { get; set; }

		public MsgColorData(Brush brush)
		{
			Brush = brush;
		}
	}

	public class MsgColor : Message<MsgColorData>
	{
		public MsgColor(MsgColorData data) : base(data) 
		{ }
		
    }
}
