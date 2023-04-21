using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public int maxZoom = 105;
    public int minZoom = 25;

    float originalSize = 55;
    float newSize = 55;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Debug.Log(Input.mouseScrollDelta.ToString());
        var ogSize = Camera.main.orthographicSize;
        var scroll = Input.mouseScrollDelta.y;
        var change = scroll * -20;
        if (NetworkManager.Singleton.IsClient && scroll != 0)
            Camera.main.orthographicSize = Mathf.Lerp(ogSize, ogSize + ((ogSize + scroll < maxZoom) && (ogSize + scroll) ) ? 0 : scroll)), 0.1f);
    }
}
