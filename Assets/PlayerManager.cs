using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // Start is called before the first frame update
    public Rigidbody2D rb;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // basic WASD key movement
        if (Input.GetKey(KeyCode.W))
        {
            // move using rigid body
            rb.AddForce(Vector2.up * 10);
        }
        if (Input.GetKey(KeyCode.A))
        {
            rb.AddForce(Vector2.left * 10);
        }
        if (Input.GetKey(KeyCode.S))
        {
            rb.AddForce(Vector2.down * 10);
        }
        if (Input.GetKey(KeyCode.D))
        {
            rb.AddForce(Vector2.right * 10);
        }
    }
}
