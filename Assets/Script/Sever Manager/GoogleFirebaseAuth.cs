using System.Collections;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Google;
using UnityEngine;

public class GoogleFirebaseAuth : MonoBehaviour
{
    public FirebaseAuth auth;
    public FirebaseUser user;
    private GoogleSignInConfiguration googleConfig;

    public static GoogleFirebaseAuth Instance;
    public GameObject Lost_Connect_Pannel;

    private bool isSigningOut = false;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        googleConfig = new GoogleSignInConfiguration
        {
            WebClientId = "870980917346-ur3o530eo7olt3o30m91gfqnq1ebgall.apps.googleusercontent.com",
            RequestIdToken = true,
            RequestEmail = true
        };
    }

    public void FirebaseAuthStarts()
    {
        auth = FirebaseAuth.DefaultInstance;
        user = auth.CurrentUser;

        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);

        Debug.Log("✅ Firebase Auth initialized");
        StartCoroutine(CheckNetworkRoutine());
    }

    public bool IsSignedIn() => auth != null && auth.CurrentUser != null;

    private void AuthStateChanged(object sender, System.EventArgs e)
    {
        user = auth.CurrentUser;

        if (IsSignedIn())
        {
            Debug.Log("✅ User signed in: " + user.DisplayName);
            Boots_Level.Instance.Userid.text = "User Id: " + user.DisplayName;
            Boots_Level.Instance.SignOutButton.SetActive(true);
        }
        else if (!isSigningOut)
        {
            Debug.Log("⚠ User signed out");
            Boots_Level.Instance.Userid.text = "User Id: Guest";
            StartCoroutine(SignOutWaitTime());
        }
    }

    private IEnumerator CheckNetworkRoutine()
    {
        while (true)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Lost_Connect_Pannel?.SetActive(true);
                //if (GameManager.Instance != null) GameManager.Instance.isTiming = false;
            }
            else
            {
                Lost_Connect_Pannel?.SetActive(false);
                //if (GameManager.Instance != null) GameManager.Instance.isTiming = true;
            }
            yield return new WaitForSeconds(2f);
        }
    }

    public void SignInWithGoogle()
    {
        GoogleSignIn.Configuration = googleConfig;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;

        var signInTask = GoogleSignIn.DefaultInstance.SignIn();

        signInTask.ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled) { Debug.LogWarning("⚠️ Google Sign-In canceled."); return; }
            if (task.IsFaulted) { Debug.LogError("Google Sign-In failed: " + task.Exception); return; }

            GoogleSignInUser googleUser = task.Result;
            Debug.Log("✅ Google Sign-In success: " + googleUser.DisplayName);

            var credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);
            auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(authTask =>
            {
                if (authTask.IsCanceled || authTask.IsFaulted)
                {
                    Debug.LogError("❌ Firebase login error: " + authTask.Exception);
                    return;
                }

                user = authTask.Result;
                Debug.LogFormat("🎉 Firebase user signed in: {0} ({1})", user.DisplayName, user.UserId);

                FirebaseManager.Instance.MergeLocalToUser(user.UserId);
            });
        });
    }

    public void SignOut()
    {
        StartCoroutine(HandleSignOutRoutine());
    }

    private IEnumerator HandleSignOutRoutine()
    {
        isSigningOut = true;

        if (auth != null) auth.SignOut();
        GoogleSignIn.DefaultInstance.SignOut();
        user = null;

        Debug.Log("🚪 User signed out, waiting for token reset...");
        yield return new WaitForSeconds(2f); // tránh sign-in lại ngay lập tức

        isSigningOut = false;
    }

    private IEnumerator SignOutWaitTime()
    {
        yield return new WaitForSeconds(1f);
        Boots_Level.Instance.boots_done = false;
        Boots_Level.Instance.GoogleSignButton();
        Ads_Manager.Instance.HideBanner();
    }
}
