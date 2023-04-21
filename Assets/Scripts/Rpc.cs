using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Rpc : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        Debug.Log($"Connected to the server: isServer: {IsServer} isHost: {IsHost} isClient: {IsClient}");
    }
    
    [ServerRpc]
    public void RequestMoveServerRpc(Vector3 direction, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            var client = NetworkManager.ConnectedClients[clientId];
            client.PlayerObject.GetComponent<Rigidbody2D>().AddForce(direction);
        }
    }

    [ClientRpc]
    public void OnEatClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("Client eat rpc");
        // get our player object
        var player = NetworkManager.ConnectedClients[NetworkManager.LocalClientId].PlayerObject;
        player.GetComponent<PlayerManager>().Score.Value++;
        player.transform.localScale += new Vector3(0.1f, 0.1f, 0.1f);
    }

    [ServerRpc]
    public void ClientShootServerRpc(Vector3 lookPos)
    {
        if (NetworkManager.ConnectedClients.ContainsKey(OwnerClientId))
        {
            var client = NetworkManager.ConnectedClients[OwnerClientId];
            var localPlayer = client.PlayerObject;
            var castRay = new Ray(client.PlayerObject.transform.position, lookPos);
            RaycastHit rayCastHit;
            if (Physics.Raycast(castRay, out rayCastHit, 100.0f))
            {
                if (rayCastHit.collider.CompareTag("Player"))
                {
                    var player = rayCastHit.collider.gameObject.GetComponent<PlayerManager>();
                    player.Score.Value--;
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
