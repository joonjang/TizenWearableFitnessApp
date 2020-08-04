using FitWatch.Model;
using FitWatch.Singleton;
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

        //singleton seems unecessary as its a small project
        WatchSingletonClass watch = WatchSingletonClass.Instance;
        DataArraySingletonClass dataArray = DataArraySingletonClass.Instance;

        List<string> prevWorkoutName;
        List<string> prevReps;
        List<string> prevWeights;

        List<string> newWeightList;

        int globalWeightIndex = 0;

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

            newWeightList = new List<string>();


            // todo: debugging
            ParseJson();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // json logic -----------------------
        // *************************************************************************** refactor list
        // todo: refactor
        void DoneFunction()
        {
            //add rep and workout info so android listview can see it

            dataArray.DataArrayObject.DataArray.Add(prevReps);
            dataArray.DataArrayObject.DataArray.Add(prevWorkoutName);

            var model = new DataArrayModel()
            {
                DataArray = dataArray.DataArrayObject.DataArray
            };
            SendJsonString = JsonConvert.SerializeObject(model);

            SaveText = @"Save successful
Tap 'Upload' next time connected to phone to update spreadsheet";


        }

        // json from android
        public void ParseJson()
        {

            watch.WatchObject = JsonConvert.DeserializeObject<WatchModel>(MainViewModel.jsonString);

            // information of android based json info in list
            prevWorkoutName = new List<string>();
            prevReps = new List<string>();
            prevWeights = new List<string>();


            bool nextRep = false;

            foreach(string item in watch.WatchObject.Workouts)
            {
                // if subject doesnt have numbers, then its an exercise
                // second element will always be set
                // anything beyond is weights until condition of no numbers

                bool isNotWorkout = Regex.IsMatch(item, @"[0-9]+");

                // if name of workout
                if (!isNotWorkout)
                {
                    prevWorkoutName.Add(item);
                    // next list item will be the rep number
                    nextRep = true;
                }
                else if (nextRep)
                {
                    prevReps.Add(item);
                    // next list item will workouts and onwards
                    nextRep = false;
                }
                else
                {
                    prevWeights.Add(item);
                }

            }

            // start from the beggining workout after parse

            WorkoutTitletString = prevWorkoutName[0];
            RepString = prevReps[0];
            PrevWeightString = prevWeights[0];

            ///////////////////////////////////////////////////////////// OLD CODE ==============================

            //// add the starting data array week and day information
            //// first data array 
            //NewWeightList.Add(watch.WatchObject.Week);
            //NewWeightList.Add(watch.WatchObject.Day);
            //DataArrayList.Add(NewWeightList);

            //// clear for next array 
            //NewWeightList = new List<string>();

        }

        // logic for previous and next workout -------------------


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

        
        void NextWorkoutInfo()
        {
            // can go back
            NavigationButton(5);

            AddOrReplace(globalWeightIndex);

            // show next input view
            InputView(globalWeightIndex + 1);

            // every tap changes the index couunt
            globalWeightIndex++;
        }

        void PreviousWorkoutInfo()
        {
            // can go forward
            NavigationButton(4);

            if (!MasterUIVisible)
            {
                NavigationButton(6);
                goto ViewEntry;
            }

            AddOrReplace(globalWeightIndex);

        ViewEntry:

            InputView(globalWeightIndex - 1);

            globalWeightIndex--;
        }

        void NavigationButton(int i)
        {
            switch (i)
            {
                // final workout reached
                case 1:
                    DoneVisible = true;
                    MasterUIVisible = false;
                    break;
                // next disabled;
                case 2:
                    CanGoNext = false;
                    break;
                // back disabled
                case 3:
                    CanGoBack = false;
                    break;
                // next disabled;
                case 4:
                    CanGoNext = true;
                    break;
                // back disabled
                case 5:
                    CanGoBack = true;
                    break;
                // enable entry view
                case 6:
                    DoneVisible = false;
                    MasterUIVisible = true;
                    break;
            }
        }

        void AddOrReplace(int index)
        {

            if (newWeightList.ElementAtOrDefault(index) != null)
            {
                newWeightList[index] = NewWeightInt.ToString();
            }
            else
            {
                newWeightList.Add(NewWeightInt.ToString());
            }

            
        }

        bool WorkoutLabelInfo(int index)
        {
            // label text of previous weights, rep, and title count

            if((prevReps.Count - 1) < (index / watch.WatchObject.Sets.Count))
            {
                NavigationButton(2);
                return true;
            }

            int repAndWorkoutIndex = (int)Math.Truncate((double)(index / watch.WatchObject.Sets.Count));

            WorkoutTitletString = prevWorkoutName[repAndWorkoutIndex];
            RepString = prevReps[repAndWorkoutIndex];
            PrevWeightString = prevWeights[repAndWorkoutIndex];

            PrevWeightString = prevWeights[index];
            SetCount(index);

            return false;
        }

        void InputView(int index)
        {
            // reset to 0 if no previous information
            // show previous information if available
            // disable entry if its designated to be empty


            bool workoutDone = WorkoutLabelInfo(index);

            if (workoutDone)
            {
                NavigationButton(1);
                return;
            }


            if (index == 0)
            {
                // disable back if start of list
                NavigationButton(3);
            }

            string queueWeight = newWeightList.ElementAtOrDefault(index) != null ? newWeightList[index] : "0";

            if (prevWeights[index] == "8888")
            {
                // empty set
                UiVisible = false;
                RestVisible = true;

                NewWeightInt = 8888; 
            }
            else if(queueWeight == "0")
            {
                UiVisible = true;
                RestVisible = false;
                // if new entry in list
                HundredWeight = 0;
                TenWeight = 0;
                OneWeight = 0;
                NewWeightInt = 0;
            }
            else
            {
                UiVisible = true;
                RestVisible = false;

                // make the entry weight equal to last entered
                NewWeightInt = Int32.Parse(queueWeight);
                HundredWeight = (int)Math.Truncate((double)(newWeightInt / 100));
                try
                {
                    TenWeight = Int32.Parse(queueWeight[queueWeight.Length - 2].ToString());
                    OneWeight = Int32.Parse(queueWeight[queueWeight.Length - 1].ToString());
                }
                catch
                {
                    try
                    {
                        TenWeight = 0;
                        OneWeight = Int32.Parse(queueWeight[queueWeight.Length - 1].ToString());
                    }
                    catch
                    {
                        TenWeight = 0;
                        OneWeight = 0;
                    }
                }
            } 
        }

        void SetCount(int index)
        {
            // prevRep count should be 6, but it is varaible
            SetString = ((index % watch.WatchObject.Sets.Count) + 1).ToString();
        }

        /// OLD CODE ==========================================================================================================

        //void RegisterNewWeight(bool AddOrDelete)
        //{

        //    if (AddOrDelete)
        //    {
        //        NewWeightList.Add(NewWeightInt.ToString());
        //        // trailing weight to view previous and forward
        //        oldWeightInt = NewWeightInt;
        //    }
        //    else
        //    {
        //        if((NewWeightList.Count - 1 ) >= 0)
        //        {
        //            NewWeightList.RemoveAt(NewWeightList.Count - 1);   
        //        }
        //        //if (SavedNextWeight.ElementAtOrDefault(weightInt) != null)
        //        //{
        //        //    SavedNextWeight[weightInt] = oldWeightInt.ToString();
        //        //}
        //    }



        //    HistoryOfEntry();
        //}



        //void HistoryOfEntry()
        //{
        //    if(weightInt == SavedNextWeight.Count)
        //    {   
        //        SavedNextWeight.Add(oldWeightInt.ToString());
        //    }
        //    if(SavedNextWeight.ElementAtOrDefault(weightInt) != null)
        //    {

        //        SavedNextWeight[weightInt] = oldWeightInt.ToString();
        //    }
        //}
        //void NextWorkoutInfo()
        //{
        //    RegisterNewWeight(true);

        //    weightInt++;
        //    CanGoBack = true;

        //    // final user input entry
        //    if (weightInt == ((watch.WatchObject.Sets.Count * workouts.Count)))
        //    {
        //        workAndRepInt++;

        //        DoneVisible = true;
        //        MasterUIVisible = false;
        //        DataArrayList.Add(NewWeightList);
        //        CanGoNext = false;
        //        maxWorkCountReached = true;
        //        // add new weight here **********************************************************************************************************
        //        return;
        //    }

        //    WeightString = weights[weightInt];

        //    if (weightInt % 6 == 0)
        //    {
        //        workAndRepInt++;
        //        WorkoutTitletString = workouts[workAndRepInt];
        //        RepString = reps[workAndRepInt];

        //        // one array worth loaded up
        //        DataArrayList.Add(NewWeightList);
        //        NewWeightList = new List<string>();

        //    }
        //    // start of new weight entry
        //    if(SavedNextWeight.ElementAtOrDefault(weightInt) != null)
        //    {
        //        NewWeightInt = Int32.Parse(SavedNextWeight[weightInt]);
        //        ShowPrevious(SavedNextWeight[weightInt]);
        //    }
        //    else
        //    {
        //        NewWeightInt = 0;
        //        OneWeight = 0;
        //        TenWeight = 0;
        //        HundredWeight = 0;
        //    }

        //    if (weights[weightInt] == "8888")
        //    {
        //        UiVisible = false;
        //        RestVisible = true;
        //        NewWeightInt = 8888;
        //    }
        //    else
        //    {
        //        UiVisible = true;
        //        RestVisible = false;
        //    }
        //    SetCount();
        //}
        //bool maxWorkCountReached = false;
        //void PreviousWorkoutInfo()
        //{
        //    DoneVisible = false;
        //    MasterUIVisible = true;

        //    // populate views
        //    if (weightInt % 6 != 0)
        //    {
        //        ShowPrevious(NewWeightList[NewWeightList.Count - 1]);
        //    }


        //    RegisterNewWeight(false);


        //    if (weightInt % 6 == 0)
        //    {
        //        // dont delete index count if at the end 
        //        if (!(workAndRepInt <= 0))
        //        {
        //            workAndRepInt--;
        //        }
        //        WorkoutTitletString = workouts[workAndRepInt];
        //        RepString = reps[workAndRepInt];

        //        // remove previous array item
        //        // remove top array item
        //        // make removed list the current
        //        NewWeightList = DataArrayList[DataArrayList.Count - 1];
        //        ShowPrevious(SavedNextWeight[weightInt]);

        //        // dont delete if the very last of list
        //        if(!maxWorkCountReached) 
        //        { 
        //            DataArrayList.RemoveAt(DataArrayList.Count - 1);
        //            //NewWeightList.RemoveAt(NewWeightList.Count - 1);
        //        }
        //        maxWorkCountReached = false;
        //    }

        //    weightInt--;
        //    CanGoNext = true;

        //    if (weightInt <= 0)
        //    {

        //        CanGoBack = false;
        //    }

        //    WeightString = weights[weightInt];


        //    SetCount();
        //}

        //int oldWeightInt;

        //void ShowPrevious(string CurrentWeight)
        //{
        //    string currentWeightInList = CurrentWeight;

        //    // ui visibility
        //    if(currentWeightInList == "8888")
        //    {
        //        UiVisible = false;
        //        RestVisible = true;
        //    }
        //    else
        //    {  
        //        UiVisible = true;
        //        RestVisible = false;
        //        oldWeightInt = NewWeightInt;
        //        NewWeightInt = Int32.Parse(currentWeightInList);
        //        HundredWeight = (int)Math.Truncate((double)(newWeightInt / 100));
        //        try
        //        {
        //            TenWeight = Int32.Parse(currentWeightInList[currentWeightInList.Length - 2].ToString());
        //            OneWeight = Int32.Parse(currentWeightInList[currentWeightInList.Length - 1].ToString());
        //        }
        //        catch
        //        {
        //            try
        //            {
        //                TenWeight = 0;
        //                OneWeight = Int32.Parse(currentWeightInList[currentWeightInList.Length - 1].ToString());
        //            }
        //            catch
        //            {
        //                TenWeight = 0;
        //                OneWeight = 0;
        //            }
        //        }
        //    }


        //}



        // --------------------------------------------------- OLD CODE ==================================





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

        private string prevWeightString;
        public string PrevWeightString
        {
            get => prevWeightString;
            set
            {
                prevWeightString = "Previous: " + value;
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
    }
}
