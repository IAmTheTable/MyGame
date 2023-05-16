using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MinimapObject
{
    public readonly Guid id = Guid.NewGuid();
    public GameObject Host;
    public GameObject Object;
    /// <summary>
    /// Position
    /// </summary>
    public Vector2 Position
    {
        get => Object.transform.position;
    }
}

public class MinimapService : MonoBehaviour
{
    public static GameObject PlayerPosPrefab;
    public static GameObject MinimapStatic;

    public GameObject Minimap;
    public GameObject PlayerPos;

    public enum MapState
    {
        Original,
        Enlarged,
        FullScreen
    }

    public MapState mapState;

    public int Count { get => ShownObjects.Count; }

    public static List<MinimapObject> ShownObjects = new();

    // Start is called before the first frame update
    void Start()
    {
        Minimap.SetActive(false);
        PlayerPosPrefab = PlayerPos;
        MinimapStatic = Minimap;
        PlayerPos.GetComponent<Image>().color = new Color(0, 200, 0);
    }

    public static MinimapObject Add(GameObject o)
    {
        Debug.Log("MMO ADD");
        MinimapObject mo = new()
        {
            Host = Instantiate(PlayerPosPrefab),
            Object = o,
        };

        mo.Host.transform.parent = MinimapStatic.transform;


        ShownObjects.Add(mo);
        return mo;
    }

    public static void Delete(MinimapObject o)
    {
        ShownObjects.Remove(o);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = new(0, 0);
        if (NetworkManager.Singleton.IsClient && Minimap.activeInHierarchy)
        {
            var (MaxX, MaxY) = (GameManager.instance.spawnArea.maxX, GameManager.instance.spawnArea.maxY);

            var plrRect = PlayerPos.GetComponent<RectTransform>();
            var (offsetMultiplierX, offsetMultiplierY) = ((Minimap.GetComponent<RectTransform>().sizeDelta.x - plrRect.sizeDelta.x) / 2f, (Minimap.GetComponent<RectTransform>().sizeDelta.y - plrRect.sizeDelta.y) / 2f);

            var newPos = new Vector3((NetworkManager.Singleton.LocalClient.PlayerObject.transform.position.x / MaxX) * offsetMultiplierX, (NetworkManager.Singleton.LocalClient.PlayerObject.transform.position.y / MaxY) * offsetMultiplierY);

            PlayerPos.GetComponent<RectTransform>().anchoredPosition = newPos;

            foreach (var obj in ShownObjects)
            {
                obj.Host.GetComponent<Image>().color = Color.red;
                obj.Host.GetComponent<RectTransform>().anchoredPosition = new(obj.Position.x / MaxX * offsetMultiplierX, obj.Position.y / MaxY * offsetMultiplierY);
            }
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            switch (mapState)
            {
                case MapState.Original:
                    mapState = MapState.Enlarged;
                    DoMapEnlarged();
                    break;
                case MapState.Enlarged:
                    mapState = MapState.FullScreen;
                    DoMapFullScreen();
                    break;
                case MapState.FullScreen:
                    mapState = MapState.Original;
                    DoMapOriginal();
                    break;
            }
        }
    }

    public void DoMapOriginal()
    {
        var rectTransform = Minimap.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new(90, 90);
        rectTransform.anchoredPosition = new(1, 0);
        rectTransform.anchorMax = new(1, 0);
        rectTransform.anchorMin = new(1, 0);
        rectTransform.pivot = new(1, 0);
    }

    public void DoMapEnlarged()
    {
        var rectTransform = Minimap.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new(180, 180);
        rectTransform.anchoredPosition = new(1, 0);
        rectTransform.anchorMax = new(1, 0);
        rectTransform.anchorMin = new(1, 0);
        rectTransform.pivot = new(1, 0);
    }

    public void DoMapFullScreen()
    {
        var rectTransform = Minimap.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new(450, 450);
        rectTransform.anchoredPosition = new(0.5f, 0.5f);
        rectTransform.anchorMax = new(0.5f, 0.5f);
        rectTransform.anchorMin = new(0.5f, 0.5f);
        rectTransform.pivot = new(0.5f, 0.5f);
    }

}
