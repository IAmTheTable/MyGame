using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityManager : MonoBehaviour
{
    public GameObject planet;
    Rigidbody2D rb;
    public float gravityForce;
    public float gravityDistance;
    

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        planet = GameObject.Find("GravityCircle");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float dist = Vector3.Distance(gameObject.transform.position, planet.transform.position);

        if (dist <= gravityDistance)
        {
            Vector3 v = planet.transform.position - transform.position;

            rb.AddForce(v.normalized * (1.0f - dist / gravityDistance) * gravityForce);
        }
    }
}
