using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


public class Buttonloadscene : MonoBehaviour
{
    public SceneList sceneStart;
    
    public GameObject ButtonStart;
    public GameObject ButtonSignOut;

    private void Start()
    {
        ButtonStart.SetActive(true);
    }
    public void loadsceneStart()
    {
        LevelTransition.Instance.EndTransition();
        ButtonStart.SetActive(false);
        ButtonSignOut.SetActive(false);
        StartCoroutine(StartWailTime());
    }

    public void loadsceneSignOut()
    {
        GoogleFirebaseAuth.Instance.SignOut();
    }




    IEnumerator StartWailTime()
    {

        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(sceneStart.ToString());
    }

    

}