﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;
using FitCompanion.Model;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Net;
using static FitCompanion.Model.SpreadsheetModel;
using System.Linq;
using System.Collections.ObjectModel;
using Xamarin.Essentials;

namespace FitCompanion.ViewModel
{

    class MainViewModel : INotifyPropertyChanged
    {
        //public event PropertyChangedEventHandler PropertyChanged;
        public ICommand SendMessageCommand { get; }
        public ICommand CloseConnectionCommand { get; }
        public ICommand ModalUrlPageCommand { get; }
        public ICommand EditCommand { get; }

        public IProviderService provider { get; set; }


        // json object
        string jsonString;
        WatchModel globalWatchModelHold;

        public ICommand SubmitJsonCommand { get; }
        public ICommand GetJsonCommand { get; }



        public MainViewModel()
        {
            provider = DependencyService.Get<IProviderService>();
            SendMessageCommand = new Command(PackageForWatch);
            CloseConnectionCommand = new Command(CloseConnection);

            SubmitJsonCommand = new Command(ConvertWatchModelToDataArry);
            GetJsonCommand = new Command(GetSpreadsheetJson);
            ModalUrlPageCommand = new Command(ShowModalPage);
            EditCommand = new Command<WorkoutModel>((obj) => EdiWorkoutFunction(obj));

            ListViewHeaderMessage(Preferences.Get("LastSentInfo", ""));

            RefreshMsgSocket();
            MessagingCenter.Subscribe<object>(Application.Current, "Update", (s) =>
            {
                RefreshMsgSocket();
            });

            MessagingCenter.Subscribe<ModalUrlViewModel, string>(this, "SavedUrlInfo", (sender, args) =>
            {
                string[] urlArray = args.Split(',');
                SpreadsheetUrl = urlArray[0];
                ScriptUrl = urlArray[1];

                Preferences.Set("SavedUrl", SpreadsheetUrl);
                Preferences.Set("SavedScriptUrl", ScriptUrl);
            });

            MessagingCenter.Subscribe<ModalEditViewModel, WorkoutModel>(this, "EditWorkout", (sender, obj) =>
            {
                ChangedWorkoutCell(obj);
            });

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        async void ShowModalPage()
        {
            await Shell.Current.Navigation.PushModalAsync(new ModalUrlPage());
            MessagingCenter.Send<MainViewModel, string>(this, "SendingSavedUrl", SpreadsheetUrl + "," + ScriptUrl);
        }

        // where info from watch is received
        void RefreshMsgSocket()
        {
            if (DeviceSocketInfo != "Empty")
            {
                ConnectedBool = true;
                if (!string.IsNullOrEmpty(ReceivedMsg))
                {
                    MakeObjectFromJson();
                }
            }
            else
            {
                ConnectedBool = false;
            }

            OnPropertyChanged(nameof(ConnectedBool));


        }


        // make object from json sent from watch
        void MakeObjectFromJson()
        {
            DataArrayModel dataArrayModel;

            // filter the received watch json empty 8888 to *
            string filterEmpty = ReceivedMsg.Replace("8888", "*");


            dataArrayModel = JsonConvert.DeserializeObject<DataArrayModel>(filterEmpty);

            // get workout name and reps then removes from DataArrayModel
            List<string> watchWorkoutTitle = dataArrayModel.DataArray[dataArrayModel.DataArray.Count - 1];
            dataArrayModel.DataArray.RemoveAt(dataArrayModel.DataArray.Count - 1);
            List<string> watchReps = dataArrayModel.DataArray[dataArrayModel.DataArray.Count - 1];
            dataArrayModel.DataArray.RemoveAt(dataArrayModel.DataArray.Count - 1);

            WatchModel watchModel = ConvertWatchJsonToWatchModel(watchWorkoutTitle, watchReps, dataArrayModel.DataArray);

            globalWatchModelHold = watchModel;
            ListViewWorkouts(watchModel);


            UploadButtonBool = true;

        }

        void ConvertWatchModelToDataArry()
        {
            globalWatchModelHold = ConvertRestNumberCellToRestStar(globalWatchModelHold);

            DataArrayModel convertedFromWatch = new DataArrayModel();
            convertedFromWatch.DataArray = new List<List<string>>();

            List<string> WeekAndDay = new List<string>();
            WeekAndDay.Add(globalWatchModelHold.Week);
            WeekAndDay.Add(globalWatchModelHold.Day);

            convertedFromWatch.DataArray.Add(WeekAndDay);

            List<string> tmpWeights = new List<string>();


            // starts at workout string list index 2 to get past workout name and rep
            int index = 2;


            while (index < globalWatchModelHold.Workouts.Count)
            {
                tmpWeights.AddRange(globalWatchModelHold.Workouts.GetRange(index, globalWatchModelHold.Sets.Count));

                convertedFromWatch.DataArray.Add(tmpWeights);
                tmpWeights = new List<string>();

                // skips to next set count plus 2 because of name and rep 
                index = index + globalWatchModelHold.Sets.Count + 2;
            }

            ApplyJsonToSheet(convertedFromWatch);
        }

        async void ApplyJsonToSheet(DataArrayModel dataArrayModel)
        {
            var client = new HttpClient();
            var uri = ScriptUrl;
            var jsonString = JsonConvert.SerializeObject(dataArrayModel);


            var requestContent = new StringContent(jsonString);

            var result = await client.PostAsync(uri, requestContent);
            var resultContent = await result.Content.ReadAsStringAsync();
            ResponseModel response = JsonConvert.DeserializeObject<ResponseModel>(resultContent);
            ProcessResponse(response);
        }

        // successfully sent to spreadsheet response
        private void ProcessResponse(ResponseModel responseModel)
        {

            ListViewBodyMessage("");
            ListViewHeaderMessage(responseModel.Message);
        }

        // for when i receive watch json and show to list view
        // last watch received json function processed before manually pressing send to sheet
        WatchModel ConvertWatchJsonToWatchModel(List<string> WorkoutNames, List<string> RepCount, List<List<string>> MasterArray)
        {
            WatchModel watchModel = new WatchModel();
            watchModel.Week = MasterArray[0][0];
            watchModel.Day = MasterArray[0][1];
            watchModel.Sets = new List<string>();
            watchModel.Workouts = new List<string>();

            // populate set count based on first workout amount
            for (int i = 0; i < MasterArray[1].Count; i++)
            {
                watchModel.Sets.Add("Set " + (i + 1));
            }

            // populate workout list past the first array which is week and day info
            for (int i = 1; i < MasterArray.Count; i++)
            {
                watchModel.Workouts.Add(WorkoutNames[i - 1]);
                watchModel.Workouts.Add(RepCount[i - 1]);
                watchModel.Workouts.AddRange(MasterArray[i]);
            }

            ListViewHeaderMessage("Received from watch: " + watchModel.Week + ", " + watchModel.Day, 3);

            return watchModel;
        }

        void CloseConnection()
        {
            provider.CloseConnection();
            ListViewHeaderMessage("");
            ListViewBodyMessage("");
        }


        void SendMessage(string msg)
        {
            if (MainPage.DeviceInfoSocket != "Empty")
            {
                try
                {
                    provider.SendData(msg);
                    ListViewHeaderMessage("Sent to watch", 1);
                    Preferences.Set("LastSentInfo", "Last workout: Sheet " + WeekStepperVal + " (" + globalWatchModelHold.Week + ") " + globalWatchModelHold.Day);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("MainViewModel SendMessage error: " + ex);
                }
            }
            else
            {
                ListViewHeaderMessage("Connect to watch first", 2);
            }
        }




        private void GetSpreadsheetJson()
        {

            if (SpreadsheetUrl == null || SpreadsheetUrl == "")
            {
                ListViewHeaderMessage("URL is empty", 2);
                return;
            }


            int sheetPageNumber = WeekStepperVal;

            // get spreadsheet key url 
            Regex regex = new Regex(@"(?<=d/)(.*)(?=/)");
            MatchCollection matches = regex.Matches(SpreadsheetUrl);
            string spreadsheetCode = matches[0].Value;
            string jsonUrl = "https://spreadsheets.google.com/feeds/cells/" + spreadsheetCode + "/" + sheetPageNumber + "/public/full?alt=json";

            try
            {
                using (WebClient wc = new WebClient())
                {
                    jsonString = wc.DownloadString(jsonUrl);
                }


                Root jsonObject = JsonConvert.DeserializeObject<Root>(jsonString);

                List<string> cellInfoList = new List<string>();
                foreach (var cell in jsonObject.Feed.Entry)
                {
                    cellInfoList.Add(cell.Content.T);

                }
                FilterThroughJsonList(cellInfoList);

            }
            catch
            {
                ListViewHeaderMessage("Unable to retrieve information", 2);

            }

            SendWatchBool = true;


        }

        void ListViewBodyMessage(string message)
        {
            Workouts = new ObservableCollection<WorkoutModel>();
            if (!string.IsNullOrEmpty(message))
            {
                Workouts.Add(new WorkoutModel()
                {
                    Weight = message
                });

            }
            OnPropertyChanged(nameof(Workouts));
        }

        void ListViewHeaderMessage(string message, int i = 0)
        {
            ListViewHeader = message;
            switch (i)
            {
                // no issue
                case 0:
                    ListViewHeaderColor = "White";
                    break;
                // success
                case 1:
                    ListViewHeaderColor = "LightGreen";
                    break;
                // error
                case 2:
                    ListViewHeaderColor = "PaleVioletRed";
                    break;
                case 3:
                    ListViewHeaderColor = "DeepSkyBlue";
                    break;
            }
        }

        // first method for the process of sending to watch
        void PackageForWatch()
        {
            if (globalWatchModelHold == null)
            {
                ListViewHeaderMessage("Connect to watch first", 2);
                return;
            };

            globalWatchModelHold = ConvertStarCellToRestNumber(globalWatchModelHold);
            var jsonString = JsonConvert.SerializeObject(globalWatchModelHold);

            SendMessage(jsonString);
        }

        // receives the changed workout and updates current listview and sendable object
        void ChangedWorkoutCell(WorkoutModel watchObject)
        {
            for (int i = 0; i < Workouts.Count; i++)
            {
                if (Workouts[i].Name == watchObject.Name)
                {
                    Workouts.RemoveAt(i);
                    Workouts.Insert(i, watchObject);
                    CollectionToNewWatchModel(Workouts, globalWatchModelHold);
                    return;
                }
            }
        }

        // replace edited workout with this list

        void ListViewWorkouts(WatchModel watchModel)
        {
            Workouts = new ObservableCollection<WorkoutModel>();

            bool nextIsRep = false;
            bool registerWorkout = false;

            WorkoutModel tmpWorkout = new WorkoutModel();

            for (int i = 0; i < watchModel.Workouts.Count; i++)
            {


                if (!watchModel.Workouts[i].Any(char.IsDigit) && watchModel.Workouts[i] != "*")
                {
                    if (i != 0)
                    {
                        Workouts.Add(tmpWorkout);
                    }

                    tmpWorkout = new WorkoutModel();

                    registerWorkout = false;
                    tmpWorkout.Name = "Workout: " + watchModel.Workouts[i];
                    nextIsRep = true;
                    continue;
                }

                if (nextIsRep)
                {
                    tmpWorkout.Rep = "Reps: " + watchModel.Workouts[i];
                    registerWorkout = true;
                    nextIsRep = false;
                    continue;
                }
                if (registerWorkout)
                {
                    tmpWorkout.Weight += watchModel.Workouts[i] + " | ";
                }


                if (i == watchModel.Workouts.Count - 1)
                {

                    Workouts.Add(tmpWorkout);

                }
            }
            OnPropertyChanged(nameof(Workouts));
        }

        void CollectionToNewWatchModel(ObservableCollection<WorkoutModel> workoutList, WatchModel oldWatchModel)
        {
            WatchModel tmpWatchModel = new WatchModel();
            tmpWatchModel.Week = oldWatchModel.Week;
            tmpWatchModel.Day = oldWatchModel.Day;
            tmpWatchModel.Sets = oldWatchModel.Sets;
            tmpWatchModel.Workouts = new List<string>();

            for (int i = 0; i < workoutList.Count; i++)
            {
                string rawName = workoutList[i].Name.Replace("Workout: ", "");
                tmpWatchModel.Workouts.Add(rawName);
                string rawRep = workoutList[i].Rep.Replace("Reps: ", "");
                tmpWatchModel.Workouts.Add(rawRep);
                string[] workoutArray = workoutList[i].Weight.Split(new[] { " | " }, StringSplitOptions.RemoveEmptyEntries);
                tmpWatchModel.Workouts.AddRange(workoutArray);
            }

            globalWatchModelHold = ConvertStarCellToRestNumber(tmpWatchModel);

        }



        // filters the json from spreadsheet
        void FilterThroughJsonList(List<string> jsonList)
        {
            UserChosenDay = "DAY " + DayStepperVal;

            WatchModel watchModel = new WatchModel();
            watchModel.Sets = new List<string>();
            watchModel.Workouts = new List<string>();

            //the first array will always be the week information
            watchModel.Week = jsonList[0];
            // we already know which day is chosen based off user input
            watchModel.Day = UserChosenDay;
            ListViewHeaderMessage(watchModel.Week + ", " + watchModel.Day);
            int i = 0;

            // master loop bool, determines if loop is iterated
            bool workoutLoop = true;

            // triggers to recording list string into the WatchModel object
            bool workoutDayFound = false;
            bool registerToObject = false;

            while (workoutLoop)
            {
                i++;
                if (i >= jsonList.Count)
                {
                    break;
                }

                if (jsonList[i] == UserChosenDay)
                {
                    workoutDayFound = true;
                }
                if (jsonList[i].Contains("Set") && workoutDayFound)
                {
                    registerToObject = true;
                    watchModel.Sets.Add(jsonList[i]);
                }

                if (registerToObject && !jsonList[i].Contains("Set"))
                {
                    // if the next array is DAY then a new workout is found,
                    //  then object is populated and its time to end the loop
                    if (jsonList[i].Contains("DAY"))
                    {
                        workoutDayFound = false;
                        registerToObject = false;
                        workoutLoop = false;
                        break;
                    }

                    watchModel.Workouts.Add(jsonList[i]);

                }


            }


            ListViewWorkouts(watchModel);

            // to convert * to empty workout for watch
            // creates global watch if given watchModel is not empty
            if (watchModel.Workouts.Count != 0)
            {
                globalWatchModelHold = ConvertStarCellToRestNumber(watchModel);
            }
            else
            {
                ListViewHeaderMessage("Unable to find information", 2);
            }

        }

        WatchModel ConvertStarCellToRestNumber(WatchModel watchModel)
        {
            for (int x = 0; x < watchModel.Workouts.Count; x++)
            {
                if (watchModel.Workouts[x].Contains("*"))
                {
                    watchModel.Workouts[x] = "8888";
                }
            }

            return watchModel;
        }

        WatchModel ConvertRestNumberCellToRestStar(WatchModel watchModel)
        {
            for (int x = 0; x < watchModel.Workouts.Count; x++)
            {
                if (watchModel.Workouts[x].Contains("8888"))
                {
                    watchModel.Workouts[x] = "*";
                }
            }

            return watchModel;
        }

        async void EdiWorkoutFunction(WorkoutModel obj)
        {
            await Shell.Current.Navigation.PushModalAsync(new ModalEditPage());
            MessagingCenter.Send<MainViewModel, WorkoutModel>(this, "EditWorkoutInfo", obj);
        }




















        public ObservableCollection<WorkoutModel> Workouts { get; set; }

        public string DeviceSocketInfo
        {
            get => MainPage.DeviceInfoSocket;
        }

        private string userChosenDay;
        public string UserChosenDay
        {
            get => userChosenDay;
            set
            {
                userChosenDay = value;
                OnPropertyChanged();
            }
        }

        private string connectColor = "PaleVioletRed";
        public string ConnectedColor
        {
            get => connectColor;
            set
            {
                connectColor = value;
            }
        }

        private string connectedString = "Not Connected To Watch";
        public string ConnectedString
        {
            get => connectedString;
            set
            {
                connectedString = value;
            }
        }

        public string ReceivedMsg
        {
            get => MainPage.ReceivedMessage;

        }

        private string listViewHeader;
        public string ListViewHeader
        {
            get => listViewHeader;
            set
            {
                listViewHeader = value;
                OnPropertyChanged();
            }
        }

        private string listViewHeaderColor;
        public string ListViewHeaderColor
        {
            get => listViewHeaderColor;
            set
            {
                listViewHeaderColor = value;
                OnPropertyChanged();
            }
        }


        private int weekStepperVal = 1;
        public int WeekStepperVal
        {
            get => weekStepperVal;
            set
            {
                weekStepperVal = value;
                WeekStepperString = "Sheet " + weekStepperVal;
            }
        }

        private string weekStepperString = "Sheet 1";
        public string WeekStepperString
        {
            get => weekStepperString;
            set
            {
                weekStepperString = value;
                OnPropertyChanged();
            }
        }

        private int dayStepperVal = 1;
        public int DayStepperVal
        {
            get => dayStepperVal;
            set
            {
                dayStepperVal = value;
                DayStepperString = "DAY " + dayStepperVal;
            }
        }

        private string dayStepperString = "DAY 1";
        public string DayStepperString
        {
            get => dayStepperString;
            set
            {
                dayStepperString = value;
                OnPropertyChanged();
            }
        }

        private bool resultResponseBool = false;
        public bool ResultResponseBool
        {
            get => resultResponseBool;
            set
            {
                resultResponseBool = value;
                OnPropertyChanged();
            }
        }

        private string resultResponseText;
        public string ResultResponseText
        {
            get => resultResponseText;
            set
            {
                resultResponseText = value;
                OnPropertyChanged();
            }
        }

        private string spreadsheetUrl = Preferences.Get("SavedUrl", "");
        public string SpreadsheetUrl
        {
            get => spreadsheetUrl;
            set
            {
                spreadsheetUrl = value;

            }
        }

        private string scriptUrl = Preferences.Get("SavedScriptUrl", "");
        public string ScriptUrl
        {
            get => scriptUrl;
            set
            {
                scriptUrl = value;
            }
        }

        private bool sendWatchBool = false;
        public bool SendWatchBool
        {
            get => sendWatchBool;
            set
            {
                sendWatchBool = value;
                OnPropertyChanged();
            }
        }

        private bool uploadButtonBool = false;
        public bool UploadButtonBool
        {
            get => uploadButtonBool;
            set
            {
                uploadButtonBool = value;
                OnPropertyChanged();
            }
        }

        private static bool connectedBool = false;
        public bool ConnectedBool
        {
            get => connectedBool;
            set
            {
                connectedBool = value;
                if (connectedBool)
                {
                    ConnectedColor = "LightGreen";
                    ConnectedString = "Connected To Watch";
                }
                else
                {
                    ConnectedColor = "PaleVioletRed";
                    ConnectedString = "Not Connected To Watch";
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(ConnectedColor));
                OnPropertyChanged(nameof(ConnectedString));
            }
        }
    }
}
