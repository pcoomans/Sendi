﻿using Sendi.Client;
using Sendi.Dispatcher;
using Sendi.Messages;
using Sendi.Util;
using SendiDemo1.MessageTypes;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SendiDemo1
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private SimpleMessageClient smc;
		private MessageDispatcher mdp;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			mdp = MessageDispatcher.GetMessageDispatcher();
			mdp.Start(false, 0);

			mdp.StatsDataChanged += Mdp_StatsDataChanged;

			smc = new SimpleMessageClient("main");
			smc.SetDispatcher(mdp);
			smc.SetFilterMode(AbstractMessageClient.EnmFilterMode.Filter);
			smc.AddMsgTypeToFilter(typeof(MsgColor), MsgColorReceived);
			smc.Start();
			smc.receiveOwnMessages = false;
		}

		private void Mdp_StatsDataChanged(object sender, StatsDataChangedEventArgs e)
		{
			Application.Current.Dispatcher.Invoke((Action)delegate
			{
				LblNrClients.Content = $"{e.TotalNrAttachedClients}";
				LblNrReceivedMsgs.Content = $"{e.TotalNrReceivedMessages}";
				LblNrSentMsgs.Content = $"{e.TotalNrSentMessages}";
			});
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

		private void Window_Unloaded(object sender, RoutedEventArgs e)
		{
			mdp = MessageDispatcher.GetMessageDispatcher();
			mdp.Stop();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			CreateClientWindow();
		}

		private void CreateClientWindow()
		{
			var smc = new Sendi.Client.SimpleMessageClient("a");
			smc.SetDispatcher(MessageDispatcher.GetMessageDispatcher());

			WndClient client = new WndClient(smc);
			client.Show();
		}

		private void colorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
		{
			Color c = e.NewValue.Value;
			SolidColorBrush scb = new SolidColorBrush(c);

			MsgColor mc = new MsgColor(scb);
			smc.SendMessage(mc);
		}
	}
}