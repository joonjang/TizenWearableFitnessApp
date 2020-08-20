using FitWatch.Model;
using Newtonsoft.Json;
using Samsung.Sap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using Tizen;
using Tizen.Applications;
using Tizen.System;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace FitWatch.ViewModel
{
    class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Agent Agent;
        private Connection Connection;
        private Peer Peer;
        private Channel ChannelId;

        public ICommand SendMessageCommand { get; }
        public ICommand ConnectCommand { get; }
        public ICommand RunForegroundCommand { get; }
        public ICommand StoreCommand { get; }
        public ICommand CloseCommand { get; }


        public MainViewModel()
        {

            SendMessageCommand = new Command(SendMessage);
            ConnectCommand = new Command(Connect);
            RunForegroundCommand = new Command(RunInForegroundMethod);

            StoreCommand = new Command(LaunchStore);
            CloseCommand = new Command(() => 
            {
                MasterUI = true;
                StoreUI = false;
            });

            //display current watch workout
            MessagingCenter.Subscribe<WorkoutViewModel, string>(this, "CurrentInfo", (sender, arg) =>
            {
                CurrentAvailableInfo = arg;
                Preferences.Set("CurrentWorkout", arg);
            });

            CurrentAvailableInfo = Preferences.Get("CurrentWorkout", "");

            // disables the foreground shell from showing
            MessagingCenter.Subscribe<ForegroundButtonViewModel>(this, "StopForeground", (sender) =>
            {
                StopForegroundMethod();
            });

        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void LaunchStore()
        {
            AppControl launchStore = new AppControl();
            string storeUrl = @"https://play.google.com/store/apps/details?id=com.joonspetproject.fitcompanion";
            launchStore.Operation = AppControlOperations.Default;
            launchStore.ApplicationId = "com.samsung.w-manager-service";
            launchStore.ExtraData.Add("deeplink", storeUrl);
            launchStore.ExtraData.Add("type", "phone");

            try
            {
                AppControl.SendLaunchRequest(launchStore);
            }
            catch(Exception e)
            {
                Console.WriteLine("Store launch error: " + e);
            }

            ShowMasterUI(true);

        }


        private void LaunchApp()
        {
            AppControl launchControl = new AppControl();
            launchControl.Operation = AppControlOperations.Default;
            launchControl.ApplicationId = "com.samsung.w-manager-service";
            launchControl.ExtraData.Add("deeplink", "joonspetproject://fit");
            //launchControl.ExtraData.Add("uri", "market://details?id=com.joonspetproject.fitcompanion");
            launchControl.ExtraData.Add("type", "phone");

            // market://details?id=com.joonspetproject.fitcompanion
            // https://play.google.com/store/apps/details?id=com.joonspetproject.fitcompanion
            // com.samsung.w-manager-service
            try
            {
                AppControl.SendLaunchRequest(launchControl);
                Console.WriteLine("Send Launch Request SENT");
            }
            catch (Exception e)
            {
                //ShowMessage("APPLAUNCH", "APP NOT FOUND ?");
                Console.WriteLine("LaunchApp error: " + e);
            }
        }
        private void ShowMessage(string message, string debugLog = null)
        {
            Toast.DisplayText(message, 1000);
            if (debugLog != null)
            {
                debugLog = message;
            }
            Console.WriteLine("[DEBUG] " + message);
        }

        private void ShowMasterUI(bool param)
        {
            MasterUI = param;
            StoreUI = !param;
        }

        private async void Connect()
        {
            

            //commented out for debugging, this is the final production code for watch to android connection
            try
            {
                Agent = await Agent.GetAgent("/joonspetproject/fit");
                var peers = await Agent.FindPeers();
                ChannelId = Agent.Channels.First().Value;
                if (peers.Count() > 0)
                {
                    Console.WriteLine("Peer found");
                    Peer = peers.First();
                    Connection = Peer.Connection;
                    Connection.DataReceived -= Connection_DataReceived;
                    Connection.DataReceived += Connection_DataReceived;
                    await Connection.Open();
                    ShowMessage("Connected");
                    LaunchApp();
                }
                else
                {
                    Console.WriteLine("Peer not found");

                    ShowMasterUI(false);

                }

                
            }
            catch (Exception ex)
            {
                ShowMessage("Error: " + ex.Message);
            }

        }


        // broadcaster that looks for messages
        private void Connection_DataReceived(object sender, DataReceivedEventArgs e)
        {
            ShowMessage("Workout received");
            string receivedJson = System.Text.Encoding.ASCII.GetString(e.Data);

            MessagingCenter.Send<MainViewModel, string>(this, "Parse", receivedJson);

        }



        void StopForegroundMethod()
        {
            Shell.Current.Navigation.PushAsync(new AppShell());
            Tizen.System.Display.StateChanged -= OnDisplayOn;
            //System.Environment.Exit(0);
        }

        void RunInForegroundMethod()
        {
            Shell.Current.Navigation.PushAsync(new AppShellForeground());
            Tizen.System.Display.StateChanged += OnDisplayOn;
            appControl = new AppControl
            {
                Operation = AppControlOperations.Default,
                ApplicationId = "org.tizen.joonspetproject.FitWatch"
            };
        }



        AppControl appControl;
        private void OnDisplayOn(object sender, DisplayStateChangedEventArgs args)
        {
            if (args.State == DisplayState.Normal)
            {
                try
                {
                    AppControl.SendLaunchRequest(appControl);
                }
                catch (Exception e)
                {
                    Console.WriteLine("CONSOLE ERROR: " + e);
                }

            }
        }


        void SendMessage()
        {

            try
            {
                if (Peer != null)
                {
                    Connection.Send(ChannelId, Encoding.UTF8.GetBytes(WorkoutViewModel.SendJsonString));
                    ShowMessage("Sent to phone");
                }
                else
                {
                    ShowMessage("Connect to phone first");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("SendMessage error: " + e);
            }


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


        private string currentAvailableInfo;
        public string CurrentAvailableInfo
        {
            get => currentAvailableInfo;
            set
            {
                currentAvailableInfo = value;
                OnPropertyChanged();
            }
        }

        private bool masterUI = true;
        public bool MasterUI
        {
            get => masterUI;
            set
            {
                masterUI = value;
                OnPropertyChanged();
            }
        }

        private bool storeUI = false;
        public bool StoreUI
        {
            get => storeUI;
            set
            {
                storeUI = value;
                OnPropertyChanged();
            }
        }
    }
}
