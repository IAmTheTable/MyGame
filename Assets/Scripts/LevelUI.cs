using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LevelUI : MonoBehaviour
{
    public GameObject Slider;
    public GameObject Base;
    public TextMeshProUGUI Text;
    public TextMeshProUGUI LevelText;
    // Start is called before the first frame update
    void Start()
    {
        Base.SetActive(false);
    }
    public float GetXPRate() => Rpc.GetXPRate();

    // Update is called once per frame
    void Update()
    {
        if(Base.activeInHierarchy)
        {
            var player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            Slider.GetComponent<RectTransform>().sizeDelta = new Vector2(Base.GetComponent<RectTransform>().sizeDelta.x * (player.XP.Value / (player.Level.Value * GetXPRate())), Slider.GetComponent<RectTransform>().sizeDelta.y);
            Text.SetText($"{player.XP.Value}/{GetXPRate() * player.Level.Value}");

            if(player.UpgradePoints.Value > 0)
                LevelText.SetText($"You have: {player.UpgradePoints.Value} upgrade points.");
            else
                LevelText.SetText("");
        }
    }
}
