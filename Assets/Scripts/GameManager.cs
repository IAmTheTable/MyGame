using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.Networking.Transport.Utilities;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    public GameObject food_Prefab;
    public GameObject foodSpawnerPrefab;
    public GameObject walls;
    public Rpc rpc;

    public int DelayMilliseconds = 1000;

    public int SpawnerCount = 5;
    public List<MinimapObject> spawners = new();

    public NetworkVariable<int> availableFood = new();
    // the area where the food can spawn
    public (int minX, int maxX, int minY, int maxY) spawnArea;

    public int MaxFoodCount = 5000;

    public override void OnNetworkSpawn()
    {
        if (!IsClient) return;
        rpc.OnClientConnectServerRpc();
        StartCoroutine(randomizeSpawn());
    }

    public void Start()
    {
        instance = this;
        walls = GameObject.Find("Walls");
        Debug.Log("Connected on client");
        var map = walls.GetComponent<EdgeCollider2D>().bounds;
        Debug.Log("Bounds: Max: " + map.max.ToString() + " Min: " + map.min.ToString() + " Size: " + map.size.ToString());
        // set the spawn area to the bounds of the map
        spawnArea = ((int)map.min.x, (int)map.max.x, (int)map.min.x, (int)map.max.x);
    }

    IEnumerator randomizeSpawn()
    {
        // spawn x count of spawners at random locations
        for (int i = 0; i < SpawnerCount; i++)
        {
            var spawnloc = new Vector3(UnityEngine.Random.Range(spawnArea.minX, spawnArea.maxX), UnityEngine.Random.Range(spawnArea.minX, spawnArea.maxX));
            Debug.Log(spawnloc);
            spawners.Add(MinimapService.Add(Instantiate(foodSpawnerPrefab, spawnloc, quaternion.identity))); // add to the minimap service
        }
        yield return null;
    }
    
    DateTime nextFoodSpawn = DateTime.Now.AddSeconds(5);

    void FixedUpdate()
    {
        if (!IsServer)
            return;
        // check if we are outside out cooldown timespan and if we arent maxed out on food on the server.
        if (DateTime.Now > nextFoodSpawn && availableFood.Value < MaxFoodCount)
        {
            nextFoodSpawn = DateTime.Now.AddMilliseconds(DelayMilliseconds);
            SpawnFood(); // spawn the food on the server
        }
    }

    public void SpawnFood()
    {
        var foodPrefab = Instantiate(food_Prefab);
        foodPrefab.GetComponent<NetworkObject>().Spawn();

        // asign a random pos within the spawn area
        var rnd = new System.Random();
        foodPrefab.transform.position = new Vector3(rnd.Next(spawnArea.minX, spawnArea.maxX), rnd.Next(spawnArea.minX, spawnArea.maxX), 0);
        var rb = foodPrefab.GetComponent<Rigidbody2D>();
        rb.AddForce(new Vector2(UnityEngine.Random.Range(-100, 100), UnityEngine.Random.Range(-100, 100)));

        availableFood.Value++;
    }
    public GameObject SpawnFoodAt(Vector3 pos)
    {
        if (!IsServer)
            return null;
        
        var foodPrefab = Instantiate(food_Prefab);
        foodPrefab.GetComponent<NetworkObject>().Spawn();

        foodPrefab.transform.position = new Vector3(pos.x, pos.y, 0);

        availableFood.Value++;
        return foodPrefab;
    }
}
