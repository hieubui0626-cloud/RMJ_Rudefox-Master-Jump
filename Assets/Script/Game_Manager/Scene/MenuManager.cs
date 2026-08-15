using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Các chế độ chơi hiện có. Thêm mode mới ở đây khi cần (VD: Boss, Multiplayer,...)
/// </summary>
public enum PlayMode
{
    World,
    Endless
}

/// <summary>
/// 1 mục cấu hình cho từng Mode: Mode + Scene tương ứng + thông tin hiển thị UI.
/// Cấu hình trong Inspector, thứ tự trong list = thứ tự xoay vòng khi bấm mũi tên.
/// </summary>
[System.Serializable]
public class PlayModeEntry
{
    public PlayMode mode;
    public SceneList sceneToLoad;
    public string displayName;
    public Sprite icon; // tuỳ chọn, nếu có UI Image hiển thị icon mode
}

/// <summary>
/// Quản lý Menu chọn chế độ chơi kiểu Carousel (xoay vòng bằng 2 nút mũi tên trái/phải).
/// - Nút mũi tên KHÔNG load scene ngay, chỉ đổi mode đang được chọn (currentEntry).
/// - Khi người chơi bấm nút "Play/Start" riêng, mới gọi LoadSceneForCurrentMode()
///   để load Scene tương ứng với mode đang chọn tại thời điểm đó.
/// </summary>
public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    private const string SelectedModeIndexKey = "SelectedPlayModeIndex";

    [Header("Danh sách các Mode (thứ tự = thứ tự xoay vòng)")]
    public List<PlayModeEntry> playModes = new List<PlayModeEntry>()
    {
        new PlayModeEntry { mode = PlayMode.World,   sceneToLoad = SceneList.Level_Map, displayName = "World" },
        new PlayModeEntry { mode = PlayMode.Endless, sceneToLoad = SceneList.Endless,   displayName = "Endless" },
    };

    [Header("UI hiển thị Mode hiện tại (tuỳ chọn)")]
    public TextMeshProUGUI modeNameText;
    public UnityEngine.UI.Image modeIconImage;

    [Header("Transition (tuỳ chọn)")]
    [Tooltip("Thời gian chờ trước khi load scene, để khớp animation chuyển cảnh")]
    public float transitionDelay = 1f;

    private int currentIndex = 0;

    public PlayModeEntry currentEntry => playModes[currentIndex];
    public PlayMode currentMode => currentEntry.mode;

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
    }

    void Start()
    {
        currentIndex = 1;
        LoadSavedModeIndex();
        UpdateModeUI();
    }

    #region Arrow Button Callbacks

    /// <summary>
    /// Gán vào nút mũi tên PHẢI (->) để chuyển sang mode kế tiếp
    /// </summary>
    public void NextMode()
    {
        if (playModes == null || playModes.Count == 0) return;

        currentIndex = (currentIndex + 1) % playModes.Count;
        OnModeChanged();
    }

    /// <summary>
    /// Gán vào nút mũi tên TRÁI (<-) để quay lại mode trước đó
    /// </summary>
    public void PreviousMode()
    {
        if (playModes == null || playModes.Count == 0) return;

        currentIndex = (currentIndex - 1 + playModes.Count) % playModes.Count;
        OnModeChanged();
    }

    #endregion

    private void OnModeChanged()
    {
        SaveSelectedModeIndex();
        UpdateModeUI();

        Debug.Log($"🎮 Đang chọn Play Mode: {currentEntry.displayName} ({currentMode})");
    }

    private void UpdateModeUI()
    {
        if (modeNameText != null)
            modeNameText.text = currentEntry.displayName;

        if (modeIconImage != null)
        {
            modeIconImage.sprite = currentEntry.icon;
            modeIconImage.enabled = currentEntry.icon != null;
        }
    }

    /// <summary>
    /// Gán vào nút "Play / Start". Load Scene dựa vào Mode đang được chọn
    /// tại thời điểm bấm (currentEntry), KHÔNG phải mode mặc định.
    /// </summary>
    public void LoadSceneForCurrentMode()
    {
        SceneList targetScene = currentEntry.sceneToLoad;

        // Nếu có hiệu ứng chuyển cảnh (giống các script khác trong project), gọi trước khi load
        if (LevelTransition.Instance != null)
        {
            LevelTransition.Instance.EndTransition();
            StartCoroutine(LoadSceneAfterDelay(targetScene));
        }
        else
        {
            LoadSceneAddressable(targetScene);
        }
    }

    private IEnumerator LoadSceneAfterDelay(SceneList scene)
    {
        yield return new WaitForSeconds(transitionDelay);
        LoadSceneAddressable(scene);
    }

    private void LoadSceneAddressable(SceneList scene)
    {
        string address = scene.ToString();
        Addressables.LoadSceneAsync(address, LoadSceneMode.Single, true);
    }

    #region Save / Load Mode Index

    private void SaveSelectedModeIndex()
    {
        PlayerPrefs.SetInt(SelectedModeIndexKey, currentIndex);
        PlayerPrefs.Save();
    }

    private void LoadSavedModeIndex()
    {
        int saved = PlayerPrefs.GetInt(SelectedModeIndexKey, 0);
        if (playModes != null && playModes.Count > 0)
        {
            currentIndex = Mathf.Clamp(saved, 0, playModes.Count - 1);
        }
    }

    #endregion
}
