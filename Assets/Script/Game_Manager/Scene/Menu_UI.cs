using UnityEngine;
using TMPro;

public class Menu_UI : MonoBehaviour
{
    public TextMeshProUGUI Total_Token_Menu;
    
    void Start()
    {
        TotalTokenMenu();
    }

    public void TotalTokenMenu()
    {
        FirebaseManager.Instance.GetTotalTokens(total =>
        {
            Total_Token_Menu.text = total.ToString();
            
            
        });
    }
    
}
