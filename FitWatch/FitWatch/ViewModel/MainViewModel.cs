using Samsung.Sap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using Tizen;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace FitWatch.ViewModel
{
    class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Agent Agent;
        private Connection Connection;
        private Peer Peer;
        private Channel ChannelId;

        public static string jsonString;

        public ICommand SendMessageCommand { get; }
        public ICommand ConnectCommand { get; }

        public MainViewModel()
        {
            SendMessageCommand = new Command(SendMessage);
            ConnectCommand = new Command(Connect);

        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void Connect()
        {
            // commented out for debugging, this is the final production code for watch to android connection
            //try
            //{
            //    Agent = await Agent.GetAgent("/joonspetproject/fit");
            //    //Agent = await Agent.GetAgent("/sample/hello");
            //    var peers = await Agent.FindPeers();
            //    ChannelId = Agent.Channels.First().Value;
            //    if (peers.Count() > 0)
            //    {

            //        Peer = peers.First();
            //        Connection = Peer.Connection;
            //        Connection.DataReceived -= Connection_DataReceived;
            //        Connection.DataReceived += Connection_DataReceived;
            //        await Connection.Open();
            //    }
            //    else
            //    {
            //        Toast.DisplayText("No peer found");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Toast.DisplayText("Error: " + ex.Message);
            //}


            // for debugging goes through the logic of getting json
            jsonString = "{\"Week\":\"Week 0\",\"Day\":\"DAY 2\",\"Sets\":[\"Set 1\",\"Set 2\",\"Set 3\",\"Set 4\",\"Set 5\",\"Set 6\"],\"Workouts\":[\"Bench\",\"4\",\"69\",\"59\",\"420\",\"40\",\"42069\",\"6\",\"Incline Press\",\"8\",\"69\",\"69\",\"420\",\"420\",\"42069\",\"6969\",\"Flies\",\"12\",\"69\",\"69\",\"420\",\"420\",\"42069\",\"6969\",\"Tricep Ext\",\"12\",\"69\",\"69\",\"420\",\"420\",\"42069\",\"6\"]}";
            MessagingCenter.Send<object>(Application.Current, "Parse");
        }


        // broadcaster that looks for messages
        private void Connection_DataReceived(object sender, DataReceivedEventArgs e)
        {

            //Toast.DisplayText(e.Peer.DeviceName);
            var receivedInfo = System.Text.Encoding.ASCII.GetString(e.Data);

            ReceivedMessage = receivedInfo;

        }

        private void SendMessage()
        {
            
            Log.Debug("WINNING", "MESSAGE SENT SAP sap ENTERED");
            try
            {
                // await Peer.SendMessage(Encoding.UTF8.GetBytes("Hello Message"));
                Log.Debug("WINNING", "connection send channel id: " + ChannelId);
                Connection.Send(ChannelId, Encoding.UTF8.GetBytes("connection hello msg"));
            }
            catch (Exception e)
            {
                Log.Debug("WINNING", "MESSAGE SAP CATCH: " + e);
            }
            Log.Debug("WINNING", "MESSAGE SENT SAP sap EXIT");

        }

        // string bind to xaml label to display message from android companion
        private string receivedMessage;
        public string ReceivedMessage
        {
            get { return receivedMessage; }
            set
            {
                receivedMessage = value;
                OnPropertyChanged(nameof(ReceivedMessage));
            }
        }

    }
}
