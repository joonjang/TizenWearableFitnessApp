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
        // public static code is the buffer between the android activity and viewmodel
        public static string DeviceInfoSocket = "Empty";
        public static string ReceivedMessage;

        public MainPage()
        {
            InitializeComponent();
        }

        public static void InfoFromAndroid()
        {
            try
            {
                MessagingCenter.Send<object>(Application.Current, "Update");
            }
            catch
            {

            }
        }
    }
}
