using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTransition : MonoBehaviour
{
    public static LevelTransition Instance;

    public Animator Transition_animator;

    void Awake()
    {
        // Gán singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        if (Transition_animator == null)
        {
            Transition_animator = GetComponent<Animator>();
        }
    }
    public void EndTransition()
    {
        Transition_animator.Play("End_Scene");
    }

    
}
