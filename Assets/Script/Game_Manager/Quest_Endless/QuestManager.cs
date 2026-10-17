using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Quản lý hệ thống Quest trong Endless Mode.
/// - Giữ 1 "pool" các loại quest (đăng ký bằng code trong RegisterQuestPool()).
/// - Pull() chọn ngẫu nhiên 1 loại quest từ pool, tính độ khó (target) theo công thức
///   tăng dần kiểu parabol nhưng có clamp min/max để không tăng quá nhanh / không bất khả thi.
/// - Mỗi frame theo dõi tiến độ quest đang active, khi hoàn thành sẽ:
///     1. Bắn event cho UI chạy hiệu ứng ngay lập tức
///     2. Chờ 1 khoảng trễ (rewardVisualDelay) để visual/animation kịp chạy
///     3. Cộng token vào lượt chơi qua GameManager.AddToken (số token thưởng tăng dần theo số quest đã hoàn thành)
///     4. Pull quest tiếp theo
///
/// Cách dùng: gắn script này vào 1 GameObject trong scene Endless (cùng chỗ với RoomManager).
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Độ khó - Height Quest")]
    [Tooltip("Độ cao mục tiêu của quest đầu tiên")]
    public float baseHeightTarget = 50f;

    [Tooltip("Hệ số tùy biến: increment(n) = heightCoefficient * n, n = số quest đã hoàn thành + 1")]
    public float heightCoefficient = 5f;

    [Tooltip("Mức tăng tối thiểu giữa 2 quest liên tiếp")]
    public float minHeightIncrement = 5f;

    [Tooltip("Mức tăng tối đa giữa 2 quest liên tiếp (chặn để không bất khả thi)")]
    public float maxHeightIncrement = 60f;

    [Header("Phần thưởng Token")]
    [Tooltip("Token thưởng cho quest đầu tiên (completedQuestCount = 0)")]
    public int baseTokenReward = 10;

    [Tooltip("Token thưởng cộng thêm cho mỗi quest đã hoàn thành trước đó: reward = base + step * completedCount")]
    public int tokenRewardStep = 5;

    [Tooltip("Thời gian trễ (giây) trước khi thực sự cộng token, để UI kịp chạy hiệu ứng ăn mừng")]
    public float rewardVisualDelay = 1.5f;

    [Header("UI")]
    [Tooltip("Kéo TextMeshPro UI có sẵn vào đây để hiển thị mục tiêu quest")]
    public TextMeshProUGUI questText;

    [Tooltip("Hiện thêm tiến độ (VD: 125 / 200m) bên dưới mục tiêu")]
    public bool showProgress = true;

    [Header("Runtime State (chỉ để xem, không chỉnh tay)")]
    public QuestBase currentQuest;
    public int completedQuestCount = 0;

    // Prototype pool - danh sách các "mẫu" quest, khi Pull sẽ Clone() ra bản chạy thật
    private readonly List<QuestBase> questPool = new List<QuestBase>();

    // Cộng dồn target độ cao qua từng quest (dùng cho công thức increment)
    private float lastHeightTarget;

    private bool isWaitingReward = false;

    // ===== Events cho UI hook (không bắt buộc dùng) =====
    public System.Action<QuestBase> OnQuestStarted;
    public System.Action<QuestBase, int> OnQuestCompleted; // (quest vừa hoàn thành, số token sẽ thưởng)
    public System.Action<int> OnTokensAwarded;              // (tổng token của lượt chơi sau khi cộng)

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        RegisterQuestPool();
    }

    void Start()
    {
        lastHeightTarget = baseHeightTarget;
        PullNextQuest();
    }

    void Update()
    {
        if (currentQuest == null || isWaitingReward) return;

        currentQuest.UpdateProgress();
        RefreshQuestUI();

        if (currentQuest.IsCompleted)
        {
            StartCoroutine(CompleteQuestRoutine(currentQuest));
        }
    }

    /// <summary>
    /// Đăng ký các loại quest có thể xuất hiện trong pool.
    /// Sau này thêm loại mới chỉ cần questPool.Add(new XxxQuest()) ở đây.
    /// </summary>
    private void RegisterQuestPool()
    {
        questPool.Add(new HeightQuest());
        // questPool.Add(new TokenCollectQuest());
        // questPool.Add(new SurviveTimeQuest());
    }

    /// <summary>
    /// Chọn ngẫu nhiên 1 loại quest từ pool, tính độ khó và bắt đầu theo dõi.
    /// </summary>
    public void PullNextQuest()
    {
        if (questPool.Count == 0)
        {
            Debug.LogWarning("QuestManager: Quest pool rỗng, không có gì để Pull.");
            return;
        }

        QuestBase prototype = questPool[Random.Range(0, questPool.Count)];
        QuestBase quest = prototype.Clone();

        // Hiện tại chỉ có HeightQuest, nếu thêm loại khác thì switch-case theo type ở đây
        // để mỗi loại có công thức tính target riêng.
        if (quest is HeightQuest)
        {
            quest.SetTarget(CalculateNextHeightTarget());
        }

        currentQuest = quest;
        isWaitingReward = false;

        RefreshQuestUI();
        OnQuestStarted?.Invoke(quest);
    }

    /// <summary>
    /// Cập nhật text mục tiêu lên UI có sẵn (nếu đã gán questText trong Inspector).
    /// </summary>
    private void RefreshQuestUI()
    {
        if (questText == null || currentQuest == null) return;

        questText.text = showProgress
            ? $"{currentQuest.displayName}\n{currentQuest.GetProgressText()}"
            : currentQuest.displayName;
    }

    /// <summary>
    /// Công thức tăng độ khó dạng parabol có kiểm soát:
    /// increment(n) = heightCoefficient * n, clamp trong [minHeightIncrement, maxHeightIncrement]
    /// target(n) = target(n-1) + increment(n)
    /// → Vì increment tăng tuyến tính theo n nên tổng target tăng theo dạng parabol,
    ///   nhưng nhờ clamp nên độ khó không bao giờ vọt quá nhanh hay đứng yên quá lâu.
    /// </summary>
    private float CalculateNextHeightTarget()
    {
        int n = completedQuestCount + 1;
        float rawIncrement = heightCoefficient * n;
        float increment = Mathf.Clamp(rawIncrement, minHeightIncrement, maxHeightIncrement);

        lastHeightTarget += increment;
        return lastHeightTarget;
    }

    /// <summary>
    /// Xử lý khi 1 quest hoàn thành: báo UI ngay, chờ delay visual, rồi mới cộng token thật
    /// và pull quest kế tiếp.
    /// </summary>
    private IEnumerator CompleteQuestRoutine(QuestBase quest)
    {
        isWaitingReward = true;

        int reward = baseTokenReward + tokenRewardStep * completedQuestCount;

        // Hiện trạng thái hoàn thành trên UI trong lúc chờ delay visual
        if (questText != null)
            questText.text = $"Hoàn thành!\n+{reward} token";

        // Báo ngay cho UI để chạy hiệu ứng (VD popup "+reward token", confetti...)
        OnQuestCompleted?.Invoke(quest, reward);

        // Khoảng trễ để visual/animation kịp chạy trước khi token thực sự được cộng
        yield return new WaitForSeconds(rewardVisualDelay);

        // Cộng vào token của lượt chơi hiện tại (currentLevelTokens + UI tokenText).
        // GameManager sẽ tự lưu lên Firebase khi kết thúc lượt (RestartLevel / SceneUndo),
        // nên không gọi FirebaseManager trực tiếp ở đây để tránh cộng trùng.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddToken(reward);
            OnTokensAwarded?.Invoke(GameManager.Instance.currentLevelTokens);
        }
        else
        {
            Debug.LogWarning("QuestManager: GameManager.Instance null, không thể cộng token.");
        }

        completedQuestCount++;
        isWaitingReward = false;
        PullNextQuest();
    }
}
