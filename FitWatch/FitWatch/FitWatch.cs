using System;
using Tizen.Applications;
using Xamarin.Forms;

namespace FitWatch
{
    class Program : global::Xamarin.Forms.Platform.Tizen.FormsApplication
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            LoadApplication(new App());
        }

        protected override void OnAppControlReceived(AppControlReceivedEventArgs e)
        {
            base.OnAppControlReceived(e);
            //// Reply to a launch request
            //ReceivedAppControl receivedAppControl = e.ReceivedAppControl;
            //if (receivedAppControl.IsReplyRequest)
            //{
            //    AppControl replyRequest = new AppControl();
            //    receivedAppControl.ReplyToLaunchRequest(replyRequest, AppControlReplyResult.Succeeded);
            //}
        }

        static void Main(string[] args)
        {
            var app = new Program();
            Forms.Init(app);
            global::Tizen.Wearable.CircularUI.Forms.FormsCircularUI.Init();
            app.Run(args);
        }
    }
}
