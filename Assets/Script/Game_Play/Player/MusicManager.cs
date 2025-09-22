using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Music Clips (gắn clip theo enum)")]
    public List<MusicEntry> musicEntries = new List<MusicEntry>();

    private Dictionary<MusicTrack, AudioClip> musicDict = new Dictionary<MusicTrack, AudioClip>();
    public AudioSource audioSource;

    [System.Serializable]
    public class MusicEntry
    {
        public MusicTrack track;
        public AudioClip clip;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.volume = 0.5f;

            // Đưa các track từ list vào dictionary để dễ tra cứu
            foreach (var entry in musicEntries)
            {
                if (!musicDict.ContainsKey(entry.track) && entry.clip != null)
                    musicDict.Add(entry.track, entry.clip);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Phát nhạc theo enum
    /// </summary>
    public void PlayMusic(MusicTrack track)
    {
        if (track == MusicTrack.None || !musicDict.ContainsKey(track))
        {
            Debug.LogWarning("Không có nhạc cho track: " + track);
            return;
        }

        audioSource.clip = musicDict[track];
        audioSource.Play();
        audioSource.volume = 0.2f;
    }

    /// <summary>
    /// Dừng nhạc hiện tại
    /// </summary>
    public void StopMusic()
    {
        audioSource.Stop();
    }

    /// <summary>
    /// Kiểm tra có đang phát nhạc cụ thể không
    /// </summary>
    public bool IsPlaying(MusicTrack track)
    {
        return audioSource.isPlaying && audioSource.clip == musicDict[track];
    }
}
