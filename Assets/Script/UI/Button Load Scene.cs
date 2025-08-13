using UnityEngine;
using UnityEngine.SceneManagement;



public class Buttonloadscene : MonoBehaviour
{
    public SceneList sceneToLoad;


    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        Ray ray = mainCam.ScreenPointToRay(InputManager.GetInputPosition());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            
            //GetComponent<Renderer>().material.color = Color.yellow;
            if (hit.collider.gameObject == gameObject)
            {
                if (InputManager.IsInputDown())
                {
                    SceneManager.LoadScene(sceneToLoad.ToString());


                }
                

            }
        }
        else
        {
            //GetComponent<Renderer>().material.color = Color.white;
        }
        
    }
}