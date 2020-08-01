using FitWatch.Model;
using Newtonsoft.Json;
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
        //public ICommand ParseCommand { get; }

        public MainViewModel()
        {
            SendMessageCommand = new Command(SendMessage);
            ConnectCommand = new Command(Connect);
            //ParseCommand = new Command(ParseFunction);

        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void Connect()
        {
            //commented out for debugging, this is the final production code for watch to android connection
            try
            {
                Agent = await Agent.GetAgent("/joonspetproject/fit");
                //Agent = await Agent.GetAgent("/sample/hello");
                var peers = await Agent.FindPeers();
                ChannelId = Agent.Channels.First().Value;
                if (peers.Count() > 0)
                {

                    Peer = peers.First();
                    Connection = Peer.Connection;
                    Connection.DataReceived -= Connection_DataReceived;
                    Connection.DataReceived += Connection_DataReceived;
                    await Connection.Open();
                }
                else
                {
                    Toast.DisplayText("No peer found");
                }
            }
            catch (Exception ex)
            {
                Toast.DisplayText("Error: " + ex.Message);
            }


            //todo: for debugging goes through the logic of getting json
            //Connection_DataReceived(new object(), null);
        }


        // broadcaster that looks for messages
        private void Connection_DataReceived(object sender, DataReceivedEventArgs e)
        {
            //// for debugging
            //var receivedInfo = "{\"Week\":\"Week 0\",\"Day\":\"DAY 2\",\"Sets\":[\"Set 1\",\"Set 2\",\"Set 3\",\"Set 4\",\"Set 5\",\"Set 6\"],\"Workouts\":[\"Bench\",\"4\",\"1\",\"2\",\"3\",\"4\",\"8888\",\"8888\",\"Incline Press\",\"8\",\"1\",\"2\",\"3\",\"4\",\"5\",\"6\",\"Flies\",\"12\",\"1\",\"2\",\"3\",\"4\",\"5\",\"6\",\"Tricep Ext\",\"12\",\"1\",\"2\",\"3\",\"4\",\"5\",\"6\"]}";
            ////jsonString = "{\"Week\":\"Week 0\",\"Day\":\"DAY 2\",\"Sets\":[\"Set 1\",\"Set 2\",\"Set 3\",\"Set 4\",\"Set 5\",\"Set 6\"],\"Workouts\":[\"Bench\",\"4\",\"1\",\"2\",\"3\",\"4\",\"8888\",\"8888\",\"Flies\",\"12\",\"1\",\"2\",\"3\",\"4\",\"5\",\"6\",\"Tricep Ext\",\"12\",\"1\",\"2\",\"3\",\"4\",\"5\",\"6\"]}";

            Toast.DisplayText("Workout received");
            var receivedInfo = System.Text.Encoding.ASCII.GetString(e.Data);

            ReceivedMessage = receivedInfo;
            ParseFunction();



        }

        void ParseFunction()
        {
            jsonString = ReceivedMessage;
            MessagingCenter.Send<object>(Application.Current, "Parse");
        }

        void SendMessage()
        {

            try
            {
                if(Peer != null)
                {
                    Connection.Send(ChannelId, Encoding.UTF8.GetBytes(WorkoutViewModel.SendJsonString));
                }
                else
                {
                    Toast.DisplayText("Connect to phone first");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("SendMessage error: " + e);
            }


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
