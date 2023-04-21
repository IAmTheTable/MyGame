using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.Networking.Transport.Utilities;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public GameObject food_Prefab;
    public GameObject walls;

    public NetworkVariable<int> availableFood = new();
    // the area where the food can spawn
    public (int minX, int maxX, int minY, int maxY) spawnArea;

    public int MaxFoodCount = 50000;

    private static List<GameObject> food = new();

    public void Start()
    {
        walls = GameObject.Find("Walls");
        Debug.Log("Connected on client");
        var map = walls.GetComponent<EdgeCollider2D>().bounds;
        Debug.Log("Bounds: Max: " + map.max.ToString() + " Min: " + map.min.ToString() + " Size: " + map.size.ToString());
        // set the spawn area to the bounds of the map
        spawnArea = ((int)map.min.x, (int)map.max.x, (int)map.min.x, (int)map.max.x);
    }

    // Update is called once per frame

    DateTime nextFoodSpawn = DateTime.Now.AddSeconds(5);

    void FixedUpdate()
    {
        if (!IsServer)
            return;

        if (DateTime.Now > nextFoodSpawn && availableFood.Value < MaxFoodCount)
        {
            nextFoodSpawn = DateTime.Now.AddMilliseconds(50);
            SpawnFood();
        }
    }

    void SpawnFood()
    {
        var foodPrefab = Instantiate(food_Prefab);
        foodPrefab.GetComponent<NetworkObject>().Spawn();

        // asign a random pos within the spawn area
        var rnd = new System.Random();
        foodPrefab.transform.position = new Vector3(rnd.Next(spawnArea.minX, spawnArea.maxX), rnd.Next(spawnArea.minX, spawnArea.maxX), 2);
        foodPrefab.transform.localScale = foodPrefab.transform.localScale + new Vector3(0, 0, 4);
        


        food.Add(foodPrefab);
        availableFood.Value++;
    }
}
