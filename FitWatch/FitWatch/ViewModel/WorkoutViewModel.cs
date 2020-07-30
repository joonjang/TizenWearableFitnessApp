using FitWatch.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace FitWatch.ViewModel
{
    class WorkoutViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        WatchModel jsonObject;
        List<string> workouts;
        List<string> reps;
        List<string> weights;

        public Command NextCommand { get; }
        public Command PreviousCommand { get; }

        public WorkoutViewModel()
        {
            NextCommand = new Command(On_NextWorkoutInfo, (x) => CanGoNext);
            PreviousCommand = new Command(On_PreviousWorkoutInfo, (x) => CanGoBack);

            MessagingCenter.Subscribe<object>(Application.Current, "Parse", (s) =>
            {
                ParseJson();
            });
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void On_NextWorkoutInfo(object obj)
        {
            NextWorkoutInfo();
        }

        private bool canGoNext = true;
        public bool CanGoNext
        {
            get=>canGoNext;
            set
            {
                canGoNext = value;
                ((Command)NextCommand).ChangeCanExecute();
            }
        }

        private void On_PreviousWorkoutInfo(object obj)
        {
            PreviousWorkoutInfo();
        }

        private bool canGoBack = false;
        public bool CanGoBack
        {
            get => canGoBack;
            set
            {
                canGoBack = value;
                ((Command)PreviousCommand).ChangeCanExecute();
            }
        }

        public void ParseJson()
        {
            

            jsonObject = JsonConvert.DeserializeObject<WatchModel>(MainViewModel.jsonString);

            workouts = new List<string>();
            reps = new List<string>();
            weights = new List<string>();


            bool nextRep = false;

            foreach(string item in jsonObject.Workouts)
            {
                // if subject doesnt have numbers, then its an exercise
                // second element will always be set
                // anything beyond is weights until condition of no numbers

                bool isNotWorkout = Regex.IsMatch(item, @"[0-9]+");

                // if name of workout
                if (!isNotWorkout)
                {
                    workouts.Add(item);
                    // next list item will be the rep number
                    nextRep = true;
                }
                else if (nextRep)
                {
                    reps.Add(item);
                    // next list item will workouts and onwards
                    nextRep = false;
                }
                else
                {
                    weights.Add(item);
                }

            }

            WorkoutTitletString = workouts[0];
            RepString = reps[0];
            WeightString = weights[0];

        }

        int workAndRepInt = 0;
        int weightInt = 0;

        void NextWorkoutInfo()
        {
           
            weightInt++;
            CanGoBack = true;
            if (weightInt == ((jsonObject.Sets.Count * workouts.Count)-1))
            {
                NextBtnString = "Done";
                CanGoNext = false;
            }

            WeightString = weights[weightInt];

            if (weightInt % 6 == 0)
            {
                workAndRepInt++;
                WorkoutTitletString = workouts[workAndRepInt];
                RepString = reps[workAndRepInt];
                
            }

            RegisterNewWeight(true);
        }

        //todo: fix ordering of iteration

        void PreviousWorkoutInfo()
        {

            if (weightInt % 6 == 0)
            {
                if (!(workAndRepInt <= 0))
                {
                    workAndRepInt--;
                }
                WorkoutTitletString = workouts[workAndRepInt];
                RepString = reps[workAndRepInt];

            }

            weightInt--;
            CanGoNext = true;

            if (weightInt <= 0)
            {
                NextBtnString = "Done";
                CanGoBack = false;
            }

            WeightString = weights[weightInt];

            

            RegisterNewWeight(false);
        }

        List<string> NewWeightList = new List<string>();
        void RegisterNewWeight(bool AddOrDelete)
        {
            //todo: add option to go back, delete previous number
            if (AddOrDelete)
                NewWeightList.Add(NewWeightString);
            else
                NewWeightList.Remove(NewWeightString);
        }

        private string newWeightString;
        public string NewWeightString
        {
            get => newWeightString;
            set
            {
                newWeightString = value;
                OnPropertyChanged();
            }
        }

        private string workoutTitleString;
        public string WorkoutTitletString
        {
            get => workoutTitleString;
            set
            {
                workoutTitleString = value;
                OnPropertyChanged();
            }
        }

        private string repString;
        public string RepString
        {
            get => repString;
            set
            {
                repString = "Reps: " + value;
                OnPropertyChanged();
            }
        }

        private string weightString;
        public string WeightString
        {
            get => weightString;
            set
            {
                weightString = "Previous: " + value;
                OnPropertyChanged();
            }
        }

        private string nextBtnString = "Next";
        public string NextBtnString
        {
            get => nextBtnString;
            set
            {
                nextBtnString = value;
                OnPropertyChanged();
            }
        }
    }
}
