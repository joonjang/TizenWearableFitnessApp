﻿using FitWatch.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace FitWatch.ViewModel
{
    class WorkoutViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public static string SendJsonString;

        WatchModel jsonObject;
        List<string> workouts;
        List<string> reps;
        List<string> weights;

        public Command NextCommand { get; }
        public Command PreviousCommand { get; }
        public Command<string> AddCommand { get; }
        public Command<string> SubtractCommand { get; }
        public Command DoneCommand { get; }

        public WorkoutViewModel()
        {
            NextCommand = new Command(On_NextWorkoutInfo, (x) => CanGoNext);
            PreviousCommand = new Command(On_PreviousWorkoutInfo, (x) => CanGoBack);

            AddCommand = new Command<string>(IncreaseWeight);
            SubtractCommand = new Command<string>(SubtractWeight);

            DoneCommand = new Command(DoneFunction);

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

        void DoneFunction()
        {
            var model = new DataArrayModel()
            {
                DataArray = DataArrayList
            };
            SendJsonString = JsonConvert.SerializeObject(model);

            SaveText = @"Save successful
Tap 'Upload' next time connected to phone to update spreadsheet";
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

        

        // two button ui logic ===============================

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

        private bool uiVisible = true;
        public bool UiVisible
        {
            get => uiVisible;
            set
            {
                uiVisible = value;
                OnPropertyChanged();
            }
        }

        private bool restVisible = false;
        public bool RestVisible
        {
            get => restVisible;
            set
            {
                restVisible = value;
                OnPropertyChanged();
            }
        }

        List<List<string>> DataArrayList = new List<List<string>>();
        List<string> NewWeightList = new List<string>();
        List<string> SavedNextWeight = new List<string>();
        void RegisterNewWeight(bool AddOrDelete)
        {

            if (AddOrDelete)
            {
                NewWeightList.Add(NewWeightInt.ToString());
                oldWeightInt = NewWeightInt;
            }
            else
            {
                if((NewWeightList.Count - 1 ) >= 0)
                {
                    NewWeightList.RemoveAt(NewWeightList.Count - 1);   
                }
                //if (SavedNextWeight.ElementAtOrDefault(weightInt) != null)
                //{
                //    SavedNextWeight[weightInt] = oldWeightInt.ToString();
                //}
            }


            
            HistoryOfEntry();
        }

        void SetCount()
        {
            SetString = ((weightInt % 6)+1).ToString();
        }

        void HistoryOfEntry()
        {
            if(weightInt == SavedNextWeight.Count)
            {   
                SavedNextWeight.Add(oldWeightInt.ToString());
            }
            if(SavedNextWeight.ElementAtOrDefault(weightInt) != null)
            {
                
                SavedNextWeight[weightInt] = oldWeightInt.ToString();
            }
        }
        void NextWorkoutInfo()
        {
            RegisterNewWeight(true);

            weightInt++;
            CanGoBack = true;

            // final user input entry
            if (weightInt == ((jsonObject.Sets.Count * workouts.Count)))
            {
                DoneVisible = true;
                MasterUIVisible = false;
                DataArrayList.Add(NewWeightList);
                CanGoNext = false;
                return;
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
            if(SavedNextWeight.ElementAtOrDefault(weightInt) != null)
            {
                NewWeightInt = Int32.Parse(SavedNextWeight[weightInt]);
                ShowPrevious(SavedNextWeight[weightInt]);
            }
            else
            {
                NewWeightInt = 0;
                OneWeight = 0;
                TenWeight = 0;
                HundredWeight = 0;
            }
            
            if (weights[weightInt] == "8888")
            {
                UiVisible = false;
                RestVisible = true;
                NewWeightInt = 8888;
            }
            else
            {
                UiVisible = true;
                RestVisible = false;
            }
            SetCount();
        }

        void PreviousWorkoutInfo()
        {
            DoneVisible = false;
            MasterUIVisible = true;

            // populate views
            if (weightInt % 6 != 0)
            {
                ShowPrevious(NewWeightList[NewWeightList.Count - 1]);
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
                ShowPrevious(NewWeightList[NewWeightList.Count - 1]);
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


            SetCount();
        }

        int oldWeightInt;

        void ShowPrevious(string CurrentWeight)
        {
            string currentWeightInList = CurrentWeight;

            // ui visibility
            if(currentWeightInList == "8888")
            {
                UiVisible = false;
                RestVisible = true;
            }
            else
            {  
                UiVisible = true;
                RestVisible = false;
                oldWeightInt = NewWeightInt;
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


        }

        // binding label information -----------------------------
        private string saveText;
        public string SaveText
        {
            get => saveText;
            set
            {
                saveText = value;
                OnPropertyChanged();
            }
        }

        private bool doneVisible = false;
        public bool DoneVisible
        {
            get => doneVisible;
            set
            {
                doneVisible = value;
                OnPropertyChanged();
            }
        }

        private bool masterUIVisbile = true;
        public bool MasterUIVisible
        {
            get => masterUIVisbile;
            set
            {
                masterUIVisbile = value;
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

        private string setString = "Set: 1";
        public string SetString
        {
            get => setString;
            set
            {
                setString = "Set: " + value;
                OnPropertyChanged();
            }
        }

    }
}
