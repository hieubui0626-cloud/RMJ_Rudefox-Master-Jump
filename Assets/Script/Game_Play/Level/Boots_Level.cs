using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Boots_Level : MonoBehaviour
{
    
    public static Boots_Level Instance;
    public SceneList sceneToLoad;
    public SceneList sceneLogin;
    public GameObject LoginButton;
    public TextMeshProUGUI Userid;
    public bool boots_done;
    public bool Test;
    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GoogleSignButton()
    {
        
        if(Test)
        {
            loadsceneUnity();
            boots_done = true;
        }
        else
        {
            if (!GoogleFirebaseAuth.Instance.IsSignedIn())
            {

                LoginButton.SetActive(true);
                SceneManager.LoadScene(sceneLogin.ToString());
            }
            else
            {

                LoginButton.SetActive(false);
                SceneManager.LoadScene(sceneToLoad.ToString());

            }
        }
        

        
        
        


    }
    
    public void Update()
    {
        //if (GoogleFirebaseAuth.Instance.user != null) return;
        if(!boots_done)
        {
            Debug.LogWarning("check");
            loadscene();

        }
    }
    

    public void loadsceneUnity()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name != "Boot_Scene") return;
        if (!Ads_Manager.Instance.ads_Active) return;
        if (!FirebaseManager.IsReady) return;
        SceneManager.LoadScene(sceneToLoad.ToString());
    }
    public void loadloginscene()
    {
        LoginButton.SetActive(true);
        SceneManager.LoadScene(sceneLogin.ToString());
    }
    public void loadscene()
    {
 
        
        
        Scene currentScene = SceneManager.GetActiveScene();
        
        //if(currentScene.name == "Boot_Scene" || currentScene.name == "Login_Scene") return;
        if (!Ads_Manager.Instance.ads_Active) return;
        if (FirebaseManager.IsReady)
        {
            //SceneManager.LoadScene(sceneToLoad.ToString());
            Debug.LogWarning("load scene");
            if (GoogleFirebaseAuth.Instance.IsSignedIn())
            {
                
                Userid.text = "User Id:" + GoogleFirebaseAuth.Instance.user.DisplayName;
                Debug.LogWarning("load scene");
                //Debug.Log("[AdManager] Firebase ready & user logged in -> Load GameScene");
                SceneManager.LoadScene(sceneToLoad.ToString());
                LoginButton.SetActive(false);
                boots_done = true;
            }
            else
            {
                //Debug.Log("[AdManager] Firebase ready but no user -> Load LoginScene");
                //SceneManager.LoadScene(currentScene.name);
            }

        }
        else
        {
            //.Log("[AdManager] Firebase not ready -> wait...");
            FirebaseManager.OnFirebaseReady += () =>
            {
                //SceneManager.LoadScene(sceneToLoad.ToString());

                if (GoogleFirebaseAuth.Instance.IsSignedIn())
                {
                    SceneManager.LoadScene(sceneToLoad.ToString());
                    LoginButton.SetActive(false);
                    boots_done = true;
                }
                else
                {
                    //SceneManager.LoadScene(currentScene.name);
                }
                
            };
        }



        
    }
}
