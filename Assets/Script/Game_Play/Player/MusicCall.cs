using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicCall : MonoBehaviour
{
    public MusicTrack musicTrack;
    void Start()
    {
        MusicManager.Instance.PlayMusic(musicTrack);
    }

    // Update is called once per frame
    

}
