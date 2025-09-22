using System;
using System.Collections;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Google;
using UnityEngine;
using UnityEngine.SceneManagement;
//"870980917346-ur3o530eo7olt3o30m91gfqnq1ebgall.apps.googleusercontent.com"
public class GoogleFirebaseAuth : MonoBehaviour
{
    public FirebaseAuth auth;
    public FirebaseUser user;
    private GoogleSignInConfiguration googleConfig;

    public static GoogleFirebaseAuth Instance;
    public GameObject Lost_Connect_Pannel;
    //public SceneList SignOut;

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

        // ⚡ Cấu hình Google Sign-In
        googleConfig = new GoogleSignInConfiguration
        {
            WebClientId = "870980917346-ur3o530eo7olt3o30m91gfqnq1ebgall.apps.googleusercontent.com", // lấy từ Firebase Console
            RequestIdToken = true,
            RequestEmail = true
        };
    }

    public void FirebaseAuthStarts()
    {
        // ⚡ Khởi tạo Firebase Auth
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                user = auth.CurrentUser;
                Debug.Log("✅ Firebase Auth initialized");
                Boots_Level.Instance.GoogleSignButton();
            }
            else
            {
                Debug.LogError("❌ Firebase dependency error: " + task.Result);
            }
        });
    }

    void Start()
    {
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null); // check ngay lần đầu
        StartCoroutine(CheckNetworkRoutine());
    }

    void OnDestroy()
    {
        if (auth != null)
        {
            auth.StateChanged -= AuthStateChanged;
        }
    }

    

    // 🔹 Kiểm tra người chơi đã đăng nhập chưa
    public bool IsSignedIn()
    {
        return auth != null && auth.CurrentUser != null;
    }

    private void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser == null)
        {
            Debug.Log("⚠ User signed out hoặc mất session. Returning to Login Scene...");
            StartCoroutine(SignOutWaitlTime());
        }
        else
        {
            Lost_Connect_Pannel.SetActive(false);
        }
    }

    private IEnumerator CheckNetworkRoutine()
    {
        while (true)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                // Chỉ hiển thị panel mất kết nối, KHÔNG load lại scene
                Lost_Connect_Pannel.SetActive(true);
            }
            else
            {
                Lost_Connect_Pannel.SetActive(false);
            }

            yield return new WaitForSeconds(2f); // check mỗi 2 giây
        }
    }

    // Gọi hàm này khi nhấn nút "Login with Google"
    public void SignInWithGoogle()
    {
        GoogleSignIn.Configuration = googleConfig;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;

        var signInTask = GoogleSignIn.DefaultInstance.SignIn();

        signInTask.ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogWarning("⚠️ Google Sign-In canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("❌ Google Sign-In error: " + task.Exception);
                return;
            }

            GoogleSignInUser googleUser = task.Result;
            Debug.Log("✅ Google Sign-In success: " + googleUser.DisplayName);

            // 🔹 Lấy credential từ Google token
            Credential credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);

            auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(authTask =>
            {
                if (authTask.IsCanceled || authTask.IsFaulted)
                {
                    Debug.LogError("❌ Firebase login error: " + authTask.Exception);
                    return;
                }

                user = authTask.Result;
                Debug.LogFormat("🎉 Firebase user signed in: {0} ({1})", user.DisplayName, user.UserId);

                // 🔹 Đồng bộ dữ liệu local sang Firebase khi login lần đầu
                FirebaseManager.Instance.MergeLocalToUser(user.UserId);
                
            });
        });
        

    }

    public void SignOut()
    {
        if (auth != null)
        {
            auth.SignOut();
        }
        GoogleSignIn.DefaultInstance.SignOut();
        user = null;
        
        
        
    }
   
    
    IEnumerator SignOutWaitlTime()
    {
        LevelTransition.Instance.EndTransition();
        yield return new WaitForSeconds(1f);
        Boots_Level.Instance.boots_done = false;
        SceneManager.LoadScene(Boots_Level.Instance.sceneLogin.ToString());
    }
    
}