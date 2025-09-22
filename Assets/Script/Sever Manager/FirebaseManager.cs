using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Auth;
using System;
using System.Collections.Generic;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;
    private DatabaseReference dbRef;

    // UID hiện tại
    private string PlayerId
    {
        get
        {
            if (FirebaseAuth.DefaultInstance != null && FirebaseAuth.DefaultInstance.CurrentUser != null)
            {
                return FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            }
            return SystemInfo.deviceUniqueIdentifier;
        }
    }

    public static bool IsReady { get; private set; } = false;
    public static event Action OnFirebaseReady;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                dbRef = FirebaseDatabase.DefaultInstance.RootReference;
                Debug.Log("✅ Firebase connected!");

                IsReady = true;
                GoogleFirebaseAuth.Instance.FirebaseAuthStarts();
                

                // 🔹 Sync local khi kết nối lần đầu
                SyncLocalToFirebase();

                OnFirebaseReady?.Invoke();
            }
            else
            {
                Debug.LogError("❌ Firebase dependency error: " + task.Result);
            }
        });
        //Boots_Level.Instance.loadscene();
    }
    #region Bestime
    
    // ============================
    //  Save / Load BestTime
    // ============================
    public void SaveBestTime(string levelName, float time)
    {
        PlayerPrefs.SetFloat("BestTime_" + levelName, time);
        PlayerPrefs.Save();

        if (dbRef != null)
        {
            dbRef.Child("players").Child(PlayerId).Child("bestTimes").Child(levelName).SetValueAsync(time);
        }
    }

    public void LoadBestTime(string levelName, Action<float> callback)
    {
        if (dbRef == null)
        {
            float bestTime = PlayerPrefs.GetFloat("BestTime_" + levelName, -1f);
            callback(bestTime);
            return;
        }

        dbRef.Child("players").Child(PlayerId).Child("bestTimes").Child(levelName).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                float bestTime = float.Parse(task.Result.Value.ToString());
                callback(bestTime);
            }
            else
            {
                float localTime = PlayerPrefs.GetFloat("BestTime_" + levelName, -1f);
                callback(localTime);
            }
        });
    }
    #endregion
    #region Completed Level
    
    // ============================
    //  Save / Load Completed Levels
    // ============================
    public void MarkLevelComplete(string levelName)
    {
        PlayerPrefs.SetInt("Level_" + levelName + "_Completed", 1);
        PlayerPrefs.Save();

        if (dbRef != null)
        {
            dbRef.Child("players").Child(PlayerId).Child("completedLevels").Child(levelName).SetValueAsync(true);
        }
    }

    public void LoadCompletedLevels(Action<List<string>> callback)
    {
        if (dbRef == null)
        {
            List<string> completed = new List<string>();
            foreach (SceneList scene in Enum.GetValues(typeof(SceneList)))
            {
                if (PlayerPrefs.GetInt("Level_" + scene + "_Completed", 0) == 1)
                    completed.Add(scene.ToString());
            }
            callback(completed);
            return;
        }

        dbRef.Child("players").Child(PlayerId).Child("completedLevels").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            List<string> completed = new List<string>();
            if (task.IsCompleted && task.Result.Exists)
            {
                foreach (var child in task.Result.Children)
                {
                    if (child.Value != null && child.Value.ToString().ToLower() == "true")
                        completed.Add(child.Key);
                }
            }
            else
            {
                foreach (SceneList scene in Enum.GetValues(typeof(SceneList)))
                {
                    if (PlayerPrefs.GetInt("Level_" + scene + "_Completed", 0) == 1)
                        completed.Add(scene.ToString());
                }
            }
            callback(completed);
        });
    }
    #endregion
    #region Đồng Bộ Dữ Liệu
    
    // ============================
    //  Đồng bộ dữ liệu
    // ============================
    public void SyncLocalToFirebase()
    {
        if (dbRef == null) return;

        foreach (SceneList scene in Enum.GetValues(typeof(SceneList)))
        {
            string key = "Level_" + scene + "_Completed";
            if (PlayerPrefs.GetInt(key, 0) == 1)
            {
                dbRef.Child("players").Child(PlayerId).Child("completedLevels").Child(scene.ToString()).SetValueAsync(true);
            }

            string timeKey = "BestTime_" + scene;
            if (PlayerPrefs.HasKey(timeKey))
            {
                float localTime = PlayerPrefs.GetFloat(timeKey);
                dbRef.Child("players").Child(PlayerId).Child("bestTimes").Child(scene.ToString()).SetValueAsync(localTime);
            }
        }

        Debug.Log("🔄 Local progress synced to Firebase for: " + PlayerId);
    }

    // 🔹 Chuyển dữ liệu từ deviceId sang UID khi user login lần đầu
    public void MergeLocalToUser(string newUid)
    {
        string deviceId = SystemInfo.deviceUniqueIdentifier;

        if (deviceId == newUid) return; // cùng ID thì bỏ qua

        Debug.Log("🔄 Merging local data from deviceId into user UID...");

        foreach (SceneList scene in Enum.GetValues(typeof(SceneList)))
        {
            string timeKey = "BestTime_" + scene;
            if (PlayerPrefs.HasKey(timeKey))
            {
                float localTime = PlayerPrefs.GetFloat(timeKey);
                dbRef.Child("players").Child(newUid).Child("bestTimes").Child(scene.ToString()).SetValueAsync(localTime);
            }

            string completeKey = "Level_" + scene + "_Completed";
            if (PlayerPrefs.GetInt(completeKey, 0) == 1)
            {
                dbRef.Child("players").Child(newUid).Child("completedLevels").Child(scene.ToString()).SetValueAsync(true);
            }
        }
    }
    #endregion
}
