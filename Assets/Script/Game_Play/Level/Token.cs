using UnityEngine;

public class Token : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("+1 token");
        if (other.CompareTag("Token"))
        {
            
            GameManager.Instance.AddToken(1); // +1 token
            other.gameObject.SetActive(false);

        }
    }
}
