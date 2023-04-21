using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Food : NetworkBehaviour
{
    public CircleCollider2D circleCollider;
    public Rpc rpc;
    // Start is called before the first frame update
    void Start()
    {

    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player"))
            return;
        Debug.Log("Eating food");
        var player = other.gameObject.GetComponent<PlayerManager>();
        rpc.OnEatClientRpc(new() { Send = new() { TargetClientIds = new[] { player.NetworkObject.OwnerClientId } } });
        NetworkObject.Despawn(true);
    }
}
