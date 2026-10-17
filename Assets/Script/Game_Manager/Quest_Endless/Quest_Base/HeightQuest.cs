using UnityEngine;

/// <summary>
/// Nhiệm vụ: đạt tới một độ cao (m) nhất định trong Endless Mode.
/// Lấy độ cao hiện tại của người chơi từ GameManager.Instance.currentHeightest
/// (đây là mốc cao nhất player đã đạt được, khớp với UI hiển thị hiện có).
/// </summary>
public class HeightQuest : QuestBase
{
    private float targetHeight;

    public override QuestBase Clone()
    {
        return new HeightQuest();
    }

    public override void SetTarget(float target)
    {
        targetHeight = target;
        displayName = $"Get to {targetHeight:0}m";
    }

    public override void UpdateProgress()
    {
        if (IsCompleted) return;
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.currentHeightest >= targetHeight)
        {
            IsCompleted = true;
        }
    }

    public override float GetProgressRatio()
    {
        if (GameManager.Instance == null || targetHeight <= 0f) return 0f;
        return Mathf.Clamp01(GameManager.Instance.currentHeightest / targetHeight);
    }

    public override string GetProgressText()
    {
        float current = GameManager.Instance != null ? GameManager.Instance.currentHeightest : 0f;
        return $"{current:0} / {targetHeight:0}m";
    }
}
