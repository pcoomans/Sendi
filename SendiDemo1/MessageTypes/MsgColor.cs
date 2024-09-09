using Sendi.Messages;
using System.Windows.Media;

namespace SendiDemo1.MessageTypes
{

	public class MsgColor : AbstractMessage
	{
		public Brush Brush { get; set; }

		public MsgColor(Brush brush)
		{
			Brush = brush;
		}
	}

}
