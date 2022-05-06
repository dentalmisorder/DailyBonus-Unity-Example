using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AnotherMatch3.SaveSystem
{
    public class LoginDataHolder : MonoBehaviour
    {
        [SerializeField] private string jsonSavingFolder = "Data";
        [SerializeField] private string jsonSavingName = "DailyData";

        [SerializeField] private int baseDailyCoinNumber = 2;
        [SerializeField] private int secondDailyCoinMultiplier = 1;
        [SerializeField] private float previousDayScale = 60f;

        private string dataPath;
        private DailyLoginData dailyLoginData;
        private Seasons currentSeason;

        private const string jsonSuffix = ".json";

        public enum Seasons
        {
            Spring = 3,
            Summer = 6,
            Autumn = 9,
            Winter = 12
        }

        public event Action OnFirstLoginToday;
        public event Action<int> OnDailyCoinsUpdate;

        private void Start()
        {
            CheckPath(out dataPath);
            CheckDirectory(Path.Combine(dataPath, jsonSavingFolder));
            
            ParseDailyLoginData();
        }

        /// <summary>
        /// Checking path according to the device, 
        /// since streamingAssets cant work properly on mobile
        /// </summary>
        /// <param name="path">path to check (careful: takes in a REF param)</param>
        private void CheckPath(out string path)
        {
            #if UNITY_EDITOR
            path = Application.streamingAssetsPath;
            #elif UNITY_ANDROID || UNITY_IOS
            path = Application.persistentDataPath;
            #endif
        }

        /// <summary>
        /// Cache the data to be able use it through out game session
        /// without need of extra performance to request and de-serialization
        /// </summary>
        private void ParseDailyLoginData()
        {
            var fullPath = Path.Combine(dataPath, jsonSavingFolder, jsonSavingName+jsonSuffix);
            if (!File.Exists(fullPath))
            {
                CreateAndCacheJsonData();           //if there is no json means you probably login for the first time ever 
                OnFirstLoginToday?.Invoke();        //(theoretically, cause in real life we will get json-like fields from database, and if field is empty, means u arent registered)
                CheckCurrentSeasonCoins();
                return;
            }

            dailyLoginData = JsonConvert.DeserializeObject<DailyLoginData>(File.ReadAllText(fullPath));
            if(DateTime.Now > dailyLoginData.lastLoginTime.AddDays(1))
            {
                OnFirstLoginToday?.Invoke();

                DailyLoginData newDailyLoginData = new DailyLoginData();
                newDailyLoginData.lastLoginTime = DateTime.Now;
                newDailyLoginData.streak = DateTime.Now > dailyLoginData.lastLoginTime.AddDays(2) ? dailyLoginData.streak++ : 0;
                //should it be like if u skip one day streak will decrease to zero again
                //i guess its better since its DAILY <- bonus, but havent seen this in rules so idk

                CheckCurrentSeasonCoins();

                File.WriteAllText(fullPath, JsonConvert.SerializeObject(newDailyLoginData, Formatting.Indented));
            }
        }

        [ContextMenu("Create Cached JSON Data")]
        private void CreateAndCacheJsonData()
        {
            DailyLoginData dailyData = new DailyLoginData();
            dailyData.lastLoginTime = DateTime.Now; //lol JsonUtility cant serialize System.DateTime xd this is so cringe
            dailyData.streak = 0;

            CheckPath(out dataPath);                                        //to make sure (if running thru ContextMenu)
            CheckDirectory(Path.Combine(dataPath, jsonSavingFolder)); 
            var fullPath = Path.Combine(dataPath, jsonSavingFolder, jsonSavingName+jsonSuffix);

            File.WriteAllText(fullPath, JsonConvert.SerializeObject(dailyData, Formatting.Indented)); //Newtonsoft is a bro while JsonUtility isnt
            //File.WriteAllText(path, JsonUtility.ToJson(dailyData, true)); //this was not working, imagine JsonUtility cant serialize default struct bruh moment
        }

        private void CheckCurrentSeasonCoins()
        {
            var currentYear = DateTime.Now.Year;
            var currentMonth = DateTime.Now.Month;
            var currentDay = DateTime.Now.Day;

            if(currentMonth >= (int)Seasons.Spring && currentMonth < (int)Seasons.Summer) //Spring
            {
                OnDailyCoinsUpdate?.Invoke(CalculateDailyCoins(currentYear, currentMonth, currentDay, (int)Seasons.Spring));
            }
            else if(currentMonth >= (int)Seasons.Summer && currentMonth < (int)Seasons.Autumn) //Summer
            {
                OnDailyCoinsUpdate?.Invoke(CalculateDailyCoins(currentYear, currentMonth, currentDay, (int)Seasons.Summer));
            }
            else if(currentMonth >= (int)Seasons.Autumn && currentMonth < (int)Seasons.Winter) //Autumn
            {
                OnDailyCoinsUpdate?.Invoke(CalculateDailyCoins(currentYear, currentMonth, currentDay, (int)Seasons.Autumn));
            }
            else //Winter
            {
                OnDailyCoinsUpdate?.Invoke(CalculateDailyCoins(currentYear, currentMonth, currentDay, (int)Seasons.Winter));
            }
        }

        private int CalculateDailyCoins(int currentYear, int currentMonth, int currentDay, int seasonToCalculate)
        {
            List<int> allDailyCoins = new List<int>();
            int currentCoins = baseDailyCoinNumber;

            allDailyCoins.Add(currentCoins);

            for (int i = seasonToCalculate; i <= currentMonth; i++)
            {
                var dayToGo = currentMonth == i ? currentDay : DateTime.DaysInMonth(currentYear, i);
                for (int j = 1; j <= dayToGo; j++)
                {
                    currentCoins = allDailyCoins.Count >= 2 ? allDailyCoins[allDailyCoins.Count - 2] + Mathf.CeilToInt((float)allDailyCoins[allDailyCoins.Count-1] / 100 * previousDayScale) : currentCoins + secondDailyCoinMultiplier;
                    allDailyCoins.Add(currentCoins);
                    Debug.Log("Current coins: " +currentCoins);
                }
            }
            return allDailyCoins[allDailyCoins.Count-1];
        }

        // private int CalculateDailyCoinsSum(int currentYear, int currentMonth, int currentDay, int seasonToCalculate)
        // {
        //     var allDailyCoins = CalculateDailyCoins(currentYear, currentMonth, currentDay, (int)Seasons.Spring);

        //     int sumDailyCoins = 0;
        //     foreach (var coin in allDailyCoins)
        //         sumDailyCoins += coin;

        //     return sumDailyCoins;
        // }

        /// <summary>
        /// Well, basically it is what it says, checking if directory exist
        /// and if not we r creating directory with given path
        /// </summary>
        /// <param name="directoryPath">The path to check</param>
        private void CheckDirectory(string directoryPath)
        {
            if(Directory.Exists(directoryPath)) return;
            Directory.CreateDirectory(directoryPath);
        }
    }
}
