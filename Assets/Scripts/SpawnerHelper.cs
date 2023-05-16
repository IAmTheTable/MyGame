using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnerHelper : MonoBehaviour
{
    public GameObject foodPrefab;
    public float SpawnRate = 3;
    private int _counter = 0;

    // Start is called before the first frame update
    void Start()
    {
        // todo disable gravity and introduce longer lifetime
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        if (_counter < SpawnRate)
            StartCoroutine(spawnFood());
    }

    IEnumerator spawnFood()
    {
        if (_counter < SpawnRate)
        {
            _counter++;
            yield return new WaitForSeconds(Random.Range(0, SpawnRate));
            var food = GameManager.instance.SpawnFoodAt(transform.position + new Vector3(Random.Range(-transform.localScale.x, transform.localScale.x), Random.Range(-transform.localScale.y, transform.localScale.y), 0));
            food.GetComponent<GravityManager>().gravityForce = 0;
            yield return new WaitForSeconds(3);
            if (food != null && food.GetComponent<NetworkObject>().IsSpawned)
                food.GetComponent<NetworkObject>().Despawn(true);
            _counter--;
        }
    }
}
