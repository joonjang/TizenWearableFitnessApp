using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FitCompanion
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public IProviderService provider { get; set; }

        public MainPage()
        {
            InitializeComponent();
        }
        private void Connect_Clicked(object sender, EventArgs e)
        {
            provider.CloseConnection();
        }

        private void Send_Clicked(object sender, EventArgs e)
        {

            try
            {

                provider = DependencyService.Get<IProviderService>();
                provider.FindPeers(entry.Text);
            }
            catch (Exception ex)
            {


            }
        }

        private async void OnDataReceived(object sender, EventArgs e)
        {
            var sapargs = e as SAPDataReceivedEventArgs;
            await DisplayAlert("message", sapargs.Message, cancel: "ok");
        }
    }
}
