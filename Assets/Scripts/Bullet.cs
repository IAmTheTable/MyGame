using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public Rpc rpc;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && IsOwner && collision.gameObject.GetComponent<NetworkObject>().OwnerClientId != NetworkManager.Singleton.LocalClientId)
        {
            rpc.OnBulletHitServerRpc(collision.gameObject.GetComponent<NetworkObject>().OwnerClientId);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
