using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // Start is called before the first frame update
    public Rigidbody2D rb;
    public Camera camera;


    private int _boostCount = 5;
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var cameraPosition = camera.transform.position;
        var playerPosition = transform.position;

        cameraPosition.x = Mathf.Lerp(cameraPosition.x, playerPosition.x, 0.1f);
        cameraPosition.y = Mathf.Lerp(cameraPosition.y, playerPosition.y, 0.1f);

        camera.transform.position = cameraPosition;
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

        if(Input.GetKey(KeyCode.LeftShift))
        {
            if (_boostCount > 0)
            {
                rb.AddForce(rb.velocity * 20);
                _boostCount--;
            }
        }
        else
        {
            _boostCount = 5;
        }
    }
}
