using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Netcode;
using UnityEngine;

public class DeathExploder : NetworkBehaviour
{
    private bool start = false;
    private bool finished = false;
    [SerializeField]
    public static float change = 0.15f;
    [SerializeField]
    public static float max = 200.0f;
    public static (float maxSizeMax, float maxSizeMin) maxSizes = (max + change, max);
    public override void OnNetworkSpawn()
    {
        start = true;
        transform.position = new(transform.position.x, transform.position.y, 100);
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    private void Update()
    {
        if (transform.localScale.x >= maxSizes.maxSizeMin)
        {
            finished = true;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (start)
        {
            if (IsServer || IsHost)
            {
                transform.localScale = new(Mathf.Lerp(transform.localScale.x, maxSizes.maxSizeMax, change), Mathf.Lerp(transform.localScale.y, maxSizes.maxSizeMax, change));
                StartCoroutine(delayDespawn());
            }
        }
    }

    private IEnumerator delayDespawn()
    {
        yield return new WaitUntil(() => finished == true);
        if (IsSpawned)
            NetworkObject.Despawn(true);
    }
}
