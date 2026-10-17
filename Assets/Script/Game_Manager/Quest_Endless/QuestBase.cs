using UnityEngine;

/// <summary>
/// Lớp cơ sở cho mọi loại Quest trong Endless Mode.
/// Mỗi loại quest cụ thể (VD: HeightQuest, TokenCollectQuest sau này...) sẽ kế thừa lớp này.
/// QuestManager giữ danh sách các "prototype" (instance mẫu) của từng loại quest,
/// khi cần Pull() sẽ Clone() ra một instance mới rồi gọi SetTarget() để cấu hình độ khó.
/// </summary>
public abstract class QuestBase
{
    /// <summary>Tên hiển thị cho UI, VD: "Đạt độ cao {target}m"</summary>
    public string displayName;

    /// <summary>Đã hoàn thành nhiệm vụ hay chưa</summary>
    public bool IsCompleted { get; protected set; }

    /// <summary>
    /// Tạo một bản sao mới của quest này (dùng làm instance chạy thực tế,
    /// vì prototype trong pool không nên bị thay đổi trạng thái).
    /// </summary>
    public abstract QuestBase Clone();

    /// <summary>
    /// Gán mục tiêu (target) cho quest dựa theo độ khó đã tính toán ở QuestManager.
    /// VD với HeightQuest: target là độ cao (m) cần đạt.
    /// </summary>
    public abstract void SetTarget(float target);

    /// <summary>
    /// Được QuestManager gọi mỗi frame (khi quest đang active) để kiểm tra tiến độ.
    /// Set IsCompleted = true khi đủ điều kiện.
    /// </summary>
    public abstract void UpdateProgress();

    /// <summary>Tỉ lệ tiến độ 0..1, dùng cho progress bar UI (không bắt buộc dùng)</summary>
    public abstract float GetProgressRatio();

    /// <summary>Text mô tả tiến độ hiện tại, VD: "125 / 200m"</summary>
    public abstract string GetProgressText();
}
