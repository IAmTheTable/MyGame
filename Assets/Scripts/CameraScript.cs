using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraScript : MonoBehaviour
{
    public int maxZoom = 105;
    public int minZoom = 25;

    float originalSize = 55;
    float newSize = 55;
    // Update is called once per frame
    void LateUpdate()
    {
        var ogSize = Camera.main.orthographicSize;
        var scroll = Input.mouseScrollDelta.y * -20;
        if (NetworkManager.Singleton.IsClient && scroll != 0)
            Camera.main.orthographicSize = Mathf.Lerp(ogSize, ogSize + (ogSize + scroll > maxZoom ? 0 : (ogSize + scroll < minZoom ? 0 : scroll)), 0.1f);
    }
}
