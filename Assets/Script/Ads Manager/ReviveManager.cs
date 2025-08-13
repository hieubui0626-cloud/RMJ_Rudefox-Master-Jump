using UnityEngine;

public class ReviveManager : MonoBehaviour
{
    public static ReviveManager Instance;

    private Vector3 lastSafePosition;
    public bool hasRevived = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RecordSafePosition(Vector3 position)
    {
        lastSafePosition = position;
    }

    public bool HasRevived()
    {
        return hasRevived;
    }

    public void OnReviveConfirmed()
    {
        hasRevived = true;

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.ReviveAt(lastSafePosition);
        }
    }

    public void ResetReviveStatus()
    {
        hasRevived = false;
    }
}
