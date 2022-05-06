using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Collections;

namespace AnotherMatch3.SaveSystem
{
    [Obsolete("Deprecated since need credentials and SDK to work, and firebase needs 1-2 days to set-up database)")]
    public class OldLoginDataHolder : MonoBehaviour
    {
        [SerializeField] private string jsonSavingFolder = "Data";
        [SerializeField] private string jsonSavingName = "DailyData";

        private string dataPath;
        private DailyLoginData dailyLoginData;

        private const string uri = "https://farmersdelight.online/DailyData.json"; //sry i dont have MySQL or Firebase to get proper database
        private const string jsonSuffix = ".json";

        public event Action OnFirstLogin;

        private void Start()
        {
            CheckPath(out dataPath);
            CheckDirectory(Path.Combine(dataPath, jsonSavingFolder));
            
            StartCoroutine(ParseDailyLoginData());
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
        private IEnumerator ParseDailyLoginData()
        {
            var fullPath = Path.Combine(dataPath, jsonSavingFolder, jsonSavingName+jsonSuffix);
            
            UnityWebRequest downloadRequest = new UnityWebRequest(uri, "GET");
            downloadRequest.downloadHandler = new DownloadHandlerFile(fullPath);
            yield return downloadRequest.SendWebRequest();

            ParseJsonData(fullPath);
        }

        private void ParseJsonData(string fileToUpload)
        {
            string jsonData = Path.Combine(dataPath, jsonSavingFolder, jsonSavingName+jsonSuffix);
            dailyLoginData = JsonConvert.DeserializeObject<DailyLoginData>(File.ReadAllText(jsonData));

            if(DateTime.Now < dailyLoginData.lastLoginTime.AddDays(1))
            {
                OnFirstLogin?.Invoke();

                var newDailyLoginData = new DailyLoginData();
                newDailyLoginData.lastLoginTime = DateTime.Now;
                newDailyLoginData.streak = dailyLoginData.lastLoginTime.AddDays(2) < DateTime.Now ? dailyLoginData.streak++ : 0; 
                //if u skip one day streak is at zero again
                //i guess its better since its DAILY <- bonus, but havent seen this in rules so idk

                File.WriteAllText(jsonData, JsonConvert.SerializeObject(newDailyLoginData, Formatting.Indented));

                UnityWebRequest uploadRequest = new UnityWebRequest(uri, "POST");
                uploadRequest.uploadHandler = new UploadHandlerFile(fileToUpload);
                Debug.Log(uploadRequest.result);
            }
        }

        [ContextMenu("Create Cached JSON Data")]
        private void CreateAndCacheJsonData()
        {
            //TODO: parse data first, if there is no data create an empty blueprint and store DateTime.Now
            //OTHERWISE, check for the streak and ofc cache class in case if i will need it again, 
            //so i dont need to create a request and parse it again

            //TODO: Make a unitywebrequest to get the dailyLoginData from server so the player cant change it locally

            DailyLoginData dailyData = new DailyLoginData();
            dailyData.lastLoginTime = DateTime.Now; //lol JsonUtility cant serialize System.DateTime xd this is so cringe
            dailyData.streak = 0;

            CheckPath(out dataPath);                                        //to make sure (if running thru ContextMenu)
            CheckDirectory(Path.Combine(dataPath, jsonSavingFolder)); 
            var fullPath = Path.Combine(dataPath, jsonSavingFolder, jsonSavingName+jsonSuffix);

            File.WriteAllText(fullPath, JsonConvert.SerializeObject(dailyData, Formatting.Indented)); //Newtonsoft is a bro while JsonUtility isnt
            //File.WriteAllText(path, JsonUtility.ToJson(dailyData, true)); //this was not working, imagine JsonUtility cant serialize default struct bruh moment
        }

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
