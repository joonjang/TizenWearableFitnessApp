using FitCompanion.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace FitCompanion.ViewModel
{
    class ModalEditViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<TempList> tempList { get; set; }
        public ICommand CancelCommand { get; }
        public ICommand SaveCommand { get; }
        
        public ModalEditViewModel()
        {
            SaveCommand = new Command(SaveFunction);
            CancelCommand = new Command(CancelFunction);

            MessagingCenter.Subscribe<MainViewModel, WorkoutModel>(this, "EditWorkoutInfo", (sender, obj) =>
            {
                WorkoutName = obj.Name;
                RepString = obj.Rep.Replace("Reps: ", "");
                string[] weightArray = obj.Weight.Split('|');
                FilterWeight(weightArray);
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        // filters out the '|' in the weight single list and removes white spaces
        void FilterWeight(string[] weightArray)
        {
            tempList = new ObservableCollection<TempList>();
            int i = 1;
            foreach(string item in weightArray)
            {
                string tmp = item.Trim();
                if (!string.IsNullOrEmpty(tmp))
                {
                    tempList.Add(new TempList()
                    {
                        WeightAmount = tmp,
                        SetCount = i
                    });
                    i++;
                }
            }
            OnPropertyChanged(nameof(tempList));
        }

        WorkoutModel SaveEdit()
        {
            WorkoutModel sendBackObject = new WorkoutModel();
            sendBackObject.Name = WorkoutName;
            sendBackObject.Rep = "Reps: " + RepString;
            foreach (var item in tempList)
            {
                if (string.IsNullOrEmpty(item.WeightAmount))
                {
                    item.WeightAmount = "0";
                }
                sendBackObject.Weight += item.WeightAmount + " | ";
            }
            return sendBackObject;
        }

        async void SaveFunction()
        {
            MessagingCenter.Send<ModalEditViewModel, WorkoutModel>(this, "EditWorkout", SaveEdit());
            await Shell.Current.Navigation.PopModalAsync();
        }

        async void CancelFunction()
        {
            await Shell.Current.Navigation.PopModalAsync();
        }

        private string workoutName;
        public string WorkoutName
        {
            get => workoutName;
            set
            {
                workoutName = value;
                OnPropertyChanged();
            }
        }

        private string repString;
        public string RepString
        {
            get => repString;
            set
            {
                repString = value;
                if (string.IsNullOrEmpty(repString))
                {
                    repString = "0";
                }
                OnPropertyChanged();
            }
        }
    }

    public class TempList
    {
        public string WeightAmount { get; set; }
        public int SetCount { get; set; }
    }
}
