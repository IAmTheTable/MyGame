using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Food : NetworkBehaviour
{
    public CircleCollider2D circleCollider;
    public Rpc rpc;

    public GameManager gameManager;
    
    // Start is called before the first frame update
    void Start()
    {
        rpc = GetComponent<Rpc>();

        // add random force to rb
    }

    private void FixedUpdate()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.AddForce(new Vector2(UnityEngine.Random.Range(-100, 100), UnityEngine.Random.Range(-100, 100)));
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        // make sure we are on the server, and that the other object is a player or the middle
        // isOwner = true
        // tag middle or player
        if (!IsOwner && !(other.gameObject.CompareTag("Middle") || other.gameObject.CompareTag("Player")))
            return;

        if (other.gameObject.CompareTag("Player"))
        {
            var player = other.gameObject.GetComponent<PlayerManager>();

            rpc.OnEatServerRpc(player.NetworkObject.OwnerClientId);
        }
        else if (other.gameObject.CompareTag("Middle") && NetworkObject.IsSpawned)
            rpc.FoodDestroyServerRpc();
    }
}