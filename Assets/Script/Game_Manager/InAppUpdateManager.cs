using System.Collections;
using UnityEngine;
using Google.Play.AppUpdate;
using Google.Play.Common;
using TMPro;

public class InAppUpdateManager : MonoBehaviour
{
    private AppUpdateManager appUpdateManager;
    public TextMeshProUGUI versionApp;

    void Start()
    {
        versionApp.text = "Version: " + Application.version;
        appUpdateManager = new AppUpdateManager();
        
        StartCoroutine(CheckForUpdate());
    }

    private IEnumerator CheckForUpdate()
    {
        //versionApp.text = "Version: " + Application.version;
        Debug.Log("Current Version: " + Application.version);

        var appUpdateInfoOperation = appUpdateManager.GetAppUpdateInfo();
        yield return appUpdateInfoOperation;

        if (appUpdateInfoOperation.IsSuccessful)
        {
            var appUpdateInfo = appUpdateInfoOperation.GetResult();

            // Kiểm tra xem có update không
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
}
