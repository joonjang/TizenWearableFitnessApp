using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace FitCompanion.ViewModel
{
    
    class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand RefreshCommand { get; }
        public ICommand SendMessageCommand { get; }
        public ICommand CloseConnectionCommand { get; }

        public IProviderService provider { get; set; }


        public MainViewModel()
        {
            provider = DependencyService.Get<IProviderService>();
            RefreshCommand = new Command(RefreshMsgSocket);
            SendMessageCommand = new Command<string>(SendMessage);
            CloseConnectionCommand = new Command(CloseConnection);

            MessagingCenter.Subscribe<object>(Application.Current, "Update", (s) => 
            {
                RefreshMsgSocket();
            });
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        


        public string DeviceSocketInfo
        {
            get => MainPage.DeviceInfoSocket;


        }

        public string ReceivedMsg
        {
            get => MainPage.ReceivedMessage;

        }

        void RefreshMsgSocket()
        {
            OnPropertyChanged(nameof(DeviceSocketInfo));
            OnPropertyChanged(nameof(ReceivedMsg));
        }

        void CloseConnection()
        {
            provider.CloseConnection();
            RefreshMsgSocket();
        }


        void SendMessage(string msg)
        {        
            try
            {
                provider.SendData(msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine("MainViewModel SendMessage error: " + ex);
            }
        }

    }


    
}
