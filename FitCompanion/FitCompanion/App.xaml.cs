using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FitCompanion
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            // DependencyService.Register<ProviderService>();
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
