using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms.Platform.Tizen;
using Samsung.Sap;
using System.Windows.Input;

namespace WearableFitnessApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        private Agent Agent;
        private Connection Connection;

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
        }


        private async void Connect()
        {
            try
            {
                //Agent = await Agent.GetAgent("/org/joonspetproject/fit");
                Agent = await Agent.GetAgent("/joonspetproject/fit");
                var peers = await Agent.FindPeers();
                if (peers.Count() > 0)
                {
                    var peer = peers.First();
                    Connection = peer.Connection;
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
        private void Connection_DataReceived(object sender, DataReceivedEventArgs e)
        {

            Toast.DisplayText(e.Peer.DeviceName);

        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            //Device.BeginInvokeOnMainThread((Action)(() =>
            //{
            //    Connect();
            //}));

            Connect();

        }
    }

    

}