using System;
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

namespace FitCompanion.ViewModel
{

    class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand RefreshCommand { get; }
        public ICommand SendMessageCommand { get; }
        public ICommand CloseConnectionCommand { get; }

        public IProviderService provider { get; set; }

        // json object
        string jsonString;
        WatchModel globalWatchModelHold;
        DataArrayModel dataArrayModelHold;


        public ICommand SubmitJsonCommand { get; }
        public ICommand GetJsonCommand { get; }

        public ObservableCollection<WorkoutModel> Workouts { get; set; }

        public MainViewModel()
        {
            provider = DependencyService.Get<IProviderService>();
            RefreshCommand = new Command(RefreshMsgSocket);
            SendMessageCommand = new Command(PackageForWatch);
            CloseConnectionCommand = new Command(CloseConnection);

            MessagingCenter.Subscribe<object>(Application.Current, "Update", (s) =>
            {
                RefreshMsgSocket();
            });

            SubmitJsonCommand = new Command(FullSendJsonToSheet);
            GetJsonCommand = new Command(GetSpreadsheetJson);



        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        public string DeviceSocketInfo
        {
            get => MainPage.DeviceInfoSocket;


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

        private bool connectedBool = false;
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

        void RefreshMsgSocket()
        {
            if (DeviceSocketInfo != "Empty")
            {
                ConnectedBool = true;
            }
            else
            {
                ConnectedBool = false;
            }

            OnPropertyChanged(nameof(ConnectedBool));

            MakeObjectFromJson();
        }


        // make object from json sent from watch
        void MakeObjectFromJson()
        {
            DataArrayModel dataArrayModel;

            // filter the received watch json empty 8888 to *
            string filterEmpty = ReceivedMsg.Replace("8888", "*");


            // for debugging
            //var filterEmpty = "{\"DataArray\":[[\"Week 1\",\"DAY 1\"],[\"0\",\"0\",\"0\",\"0\",\"0\",\"0\"],[\"0\",\"0\",\"0\",\"0\",\"*\",\"*\"],[\"0\",\"0\",\"0\",\"0\",\"*\",\"*\"],[\"0\",\"0\",\"0\",\"0\",\"*\",\"*\"],[\"4\",\"8\",\"4\",\"12\"],[\"Deadlift\",\"Chinups\",\"Rows\",\"Curls\"]]}";


            dataArrayModel = JsonConvert.DeserializeObject<DataArrayModel>(filterEmpty);

            // get workout name and reps then removes from DataArrayModel
            List<string> watchWorkoutTitle = dataArrayModel.DataArray[dataArrayModel.DataArray.Count - 1];
            dataArrayModel.DataArray.RemoveAt(dataArrayModel.DataArray.Count - 1);
            List<string> watchReps = dataArrayModel.DataArray[dataArrayModel.DataArray.Count - 1];
            dataArrayModel.DataArray.RemoveAt(dataArrayModel.DataArray.Count - 1);

            ConvertWatchJsonToWatchModel(watchWorkoutTitle, watchReps, dataArrayModel.DataArray);

            dataArrayModelHold = dataArrayModel;
            
        }

        void FullSendJsonToSheet()
        {
            if(dataArrayModelHold != null)
            {
                ApplyJsonToSheet(dataArrayModelHold);
            }
            
        }

        void ConvertWatchJsonToWatchModel(List<string> WorkoutNames, List<string> RepCount, List<List<string>> MasterArray)
        {
            WatchModel watchModel = new WatchModel();
            watchModel.Week = MasterArray[0][0];
            watchModel.Day = MasterArray[0][1];
            watchModel.Sets = new List<string>();
            watchModel.Workouts = new List<string>();

            // populate set count based on first workout amount
            for(int i = 0; i < MasterArray[1].Count; i++)
            {
                watchModel.Sets.Add("Set " + (i + 1));
            }

            // populate workout list past the first array which is week and day info
            for(int i = 1; i < MasterArray.Count; i++)
            {
                watchModel.Workouts.Add(WorkoutNames[i - 1]);
                watchModel.Workouts.Add(RepCount[i - 1]);
                watchModel.Workouts.AddRange(MasterArray[i]);
            }

            ListViewWorkouts(watchModel);

        }

        void CloseConnection()
        {
            provider.CloseConnection();
            //RefreshMsgSocket();
        }


        void SendMessage(string msg)
        {
            if(MainPage.DeviceInfoSocket != "Empty")
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
            else
            {
                ConnectWatchWarning();
            }
        }



        /// <summary>
        /// /////////////// JSON LOGIC ////////////////////////////// JSON LOGIC //////////////////// JSON LOGIC ///////////////// JSON LOGIC ////////////////////////////
        /// </summary>




        async void ApplyJsonToSheet(DataArrayModel dataArrayModel)
        {
            var client = new HttpClient();


            // todo: for debugging where this Json data is sent to google sheet, make it user entered url
            var uri = "https://script.google.com/macros/s/AKfycby2BGbNJwvzgqp4hay1CR0V3cznlND4u3Ra2-mysvdELCbO3II/exec";
            var jsonString = JsonConvert.SerializeObject(dataArrayModel);

            // from jsonString
            //string tmp = "{\"DataArray\":[[\"Week 0\",\"DAY 1\"],[\"69\",\"69\",\"420\",\"420\",\"42069\",\"6969\"],[\"69\",\"69\",\"420\",\"420\",\"42069\",\"6969\"],[\"69\",\"69\",\"420\",\"420\",\"42069\",\"6969\"],[\"69\",\"69\",\"420\",\"420\",\"42069\",\"6969\"]]}";
            //var requestContent = new StringContent(tmp);
            //

            var requestContent = new StringContent(jsonString);

            var result = await client.PostAsync(uri, requestContent);
            var resultContent = await result.Content.ReadAsStringAsync();
            ResponseModel response = JsonConvert.DeserializeObject<ResponseModel>(resultContent);
            ProcessResponse(response);
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

        private void ProcessResponse(ResponseModel responseModel)
        {
            Workouts = new ObservableCollection<WorkoutModel>();
            Workouts.Add(new WorkoutModel()
            {
                Weight = responseModel.Message
        });
            OnPropertyChanged(nameof(Workouts));
        }


        private string spreadsheetUrl;
        public string SpreadsheetUrl
        {
            get => spreadsheetUrl;
            set
            {
                spreadsheetUrl = value;
                OnPropertyChanged();
            }
        }


        private void GetSpreadsheetJson()
        {
            // todo: for debugging, make text user input and saved by preference
            SpreadsheetUrl = "https://docs.google.com/spreadsheets/d/1XWQNN76FJgt3X_213zwqblrOu2eSI0Tss1Zt1jPNLi0/edit#gid=524439697";
            



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
                Workouts = new ObservableCollection<WorkoutModel>();
                Workouts.Add(new WorkoutModel()
                {
                    Weight = "Unable to retrieve information"
                });
                OnPropertyChanged(nameof(Workouts));
                return;
            }

            
        }

        void ConnectWatchWarning()
        {
            Workouts = new ObservableCollection<WorkoutModel>();
            Workouts.Add(new WorkoutModel()
            {
                Weight = "Connect to watch first"
            });
            OnPropertyChanged(nameof(Workouts));
        }

        

        void PackageForWatch()
        {
            if (globalWatchModelHold == null)
            {
                ConnectWatchWarning();
                return;
            };

            var jsonString = JsonConvert.SerializeObject(globalWatchModelHold);

            SendMessage(jsonString);
        }

        void ListViewWorkouts(WatchModel watchModel)
        {
            Workouts = new ObservableCollection<WorkoutModel>();

            bool nextIsRep = false;
            bool registerWorkout = false;

            WorkoutModel tmpWorkout = new WorkoutModel();

            for(int i = 0; i < watchModel.Workouts.Count; i++)
            {
                

                if (!watchModel.Workouts[i].Any(char.IsDigit) && watchModel.Workouts[i] != "*")
                {
                    if(i != 0)
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

            int i = 0;

            // master loop bool, determines if loop is iterated
            bool workoutLoop = true;

            // triggers to recording list string into the WatchModel object
            bool workoutDayFound = false;
            bool registerToObject = false;

            while (workoutLoop)
            {
                i++;
                if(i >= jsonList.Count)
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

            // to convert stop to empty workout for watch
            if (watchModel.Workouts.Count != 0)
            {
                for (int x = 0; x < watchModel.Workouts.Count; x++)
                {
                    if (watchModel.Workouts[x].Contains("*"))
                    {
                        watchModel.Workouts[x] = "8888";
                    }
                }
            }
            else
            {
                Workouts.Add(new WorkoutModel()
                {
                    Weight = "Unable to find information"
                });
            }

            globalWatchModelHold = watchModel;

        }


        private int weekStepperVal = 1;
        public int WeekStepperVal
        {
            get => weekStepperVal;
            set
            {
                weekStepperVal = value;
                WeekStepperString = "Week " + weekStepperVal;
            }
        }

        private string weekStepperString = "Week 1";
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
    }
}
