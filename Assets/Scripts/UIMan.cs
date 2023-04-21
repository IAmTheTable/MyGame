using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class UIMan : MonoBehaviour
{
    public GameObject GameManager;
    public GameObject host;
    public GameObject client;

    public void OnHost()
    {
        NetworkManager.Singleton.StartHost();
        host.SetActive(false);
        client.SetActive(false);

        // spawn the game manager on the server
        GameObject gm = Instantiate(GameManager, Vector3.zero, Quaternion.identity);
        gm.GetComponent<NetworkObject>().Spawn();
    }

    public void OnJoin()
    {
        NetworkManager.Singleton.StartClient();
        host.SetActive(false);
        client.SetActive(false);
    }


}
