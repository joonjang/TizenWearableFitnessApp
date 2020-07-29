using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FitWatch
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class App : Application
    {
        public WorkoutPage workoutPage;
        public App()
        {
            InitializeComponent();
            CarouselPage carouselPage = new CarouselPage();
            
            carouselPage.Children.Add(new MainPage());
            carouselPage.Children.Add(new WorkoutPage());


            MainPage = carouselPage;
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}

