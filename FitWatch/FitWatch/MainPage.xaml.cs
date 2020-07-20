using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Tizen.Wearable.CircularUI.Forms;
using Samsung.Sap;

namespace FitWatch
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        private Agent Agent;
        private Connection Connection;
        private Peer Peer;

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
        }


        private async void Connect()
        {
            try
            {
                Agent = await Agent.GetAgent("/joonspetproject/fit");
                //Agent = await Agent.GetAgent("/sample/hello");
                var peers = await Agent.FindPeers();
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
        }

        private void OnMessage(Peer peer, byte[] content)
        {
            //ShowMessage("Received data: " + Encoding.UTF8.GetString(content));
            ReceivedMessage = Encoding.UTF8.GetString(content);
            
        }


        // broadcaster that looks for messages
        private void Connection_DataReceived(object sender, DataReceivedEventArgs e)
        {

            //Toast.DisplayText(e.Peer.DeviceName);
            var receivedInfo = System.Text.Encoding.ASCII.GetString(e.Data);
            
            ReceivedMessage = receivedInfo;

        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            //Device.BeginInvokeOnMainThread((Action)(() =>
            //{
            //    Connect();
            //}));
            ReceivedMessage = "SUCCESS!";
            Connect();
            // SendMessage();
            
        }

        private async void SendMessage()
        {
            // todo: adjust privilge to allow sending message
            await Peer.SendMessage(Encoding.UTF8.GetBytes("Hello Message"));

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

        private void Button_Clicked_1(object sender, EventArgs e)
        {
            SendMessage();
        }
    }
}