using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using static Rpc;

public class UpgradePart : MonoBehaviour
{
    public TextMeshProUGUI Text;
    public UpgradeType upgradeType;
    // Start is called before the first frame update
    void Start()
    {
        
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        var player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();
        if(NetworkManager.Singleton.IsClient)
        {
            Text.SetText($"{player.GetUpgradeValue(upgradeType)}/10");
        }
    }
}
