using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;



public class Button_UI : MonoBehaviour
{
    public SceneList sceneToLoad;
    public SceneList sceneStart;
    public GameObject ButtonStart;
    public GameObject Pannel;

    public void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name == "Main_Menu")
        {
            Boots_Level.Instance.SignOutButton.SetActive(true);
        }
        else
        {
            Boots_Level.Instance.SignOutButton.SetActive(false);
        }
        
    }
    public void UndoButton()
    {
        SceneManager.LoadScene(sceneToLoad.ToString());

    }
    public void loadsceneStart()
    {
        SceneManager.LoadScene(sceneStart.ToString());
        StartCoroutine(StartWailTime());
    }
    IEnumerator StartWailTime()
    {
        LevelTransition.Instance.EndTransition();
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(sceneStart.ToString());

    }

    public void OpenPannel()
    { Pannel.SetActive(true);}

    public void ClosePannel()
    { Pannel.SetActive(false); }

}
