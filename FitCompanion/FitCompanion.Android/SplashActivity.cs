using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace FitCompanion.Droid
{
    [Activity(LaunchMode = LaunchMode.SingleTop, 
        Theme = "@style/MyTheme.Splash",
        MainLauncher = true, 
        NoHistory = true)]
    public class SplashActivity : Activity
    {
        static readonly string TAG = "X:" + typeof(SplashActivity).Name;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Console.WriteLine(TAG, "SplashActivity.OnCreate");
            // Create your application here
        }
        protected override void OnResume()
        {
            base.OnResume();
            Task startupWork = new Task(() => { Startup(); });
            startupWork.Start();
        }
        // Simulates background work that happens behind the splash screen
        void Startup()
        {
            Console.WriteLine(TAG, "Startup work is finished - starting MainActivity.");
            StartActivity(new Intent(Application.Context, typeof(MainActivity)));
        }
        // override back press to do nothing
        public override void OnBackPressed() { }
    }


        //public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        //{
        //    base.OnCreate(savedInstanceState, persistentState);
            
        //}

        // Launches the startup task
        

       
    
}