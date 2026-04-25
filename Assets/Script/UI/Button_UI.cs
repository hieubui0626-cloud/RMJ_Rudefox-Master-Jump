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
    //Store 
    public GameObject StorePannel;
    public List<GameObject> StoreDisableObjects;
    public Vector3 posBase;
    public Vector3 posZoom;

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

    public void OpenStorePannel()
    {
        StopAllCoroutines(); // Tránh xung đột nếu nhấn nút liên tục
        StartCoroutine(MoveCamera(posZoom, true));
    }

    public void CloseStorePannel()
    {
        StopAllCoroutines();
        StartCoroutine(MoveCamera(posBase, false));
    }

    IEnumerator MoveCamera(Vector3 targetPos, bool isOpen)
    {
        float elapsedTime = 0f;
        float duration = 0.5f;
        Vector3 startingPos = Camera.main.transform.position;

        // Nếu mở: Hiện panel ngay để người dùng thấy giao diện luôn
        if (isOpen)
        {
            StorePannel.SetActive(true);
            foreach (GameObject obj in StoreDisableObjects)
            {
                if (obj != null) obj.SetActive(false);
            }
        } 
        

        while (elapsedTime < duration)
        {
            Camera.main.transform.position = Vector3.Lerp(startingPos, targetPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.position = targetPos;

        // Nếu đóng: Đợi zoom xong mới tắt panel cho mượt
        if (!isOpen)
        {
            StorePannel.SetActive(false);
            foreach (GameObject obj in StoreDisableObjects)
            {
                if (obj != null) obj.SetActive(true);
            }
        } 
        
    }

}
