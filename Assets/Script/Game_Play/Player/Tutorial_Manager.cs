using UnityEngine;
using UnityEngine.UI;

public class Tutorial_Manager : MonoBehaviour
{
    public GameObject tutorialUIPopup;   // Popup UI hỏi người chơi
    public Text tutorialText;            // Text hiển thị hướng dẫn
           
    public GameObject TextObject;
               

    private enum TutorialStep { None, ShowUI, HoldPress, DragDirection, ReleaseJump, Complete }
    private TutorialStep currentStep = TutorialStep.None;

    void Start()
    {
        // Ban đầu pause game, hỏi người chơi
        Time.timeScale = 0f;
        tutorialUIPopup.SetActive(true);
        tutorialText.text = "TUTORIAL";
        
        

        // Chờ người chơi chọn -> OnAcceptTutorial() hoặc OnSkipTutorial()
    }

    // Gọi từ nút "Yes" trong popup
    public void OnAcceptTutorial()
    {
        tutorialText.text = "Press And Hold";
        tutorialText.color = Color.white;
        currentStep = TutorialStep.HoldPress;
        tutorialUIPopup.SetActive(false);
        TextObject.SetActive(true);
        Time.timeScale = 1f; // resume game
    }

    // Gọi từ nút "Skip" trong popup
    public void OnSkipTutorial()
    {
        tutorialUIPopup.SetActive(false);
        currentStep = TutorialStep.Complete; // Bỏ qua tutorial
        Time.timeScale = 1f;
        
    }

    void Update()
    {
        switch (currentStep)
        {
            
            case TutorialStep.HoldPress:
                if (InputManager.IsInputDown())
                {
                    
                    tutorialText.text = "Drag to Diracton";
                    currentStep = TutorialStep.DragDirection;
                }
                break;

            case TutorialStep.DragDirection:
                if (InputManager.IsInputHeld())
                {
                    //do something to show direction (optional)

                }
                if (InputManager.IsInputUp())
                {
                    
                    tutorialText.text = "Release to Jump";
                    currentStep = TutorialStep.ReleaseJump;
                    
                }
                break;

            case TutorialStep.ReleaseJump:
                // Chờ PlayerLanded() để hoàn tất
                break;

            case TutorialStep.Complete:
                // Tutorial đã xong hoặc skip -> gameplay bình thường
                break;
        }
    }

    public void PlayerLanded()
    {
        if (currentStep == TutorialStep.ReleaseJump)
        {
            
            TextObject.SetActive(false);
            
            currentStep = TutorialStep.Complete;
        }
    }
}
