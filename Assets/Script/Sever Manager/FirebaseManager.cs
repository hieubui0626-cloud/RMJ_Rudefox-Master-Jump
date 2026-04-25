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

    /// <summary>
    /// UID hiện tại: ưu tiên Firebase UID nếu đã login,
    /// fallback về guest_xxx khi chưa login.
    /// </summary>
    private string PlayerId
    {
        get
        {
            if (FirebaseAuth.DefaultInstance != null)
            {
                var user = FirebaseAuth.DefaultInstance.CurrentUser;
                if (user != null) return user.UserId;
            }
            return "guest_" + SystemInfo.deviceUniqueIdentifier;
        }
    }

    public static bool IsReady { get; private set; } = false;
    public static event Action OnFirebaseReady;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
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
                SyncLocalToFirebase();

                OnFirebaseReady?.Invoke();
            }
            else
            {
                Debug.LogError("❌ Firebase dependency error: " + task.Result);
            }

        });
    }

    #region Best Time
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

        dbRef.Child("players").Child(PlayerId).Child("bestTimes").Child(levelName)
        .GetValueAsync().ContinueWithOnMainThread(task =>
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

    #region Completed Levels
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

        dbRef.Child("players").Child(PlayerId).Child("completedLevels").GetValueAsync()
        .ContinueWithOnMainThread(task =>
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

    #region Sync & Merge
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

    public void MergeLocalToUser(string newUid)
    {
        string guestId = "guest_" + SystemInfo.deviceUniqueIdentifier;
        if (guestId == newUid) return;

        Debug.Log("🔄 Merging local data from guest into user UID...");

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

    #region Token System
    public void GetTotalTokens(Action<int> callback)
    {
        if (dbRef == null)
        {
            int cached = PlayerPrefs.GetInt("CachedTokens", 0);
            callback?.Invoke(cached);
            return;
        }

        dbRef.Child("players").Child(PlayerId).Child("tokens").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                int total = int.Parse(task.Result.Value.ToString());
                PlayerPrefs.SetInt("CachedTokens", total);
                PlayerPrefs.Save();
                callback?.Invoke(total);
            }
            else
            {
                Debug.LogWarning("⚠️ Không lấy được token từ Firebase → dùng cache");
                int cached = PlayerPrefs.GetInt("CachedTokens", 0);
                callback?.Invoke(cached);
            }
        });
    }

    public void UpdateTotalTokens(int amountToAdd, Action<int> callback = null)
    {
        GetTotalTokens(total =>
        {
            int newTotal = total + amountToAdd;

            if (dbRef != null)
            {
                dbRef.Child("players").Child(PlayerId).Child("tokens").SetValueAsync(newTotal);
            }

            PlayerPrefs.SetInt("CachedTokens", newTotal);
            PlayerPrefs.Save();

            callback?.Invoke(newTotal);
        });
    }

    public void SetTotalTokens(int newTotal, Action callback = null)
    {
        if (dbRef != null)
        {
            dbRef.Child("players").Child(PlayerId).Child("tokens").SetValueAsync(newTotal);
        }

        PlayerPrefs.SetInt("CachedTokens", newTotal);
        PlayerPrefs.Save();

        callback?.Invoke();
    }
    #endregion

    #region SkinSystem
    public void SaveUnlockedSkins(List<string> unlockedList)
    {
        Dictionary<string, object> unlockedMap = new Dictionary<string, object>();
        foreach (var id in unlockedList)
            unlockedMap[id] = true;

        dbRef.Child("players").Child(PlayerId)
            .Child("skins").Child("unlocked")
            .SetValueAsync(unlockedMap);
    }
    public void SaveEquippedSkins(Dictionary<SkinType, string> equippedMap)
    {
        Dictionary<string, object> map = new Dictionary<string, object>();
        foreach (var kv in equippedMap)
            map[kv.Key.ToString()] = kv.Value;

        dbRef.Child("players").Child(PlayerId)
            .Child("skins").Child("equipped")
            .SetValueAsync(map);
    }

    public void LoadPlayerSkinData(Action<int, List<string>, Dictionary<SkinType, string>> callback)
    {
        dbRef.Child("players").Child(PlayerId).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted && !task.IsFaulted)
            {
                DataSnapshot snap = task.Result;
                int tokens = 0;
                List<string> unlocked = new List<string>();
                Dictionary<SkinType, string> equipped = new Dictionary<SkinType, string>();

                if (snap.Child("tokens").Exists)
                    tokens = int.Parse(snap.Child("tokens").Value.ToString());

                if (snap.Child("skins").Child("unlocked").Exists)
                {
                    foreach (var child in snap.Child("skins").Child("unlocked").Children)
                        unlocked.Add(child.Key);
                }

                if (snap.Child("skins").Child("equipped").Exists)
                {
                    foreach (var child in snap.Child("skins").Child("equipped").Children)
                    {
                        SkinType type;
                        if (Enum.TryParse(child.Key, out type))
                            equipped[type] = child.Value.ToString();
                    }
                }

                callback(tokens, unlocked, equipped);
            }
            else
            {
                Debug.LogWarning("Failed to load skin data from Firebase");
                callback(0, new List<string>(), new Dictionary<SkinType, string>());
            }
        });
    }

    #endregion
}
