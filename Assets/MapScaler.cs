using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapScaler : MonoBehaviour
{
    [Range(-1000, 1000)]
    public float scaleX = 100;
    [Range(-1000, 1000)]
    public float scaleY = 100;

    public List<GameObject> thingstoscale = new();

    // Start is called before the first frame update
    void Start()
    {
        foreach (var item in thingstoscale)
        {
            item.transform.localScale = new Vector3(scaleX, scaleY, 1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
