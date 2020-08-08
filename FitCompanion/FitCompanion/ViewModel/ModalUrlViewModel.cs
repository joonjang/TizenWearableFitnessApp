using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Windows.Input;

namespace FitCompanion.ViewModel
{
    class ModalUrlViewModel : INotifyPropertyChanged
    {
        public ICommand CancelCommand { get; }
        public ICommand SaveCommand { get; }

        public ModalUrlViewModel()
        {
            SaveCommand = new Command(SaveFunction);
            CancelCommand = new Command(CancelFunction);

            MessagingCenter.Subscribe<MainViewModel, string>(this, "SendingSavedUrl", (sender, args) =>
            {
                string[] urlArray = args.Split(',');
                SheetUrl = urlArray[0];
                ScriptUrl = urlArray[1];
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        async void SaveFunction()
        {
            if(string.IsNullOrEmpty(SheetUrl) || string.IsNullOrEmpty(ScriptUrl))
            {
                ErrorString = "URL entry error";
                return;
            }

            MessagingCenter.Send<ModalUrlViewModel, string>(this, "SavedUrlInfo", SheetUrl + "," + ScriptUrl);
            await Shell.Current.Navigation.PopModalAsync();
        }

        async void CancelFunction()
        {
            await Shell.Current.Navigation.PopModalAsync();
        }


        private string sheetUrl;
        public string SheetUrl
        {
            get => sheetUrl;
            set
            {
                sheetUrl = value;
                OnPropertyChanged();
            }
        }

        private string scriptUrl;
        public string ScriptUrl
        {
            get => scriptUrl;
            set
            {
                scriptUrl = value;
                OnPropertyChanged();
            }
        }

        private string errorString;
        public string ErrorString
        {
            get => errorString;
            set
            {
                errorString = value;
                OnPropertyChanged();
            }
        }
    }
}
