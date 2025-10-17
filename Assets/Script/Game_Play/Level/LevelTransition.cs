using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTransition : MonoBehaviour
{
    public static LevelTransition Instance;

    public Animator Transition_animator;

    public GameObject Transiton_Gameobject;

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

        
    }

    private void Start()
    {
        Transiton_Gameobject.SetActive(true);
        if (Transition_animator == null)
        {
            Transition_animator = Transiton_Gameobject.GetComponent<Animator>();
        }

        //StartTransiton();
    }

    public void StartTransiton()
    {
        Transition_animator.Play("Start_Scene");
    }
    
    public void EndTransition()
    {
        Transition_animator.Play("End_Scene");
    }

    
}
