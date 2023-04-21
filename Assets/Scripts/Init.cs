using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class Init : MonoBehaviour
{
    // Start is called before the first frame update

    public bool hostMode = false;
    public TextMeshProUGUI text;
    public ulong clientId = 0;
    
    void Start()
    {
        text = GameObject.Find("Text").GetComponent<TextMeshProUGUI>();
        NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
    }

    private void Singleton_OnClientConnectedCallback(ulong obj)
    {
        clientId = obj;
        text.text = "Players: " + NetworkManager.Singleton.ConnectedClients.Count;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
