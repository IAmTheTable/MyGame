using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using Unity.Networking.Transport;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerManager : NetworkBehaviour
{
    public NetworkVariable<int> Score = new();

    public Rigidbody2D rb;
    private Camera camera;
    public Physics2DRaycaster raycaster;
    public GameObject bulletPrefab;

    
    public int speedMultiplier = 15;
    public int boostMultiplier = 25;

    private Rpc _rpc;
    private int _boostCount = 5;


    public override void OnNetworkSpawn()
    {
        // z index to 2, because layers
        transform.position += new Vector3(0, 0, 2);
        gameObject.tag = "Player"; //player tag for server uses
        Debug.Log("Player Spawned: id " + NetworkManager.LocalClientId);
        //Camera.main.transform.SetParent(transform);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        _rpc = GetComponent<Rpc>();
        if (IsClient)
            camera = Camera.main;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        // client only camera manipulation
        if (IsClient)
        {
            var cameraPosition = camera.transform.position;
            var playerPosition = transform.position;

            cameraPosition.x = Mathf.Lerp(cameraPosition.x, playerPosition.x, 0.1f);
            cameraPosition.y = Mathf.Lerp(cameraPosition.y, playerPosition.y, 0.1f);

            camera.transform.position = cameraPosition;
        }


        // basic WASD key movement

        if (Input.GetKey(KeyCode.W))
        {
            // move using rigid body
            _rpc.RequestMoveServerRpc(Vector3.up * speedMultiplier);
        }
        if (Input.GetKey(KeyCode.A))
        {
            _rpc.RequestMoveServerRpc(Vector2.left * speedMultiplier);
        }
        if (Input.GetKey(KeyCode.S))
        {
            _rpc.RequestMoveServerRpc(Vector2.down * speedMultiplier);
        }
        if (Input.GetKey(KeyCode.D))
        {
            _rpc.RequestMoveServerRpc(Vector2.right * speedMultiplier);
        }

        if (Input.GetMouseButtonDown(0))
        {
            // get mouse position
            var mousePosition = Input.mousePosition;
            // convert to world position
            var worldPosition = camera.ScreenToWorldPoint(mousePosition);
            // get direction
            var direction = worldPosition - transform.position;
            // shoot
            _rpc.ClientShootServerRpc(direction);

            // spawn a circle as a "bullet"
            
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (_boostCount > 0)
            {
                _rpc.RequestMoveServerRpc(rb.velocity * boostMultiplier);
                _boostCount--;
            }
        }
        else
        {
            _boostCount = 5;
        }
    }
}
