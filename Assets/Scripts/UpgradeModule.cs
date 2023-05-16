using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class UpgradeModule : MonoBehaviour
{
    public GameObject panel;
    public GameObject trigger;
    private float startHeight = 0;
    private float endHeight = 30;

    private bool isShown = false;


    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
        var scale = panel.GetComponent<RectTransform>().sizeDelta;
        startHeight = scale.y;
        panel.GetComponent<RectTransform>().sizeDelta = new Vector3(scale.x, endHeight);
        var ev = trigger.GetComponent<EventHooks>();

        ev.OnMouseHoverStart += PanelOnMouseOver;
        ev.OnMouseHoverEnd += PanelOnMouseExit;

        //panel.SetActive(false);
    }

    public void PanelOnMouseOver()
    {
        // if the upgrade menu is hidden
        if (!isShown)
            isShown = !isShown;
        Debug.Log("22222Mouse over");
    }
    public void PanelOnMouseExit()
    {
        // if the upgrade menu is Shown
        if (isShown)
            isShown = !isShown;

        Debug.Log("22222Mouse left");
    }

    // Update is called once per frame
    void Update()
    {
        if (isShown)
            panel.GetComponent<RectTransform>().sizeDelta = new Vector2(panel.GetComponent<RectTransform>().sizeDelta.x, Mathf.Lerp(panel.GetComponent<RectTransform>().sizeDelta.y, startHeight, 0.1f));
        else
            panel.GetComponent<RectTransform>().sizeDelta = new Vector2(panel.GetComponent<RectTransform>().sizeDelta.x, Mathf.Lerp(panel.GetComponent<RectTransform>().sizeDelta.y, endHeight, 0.1f));
    }


    public void DoHealthUpgrade()
    {
        var player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();
        player._rpc.RequestUpgradeServerRpc(Rpc.UpgradeType.Health);
    }
    public void DoRegenUpgrade()
    {
        var player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();
        player._rpc.RequestUpgradeServerRpc(Rpc.UpgradeType.Regen);
    }
    public void DoFireRateUpgrade()
    {
        var player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();
        player._rpc.RequestUpgradeServerRpc(Rpc.UpgradeType.Firerate);
    }
    public void DoDamageUpgrade()
    {
        var player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();
        player._rpc.RequestUpgradeServerRpc(Rpc.UpgradeType.Damage);
    }
    public void DoBulletSpeedUpgrade()
    {
        var player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();
        player._rpc.RequestUpgradeServerRpc(Rpc.UpgradeType.BulletSpeed);
    }
    public void DoPlayerSpeedUpgrade()
    {
        var player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();
        player._rpc.RequestUpgradeServerRpc(Rpc.UpgradeType.MoveSpeed);
    }
}
