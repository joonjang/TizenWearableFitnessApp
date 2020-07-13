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
        public IProviderService provider;
        public static object deviceInfo;

        public MainPage()
        {
            provider = DependencyService.Get<IProviderService>();
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

                
                provider.SendData(entry.Text);
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

        private void deviceInfoTxt_Clicked(object sender, EventArgs e)
        {
            deviceInfoTxt.Text = deviceInfo.ToString();
        }
    }
}
