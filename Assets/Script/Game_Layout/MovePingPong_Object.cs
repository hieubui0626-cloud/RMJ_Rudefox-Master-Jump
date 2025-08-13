using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePingPong_Object : MonoBehaviour
{
    public float pingPongLength = 3f; // Length of the Ping Pong effect
    public float speed = 1f; // Speed of the Ping Pong effect

    private float initialY; // Store the initial Y position

    void Start()
    {
        initialY = transform.position.y; // Store the initial Y position
    }

    void Update()
    {
        // Calculate the new Y position using Mathf.PingPong
        float newY = initialY + Mathf.PingPong(Time.time * speed, pingPongLength) - pingPongLength / 1f;

        // Update the object's position
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
