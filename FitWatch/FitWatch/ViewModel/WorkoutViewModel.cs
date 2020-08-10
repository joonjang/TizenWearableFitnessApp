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
using System.Threading.Tasks;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Essentials;
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


            //// if previous info exists load ui and list
            List<string> loadedSavedList = LoadSave();
            if (loadedSavedList != null)
            {
                newWeightList = loadedSavedList;

                LoadStartEntry();

            }
            else
            {
                newWeightList = new List<string>();
            }


            // load previously saved json if it exists
            SendJsonString = Preferences.Get("SendJson", "");

            MessagingCenter.Subscribe<MainViewModel, string>(this, "Parse", (sender, arg) =>
            {
                ParseJson(arg);
            });
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // json logic -----------------------

        void LoadStartEntry()
        {
            NavigationButton(7);
            InputView(0);
        }

        List<string> LoadSave()
        {
            string SavedListJson = Preferences.Get("SavedList", "");
            string SavedWatchJson = Preferences.Get("WatchObject", "");
            List<string> SavedList = JsonConvert.DeserializeObject<List<string>>(SavedListJson);
            WatchModel SavedWatch = JsonConvert.DeserializeObject<WatchModel>(SavedWatchJson);

            watch.WatchObject = SavedWatch;
            if (watch.WatchObject != null)
            {
                PopulatePreviousWorkInfo();
            }
            return SavedList;
        }

        void DoneFunction()
        {

            dataArray.DataArrayObject = new DataArrayModel();
            dataArray.DataArrayObject.DataArray = new List<List<string>>();

            List<string> firstIndexObject = new List<string>();
            firstIndexObject.Add(watch.WatchObject.Week);
            firstIndexObject.Add(watch.WatchObject.Day);

            dataArray.DataArrayObject.DataArray.Add(firstIndexObject);

            List<List<string>> orderedWorkoutWeight = DivideWorkout();
            foreach (List<string> item in orderedWorkoutWeight)
            {
                dataArray.DataArrayObject.DataArray.Add(item);
            }


            dataArray.DataArrayObject.DataArray.Add(prevReps);
            dataArray.DataArrayObject.DataArray.Add(prevWorkoutName);

            var model = new DataArrayModel()
            {
                DataArray = dataArray.DataArrayObject.DataArray
            };
            SendJsonString = JsonConvert.SerializeObject(model);

            SaveText = @"Save successful
Tap 'Upload' next time connected to phone to update spreadsheet";

            ShowSavedMessage();

            Preferences.Set("SendJson", SendJsonString);

        }

        async void ShowSavedMessage()
        {
            await Task.Delay(6000);
            SaveText = "";
        }

        List<List<string>> DivideWorkout()
        {
            List<List<string>> tmpMaster = new List<List<string>>();
            List<string> tmpSegment = new List<string>();

            for (int i = 1; i <= newWeightList.Count; i++)
            {
                tmpSegment.Add(newWeightList[i - 1]);
                if (i % (watch.WatchObject.Sets.Count) == 0)
                {
                    tmpMaster.Add(tmpSegment);
                    tmpSegment = new List<string>();
                }
            }

            return tmpMaster;
        }

        // json from android
        public void ParseJson(string json)
        {
            globalWeightIndex = 0;
            NavigationButton(7);
            newWeightList = new List<string>();
            newWeightList.Add("0");
            watch.WatchObject = JsonConvert.DeserializeObject<WatchModel>(json);
            PopulatePreviousWorkInfo();
            InputView(0);
            
            MessagingCenter.Send<WorkoutViewModel, string>(this, "CurrentInfo", (watch.WatchObject.Week + ", " + watch.WatchObject.Day));
        }

        void PopulatePreviousWorkInfo()
        {
            // start from the beggining workout after parse
            // information of android based json info in list
            prevWorkoutName = new List<string>();
            prevReps = new List<string>();
            prevWeights = new List<string>();


            bool nextRep = false;

            foreach (string item in watch.WatchObject.Workouts)
            {
                // if subject doesnt have numbers, then its an exercise
                // second element will alws be set
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
            WorkoutTitletString = prevWorkoutName[0];
            RepString = prevReps[0];
            PrevWeightString = prevWeights[0];
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

            AddSubtractVisible();
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

            AddSubtractVisible();
        }

        void AddSubtractVisible()
        {


            if (HundredWeight <= 0)
            {
                HundredDownVisible = false;
            }
            else
            {
                HundredDownVisible = true;
            }
            if (TenWeight <= 0)
            {
                TenDownVisible = false;
            }
            else
            {
                TenDownVisible = true;
            }
            if (OneWeight <= 0)
            {
                OneDownVisible = false;
            }
            else
            {
                OneDownVisible = true;
            }

            if (TenWeight >= 9)
            {
                TenUpVisible = false;
            }
            else
            {
                TenUpVisible = true;
            }
            if (OneWeight >= 9)
            {
                OneUpVisible = false;
            }
            else
            {
                OneUpVisible = true;
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
                // show entry info
                case 7:
                    MasterUIVisible = true;
                    DoneVisible = false;
                    CanGoNext = true;
                    break;
            }
        }

        // logic that dictates whether new list item or if index item is overwritten
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
            SaveChanges();

        }

        bool WorkoutLabelInfo(int index)
        {
            // label text of previous weights, rep, and title count

            // checks if index is past amount of reps 
            if (globalWeightIndex + 1 >= watch.WatchObject.Workouts.Count - (prevReps.Count * 2))
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

        void SaveChanges()
        {

            // serialize and watch object containg set, rep, previous weight info list to json 
            string listJson = JsonConvert.SerializeObject(newWeightList);
            string savedWatchJson = JsonConvert.SerializeObject(watch.WatchObject);
            Preferences.Set("SavedList", listJson);
            Preferences.Set("WatchObject", savedWatchJson);
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
            else if (queueWeight == "0")
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
            AddSubtractVisible();

            /// prototype
        }

        void SetCount(int index)
        {
            // prevRep count should be 6, but it is varaible
            SetString = ((index % watch.WatchObject.Sets.Count) + 1).ToString();
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

        private bool masterUIVisbile = false;
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

        private bool canGoNext = false;
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

        private bool hundredDownVisible = false;
        public bool HundredDownVisible
        {
            get => hundredDownVisible;
            set
            {
                hundredDownVisible = value;
                OnPropertyChanged();
            }
        }
        private bool tenDownVisible = false;
        public bool TenDownVisible
        {
            get => tenDownVisible;
            set
            {
                tenDownVisible = value;
                OnPropertyChanged();
            }
        }
        private bool oneDownVisible = false;
        public bool OneDownVisible
        {
            get => oneDownVisible;
            set
            {
                oneDownVisible = value;
                OnPropertyChanged();
            }
        }
        private bool tenUpVisible = true;
        public bool TenUpVisible
        {
            get => tenUpVisible;
            set
            {
                tenUpVisible = value;
                OnPropertyChanged();
            }
        }
        private bool oneUpVisible = true;
        public bool OneUpVisible
        {
            get => oneUpVisible;
            set
            {
                oneUpVisible = value;
                OnPropertyChanged();
            }
        }
    }
}
