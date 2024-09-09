using Sendi.Client;
using Sendi.Messages;
using SendiDemo1.MessageTypes;
using System.Windows;

namespace SendiDemo1
{
	/// <summary>
	/// Interaction logic for WndClient.xaml
	/// </summary>
	public partial class WndClient : Window
    {
        private IMessageClient messageClient;

        public WndClient(IMessageClient msgClient)
        {
            InitializeComponent();
            Width = 300;
            Height = 150;

            // connect to message bus
            messageClient = msgClient;
            messageClient.SetFilterMode(AbstractMessageClient.EnmFilterMode.Filter);
            messageClient.AddMsgTypeToFilter(new MsgColor(null), MsgColorReceived);
            messageClient.Start();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            messageClient.Stop();
        }

        public void MsgColorReceived(AbstractMessage message)  
        {
			if (message is MsgColor msgColor)
			{
				Application.Current.Dispatcher.Invoke((Action)delegate
				{
					this.Background = msgColor.Brush;
				});
			}
        }
	}
}
