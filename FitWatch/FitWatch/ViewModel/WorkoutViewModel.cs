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
        public Command<string> AddCommand { get; }
        public Command<string> SubtractCommand { get; }

        public WorkoutViewModel()
        {
            NextCommand = new Command(On_NextWorkoutInfo, (x) => CanGoNext);
            PreviousCommand = new Command(On_PreviousWorkoutInfo, (x) => CanGoBack);

            AddCommand = new Command<string>(IncreaseWeight);
            SubtractCommand = new Command<string>(SubtractWeight);

            MessagingCenter.Subscribe<object>(Application.Current, "Parse", (s) =>
            {
                ParseJson();
            });
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // json logic -----------------------

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

            // start from the beggining workout after parse

            WorkoutTitletString = workouts[0];
            RepString = reps[0];
            WeightString = weights[0];

            // add the starting data array week and day information
            // first data array 
            NewWeightList.Add(jsonObject.Week);
            NewWeightList.Add(jsonObject.Day);
            DataArrayList.Add(NewWeightList);

            // clear for next array 
            NewWeightList = new List<string>();

        }

        // logic for previous and next workout -------------------

        int workAndRepInt = 0;
        int weightInt = 0;

        private int newWeightInt;
        public int NewWeightInt
        {
            get => newWeightInt;
            set
            {
                newWeightInt = value;
                OnPropertyChanged();
            }
        }

        

        void IncreaseWeight(string i)
        {
            if (int.TryParse(i, out int num))
            {
                NewWeightInt += num;
                switch (num)
                {
                    case 100:
                        HundredWeight++;
                        break;
                    case 10:
                        TenWeight++;
                        break;
                    case 1:
                        OneWeight++;
                        break;

                }
            }
        }

        void SubtractWeight(string i)
        {
            if (int.TryParse(i, out int num))
            {
                NewWeightInt -= num;
                switch (num)
                {
                    case 100:
                        HundredWeight--;
                        break;
                    case 10:
                        TenWeight--;
                        break;
                    case 1:
                        OneWeight--;
                        break;

                }
            }
        }

        private int hundredWeight = 0;
        public int HundredWeight
        {
            get => hundredWeight;
            set
            {
                hundredWeight = value;
                OnPropertyChanged();
            }
        }

        private int tenWeight = 0;
        public int TenWeight
        {
            get => tenWeight;
            set
            {
                tenWeight = value;
                OnPropertyChanged();
            }
        }

        private int oneWeight = 0;
        public int OneWeight
        {
            get => oneWeight;
            set
            {
                oneWeight = value;
                OnPropertyChanged();
            }
        }

        private void On_NextWorkoutInfo(object obj)
        {
            NextWorkoutInfo();
        }

        private bool canGoNext = true;
        public bool CanGoNext
        {
            get => canGoNext;
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

        List<List<string>> DataArrayList = new List<List<string>>();
        List<string> NewWeightList = new List<string>();
        void RegisterNewWeight(bool AddOrDelete)
        {

            if (AddOrDelete)
            {
                NewWeightList.Add(NewWeightInt.ToString());
            }
            else
            {
                if((NewWeightList.Count - 1 ) >= 0)
                {
                    NewWeightList.RemoveAt(NewWeightList.Count - 1);
                }
            }
        }
        void NextWorkoutInfo()
        {
            RegisterNewWeight(true);

            weightInt++;
            CanGoBack = true;
            if (weightInt == ((jsonObject.Sets.Count * workouts.Count)-1))
            {
                
                CanGoNext = false;
            }

            WeightString = weights[weightInt];

            if (weightInt % 6 == 0)
            {
                workAndRepInt++;
                WorkoutTitletString = workouts[workAndRepInt];
                RepString = reps[workAndRepInt];

                // one array worth loaded up
                DataArrayList.Add(NewWeightList);
                NewWeightList = new List<string>();
                
            }

            // start of new weight entry
            NewWeightInt = 0;
            OneWeight = 0;
            TenWeight = 0;
            HundredWeight = 0;
            
        }

        void PreviousWorkoutInfo()
        {

            // populate views
            if (weightInt % 6 != 0)
            {
                ShowPrevious();
            }
                

            RegisterNewWeight(false);

            
            if (weightInt % 6 == 0)
            {
                if (!(workAndRepInt <= 0))
                {
                    workAndRepInt--;
                }
                WorkoutTitletString = workouts[workAndRepInt];
                RepString = reps[workAndRepInt];

                // remove previous array item
                // remove top array item
                // make removed list the current
                NewWeightList = DataArrayList[DataArrayList.Count - 1];
                ShowPrevious();
                DataArrayList.RemoveAt(DataArrayList.Count - 1);
                NewWeightList.RemoveAt(NewWeightList.Count - 1);
            }

            weightInt--;
            CanGoNext = true;

            if (weightInt <= 0)
            {
                
                CanGoBack = false;
            }

            WeightString = weights[weightInt];

            
            
        }

        void ShowPrevious()
        {
            string currentWeightInList = NewWeightList[NewWeightList.Count - 1];
            NewWeightInt = Int32.Parse(currentWeightInList);
            HundredWeight = (int)Math.Truncate((double)(newWeightInt / 100));
            try
            {
                TenWeight = Int32.Parse(currentWeightInList[currentWeightInList.Length - 2].ToString());
                OneWeight = Int32.Parse(currentWeightInList[currentWeightInList.Length - 1].ToString());
            }
            catch
            {
                try
                {
                    TenWeight = 0;
                    OneWeight = Int32.Parse(currentWeightInList[currentWeightInList.Length - 1].ToString());
                }
                catch
                {
                    TenWeight = 0;
                    OneWeight = 0;
                }
                
            }
            
        }

        // binding label information -----------------------------

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

        
    }
}
