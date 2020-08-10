using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

// https://docs.microsoft.com/en-us/xamarin/android/platform/binding-java-library/
// https://xamarinhelp.com/creating-xamarin-android-binding-library/
// using Java binding
using Com.Samsung.Accessory;
using Com.Samsung.Android.Sdk;
using Com.Samsung.Android.Sdk.Accessory;
using Java.Interop;


[assembly: Xamarin.Forms.Dependency(typeof(FitCompanion.Droid.ProviderService))]
namespace FitCompanion.Droid
{
    [Service(Exported = true, Name = "FitCompanion.Droid.ProviderService")]
    public class ProviderService : SAAgent, IProviderService
    {
        public static readonly string TAG = typeof(ProviderService).Name;
        public static IBinder mBinder { get; private set; }
        public static readonly Java.Lang.Class SASOCKET_CLASS = Java.Lang.Class.FromType(typeof(ProviderServiceSocket)).Class;
        public static ProviderServiceSocket mSocketServiceProvider;
        private bool _isRunning;
        private Context _context;
        private readonly Task _task;
        private static readonly int CHANNEL_ID = 104;


        [Export(SuperArgumentsString = "\"ProviderService\", ProviderService_ProviderServiceSocket.class")]
        public ProviderService() : base("ProviderService", SASOCKET_CLASS)
        {

        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _isRunning = false;

            if (_task != null && _task.Status == TaskStatus.RanToCompletion)
            {
                _task.Dispose();
            }
        }

        // Explanation of whats happening, Bound Service
        // https://docs.microsoft.com/en-us/xamarin/android/app-fundamentals/services/creating-a-service/bound-services

        public override IBinder OnBind(Intent intent)
        {
            // This method must always be implemented
            Android.Util.Log.Debug(TAG, "OnBind");
            mBinder = new AgentBinder(this);
            return mBinder;
        }

        public override bool OnUnbind(Intent intent)
        {
            // This method is optional to implement
            Android.Util.Log.Debug(TAG, "OnUnbind");
            return base.OnUnbind(intent);
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            FindPeerAgents();

            return base.OnStartCommand(intent, flags, startId);
        }














        const int pendingIntentId = 0;
        const string channelId = "fit_channel_01";
        const string channelName = "Accessory_SDK_Fit";

        NotificationManager manager;
        bool channelInitialized = false;

        void CreateNotificationChannel()
        {

            
            manager = (NotificationManager)Application.Context.GetSystemService(Application.NotificationService);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {

                var channelNameJava = new Java.Lang.String(channelName);
                var channel = new NotificationChannel(channelId, channelNameJava, NotificationImportance.Default)
                {
                    Description = "TEST TEST TEST"
                };
                manager.CreateNotificationChannel(channel);
            }

            channelInitialized = true;
        }

        public override void OnCreate()
        {
            base.OnCreate();

            

            //the reason the notification wont go away
            //https://docs.microsoft.com/en-us/xamarin/android/app-fundamentals/services/foreground-services

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                if (!channelInitialized)
                {
                    CreateNotificationChannel();
                }


                int notifyID = 1;

                Intent intent = new Intent(Application.Context, typeof(MainActivity));









                intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.SingleTop);












                PendingIntent pendingIntent = PendingIntent.GetActivity(Application.Context, pendingIntentId, intent, PendingIntentFlags.UpdateCurrent);

                NotificationCompat.Builder builder = new NotificationCompat.Builder(Application.Context, channelId)

                    .SetAutoCancel(true)

                    //todo: change name
                    .SetContentTitle(TAG)
                    .SetContentText("connected to your watch")

                    .SetContentIntent(pendingIntent)

                    // todo: check icon
                    .SetLargeIcon(BitmapFactory.DecodeResource(Application.Context.Resources, Resource.Drawable.ic_mtrl_chip_checked_circle))
                    .SetSmallIcon(Resource.Drawable.ic_mtrl_chip_close_circle)
                    .SetDefaults((int)NotificationDefaults.Sound | (int)NotificationDefaults.Vibrate)

                    .SetChannelId(channelId)
                    .SetPriority(1)
                    .SetVisibility(1)
                    .SetCategory(Android.App.Notification.CategoryService);

                //StartForeground(notifyID, builder.Build());
                //var notification = builder.Build();
                //manager.Notify(notifyID, notification);


                StartForeground(notifyID, builder.Build());













            }


            _context = this;
            _isRunning = false;

            var mAccessory = new SA();
            try
            {
                mAccessory.Initialize(this);
            }
            catch (SsdkUnsupportedException)
            {
                // try to handle SsdkUnsupportedException
            }
            catch (Exception e1)
            {
                e1.ToString();

                /*
                * Your application can not use Samsung Accessory SDK. Your application should work smoothly
                * without using this SDK, or you may want to notify user and close your application gracefully
                * (release resources, stop Service threads, close UI thread, etc.)
                */
                StopSelf();
            }


            bool isFeatureEnabled = mAccessory.IsFeatureEnabled(SA.DeviceAccessory);

            MainPage.DeviceInfoSocket = "Connected";
            MainPage.InfoFromAndroid();

        }

        //private void DoWork(string msg)
        //{
        //    try
        //    {
        //        FindPeers(msg);


        //    }
        //    catch (Exception e)
        //    {

        //    }
        //    finally
        //    {
        //        StopSelf();
        //    }
        //}

        public void SendData(string msg)
        {

            //mSocketServiceProvider = (ProviderServiceSocket) MainPage.deviceSocketInfo ;

            if (mSocketServiceProvider != null)
            {
                try
                {
                    mSocketServiceProvider.Send(CHANNEL_ID, System.Text.Encoding.ASCII.GetBytes(msg));
                }
                catch (Exception e)
                {
                    Console.WriteLine("SendData error: " + e);
                }
            }
        }


        protected override void OnFindPeerAgentsResponse(SAPeerAgent[] p0, int result)
        {
#if DEBUG
            Console.WriteLine(TAG, "onFindPeerAgentResponse : result =" + result);
#endif

            if (result == PeerAgentFound)
            {
                foreach (SAPeerAgent peerAgent in p0)
                {
                    //  Cache(peerAgent);
                    RequestServiceConnection(peerAgent);

                }
            }
        }

        protected override void OnServiceConnectionRequested(SAPeerAgent p0)
        {
            if (p0 != null)
            {
                AcceptServiceConnectionRequest(p0);

            }
        }



        // this is where the connection socket is called
        protected override void OnServiceConnectionResponse(SAPeerAgent p0, SASocket socket, int result)
        {
            // Cache(socket);
            if ((result == SAAgent.ConnectionSuccess))
            {
                if ((socket != null))
                {
                    mSocketServiceProvider = (ProviderServiceSocket)(socket);


                    // mSocketServiceProvider.Send(CHANNEL_ID, System.Text.Encoding.ASCII.GetBytes(Message));
                }

            }
            else if ((result == SAAgent.ConnectionAlreadyExist))
            {
#if DEBUG
                Console.WriteLine("onServiceConnectionResponse, CONNECTION_ALREADY_EXIST");
#endif

            }

            // var socketConnection = mSocketServiceProvider.IsConnected;

        }

        private bool processUnsupportedException(SsdkUnsupportedException e)
        {
#if DEBUG
            Console.WriteLine(e.ToString());
#endif

            if (e.Equals(SsdkUnsupportedException.VendorNotSupported)
                        || e.Equals(SsdkUnsupportedException.DeviceNotSupported))
            {
                StopSelf();
            }
            else if (e.Equals(SsdkUnsupportedException.LibraryNotInstalled))
            {
#if DEBUG
                Console.WriteLine(TAG + "You need to install Samsung Accessory SDK to use this application.");
#endif

            }
            else if (e.Equals(SsdkUnsupportedException.LibraryUpdateIsRequired))
            {
#if DEBUG
                Console.WriteLine(TAG + "You need to update Samsung Accessory SDK to use this application.");

#endif
            }
            else if (e.Equals(SsdkUnsupportedException.LibraryUpdateIsRecommended))
            {
#if DEBUG
                Console.WriteLine(TAG + TAG, "We recommend that you update your Samsung Accessory SDK before using this application.");

#endif
                return false;
            }

            return true;
        }

        public bool CloseConnection()
        {
            if ((mSocketServiceProvider != null))
            {
                mSocketServiceProvider.Close();
                mSocketServiceProvider = null;

                Intent serviceIntent = new Intent(Application.Context, typeof(ProviderService));
                Application.Context.StopService(serviceIntent);

                return true;
            }
            else
            {
                return false;
            }

        }



        public class AgentBinder : Binder
        {
            public AgentBinder(ProviderService service) => Service = service;

            public ProviderService Service { get; private set; }
        }

        // the connection between watch and android
        public class ProviderServiceSocket : SASocket
        {
            [Export(SuperArgumentsString = "\"ProviderServiceSocket\"")]
            public ProviderServiceSocket() : base(p0: "ProviderServiceSocket")
            {

            }


            public override void OnReceive(int channelId, byte[] bytes)
            {
                // Check received data 

                string message = System.Text.Encoding.UTF8.GetString(bytes);

                MainPage.ReceivedMessage = message;
                MainPage.InfoFromAndroid();
#if DEBUG
                Console.WriteLine("Received: ", message);

            }
#endif


            protected override void OnServiceConnectionLost(int p0)
            {
                // ResetCache();
                Close();
                MainPage.DeviceInfoSocket = "Empty";
                MainPage.InfoFromAndroid();
                Intent serviceIntent = new Intent(Application.Context, typeof(ProviderService));
                Application.Context.StopService(serviceIntent);
            }
            public override void OnError(int p0, string p1, int p2)
            {

                // Error handling
            }
        }

    }

}