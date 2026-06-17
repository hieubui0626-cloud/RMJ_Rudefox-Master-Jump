using System.Collections;
using UnityEngine;
using Google.Play.AppUpdate;
using Google.Play.Common;
using TMPro;
using Firebase;
using Firebase.RemoteConfig;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor; // Dành cho nút test trong Inspector
#endif

public class InAppUpdateManager : MonoBehaviour
{
    private AppUpdateManager appUpdateManager;
    public TextMeshProUGUI versionApp;

    [Header("Firebase Config Keys")]
    public string latestVersionKey = "latest_version";
    public string updateUrlKey = "update_url";
    public string forceUpdateKey = "force_update";

    [Header("Test Mode")]
    public bool testInEditor = true; // Có thể tắt nếu không muốn chạy Firebase trong Editor

    private void Start()
    {
        if (versionApp != null)
            versionApp.text = "Version: " + Application.version;
        if (testInEditor)
        {
            Debug.Log("Editor build → Using Firebase Update check.");
            StartCoroutine(CheckForUpdate_Firebase());
        }
        else 
        {
            // 2. Nếu chạy trên máy thật, kiểm tra xem có phải bản build từ Google Play không
            if (IsRunningFromPlayStore())
            {
                Debug.Log("Running from Google Play → Using Play App Update");
                appUpdateManager = new AppUpdateManager();
                StartCoroutine(CheckForUpdate_CHPlay());
            }
            else
            {
                // Dự phòng: Nếu cài từ APK ngoài, các store khác -> Dùng Firebase
                Debug.Log("Running outside Play Store (Sideloaded APK) → Using Firebase Remote Config");
                StartCoroutine(CheckForUpdate_Firebase());
            }
        }
        
    }

    #region -------- CH Play Update --------
    private IEnumerator CheckForUpdate_CHPlay()
    {
        Debug.Log("Checking for Google Play updates...");
        var appUpdateInfoOperation = appUpdateManager.GetAppUpdateInfo();
        yield return appUpdateInfoOperation;

        if (appUpdateInfoOperation.IsSuccessful)
        {
            var appUpdateInfo = appUpdateInfoOperation.GetResult();

            if (appUpdateInfo.UpdateAvailability == UpdateAvailability.UpdateAvailable &&
                appUpdateInfo.IsUpdateTypeAllowed(AppUpdateOptions.ImmediateAppUpdateOptions()))
            {
                Debug.Log("Update available -> starting immediate update...");
                yield return StartCoroutine(StartImmediateUpdate(
                    appUpdateInfo,
                    AppUpdateOptions.ImmediateAppUpdateOptions()
                ));
            }
            else
            {
                Debug.Log("No update available.");
            }
        }
        else
        {
            Debug.LogError("CheckForUpdate failed: " + appUpdateInfoOperation.Error);
        }
    }

    private IEnumerator StartImmediateUpdate(AppUpdateInfo appUpdateInfo, AppUpdateOptions appUpdateOptions)
    {
        var startUpdateRequest = appUpdateManager.StartUpdate(appUpdateInfo, appUpdateOptions);
        yield return startUpdateRequest;

        if (startUpdateRequest.Error != AppUpdateErrorCode.NoError)
        {
            Debug.LogError("Update failed: " + startUpdateRequest.Error);
        }
        else
        {
            Debug.Log("Update started successfully.");
        }
    }
    #endregion

    #region -------- Firebase Remote Config Update --------
    private IEnumerator CheckForUpdate_Firebase()
    {
        Debug.Log("Checking for update via Firebase Remote Config...");
        var task = CheckForUpdateAsync();
        yield return new WaitUntil(() => task.IsCompleted);
    }

    private async Task CheckForUpdateAsync()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus != DependencyStatus.Available)
        {
            Debug.LogError("Firebase dependencies not ready: " + dependencyStatus);
            return;
        }

        await FirebaseRemoteConfig.DefaultInstance.FetchAsync(System.TimeSpan.FromSeconds(10));
        await FirebaseRemoteConfig.DefaultInstance.ActivateAsync();

        string latestVersion = FirebaseRemoteConfig.DefaultInstance.GetValue(latestVersionKey).StringValue;
        string updateUrl = FirebaseRemoteConfig.DefaultInstance.GetValue(updateUrlKey).StringValue;
        bool forceUpdate = FirebaseRemoteConfig.DefaultInstance.GetValue(forceUpdateKey).BooleanValue;

        Debug.Log($"Firebase version: {latestVersion}, Current: {Application.version}");

        if (IsNewerVersion(latestVersion, Application.version))
        {
            Debug.Log("🔥 Có bản cập nhật mới từ Firebase!");
            if (forceUpdate)
            {
                Debug.Log("Force update enabled → mở URL ngay.");
                Application.OpenURL(updateUrl);
            }
            else
            {
                ShowUpdatePopup(updateUrl, latestVersion);
            }
        }
        else
        {
            Debug.Log("✅ Game đang ở phiên bản mới nhất.");
        }
    }

    private bool IsNewerVersion(string remote, string local)
    {
        try
        {
            System.Version remoteV = new System.Version(remote);
            System.Version localV = new System.Version(local);
            return remoteV.CompareTo(localV) > 0;
        }
        catch
        {
            Debug.LogWarning($"Version parse error. Remote: {remote}, Local: {local}");
            return false;
        }
    }

    private void ShowUpdatePopup(string url, string newVer)
    {
        Debug.Log($"📢 Có bản cập nhật mới ({newVer})! Tải tại: {url}");
        // Ở đây bạn có thể hiển thị popup UI thật
        // Ví dụ:
        // updatePopup.SetActive(true);
        // updateButton.onClick.AddListener(() => Application.OpenURL(url));
    }
    #endregion

    #region -------- Utility --------
    private bool IsRunningFromPlayStore()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    try
    {
        using (var playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (var currentActivity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                using (var packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager"))
                {
                    string packageName = currentActivity.Call<string>("getPackageName");
                    // Lấy tên của ứng dụng đã cài đặt game này
                    string installerName = packageManager.Call<string>("getInstallerPackageName", packageName);
                    
                    // Nếu nguồn cài đặt là com.android.vending -> Đích thị là từ Google Play Store
                    return installerName == "com.android.vending";
                }
            }
        }
    }
    catch (System.Exception e)
    {
        Debug.LogWarning("Không lấy được nguồn cài đặt: " + e.Message);
        return false;
    }
#else
        return false;
#endif
    }
    #endregion

#if UNITY_EDITOR
    // 🔘 Thêm nút test trong Inspector để chạy Firebase update check thủ công
    [CustomEditor(typeof(InAppUpdateManager))]
    public class InAppUpdateManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            InAppUpdateManager script = (InAppUpdateManager)target;

            if (GUILayout.Button("🧩 Check Firebase Update Now"))
            {
                script.StartCoroutine(script.CheckForUpdate_Firebase());
            }
        }
    }
#endif
}
