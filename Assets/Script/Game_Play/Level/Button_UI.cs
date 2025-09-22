using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;



public class Button_UI : MonoBehaviour
{
    public SceneList sceneToLoad;

    public void UndoButton()
    {
        SceneManager.LoadScene(sceneToLoad.ToString());

    }

    // Update is called once per frame

}
