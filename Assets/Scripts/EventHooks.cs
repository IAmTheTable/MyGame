using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventHooks : MonoBehaviour
{
    public Action OnMouseHoverStart = new(() => { });
    public Action OnMouseHoverEnd = new(() => { });

    // Start is called before the first frame update
    void Start()
    {
        gameObject.AddComponent<BoxCollider2D>();
        OnMouseHoverStart += () => { Debug.Log("event] mousehover start"); };
        OnMouseHoverEnd += () => { Debug.Log("event] mousehover end"); };

        OnMouseHoverStart();
        OnMouseHoverEnd();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMouseOver()
    {
        OnMouseHoverStart.Invoke();
        Debug.Log("Mouse over");
    }
    public void OnMouseExit()
    {
        OnMouseHoverEnd.Invoke();
        Debug.Log("Mouse eixt");
    }
}
