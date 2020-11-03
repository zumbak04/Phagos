﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    public GameObject bacterium;
    public GameObject foodPrefab;

    public Vector2 gameArea;
    public int StartNumberOfFood = 1000;
    public int StartNumberOfBacteria = 100;
    public int mutationBeforeNewID = 20;
    public float energyPerSecondLoss = 0.2f;
    public float energyPerSizeLoss = 0.2f;
    public float defaultDivisionEnergy = 15f;

    public int recentGenomeID = 0;
    public int recentBacteriumID = 0;

    Transform foodHolder;
    Transform bacteriumHolder;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        foodHolder = new GameObject("Food Holder").transform;
        bacteriumHolder = new GameObject("Bacterium Holder").transform;

        for (int i = 0; i < StartNumberOfBacteria; i++)
        {
            Genome genome = new Genome(64);
            Vector2 spawnPoint = new Vector2(Random.Range(-gameArea.x, gameArea.x), Random.Range(-gameArea.y, gameArea.y));
            SpawnBacterium(spawnPoint, genome);
        }
        for (int i = 0; i < StartNumberOfFood; i++)
        {
            Vector2 spawnPoint = new Vector2(Random.Range(-gameArea.x, gameArea.x), Random.Range(-gameArea.y, gameArea.y));
            SpawnFood(spawnPoint);
        }

        Camera camera = gameObject.GetComponent<Camera>();
        camera.orthographicSize = gameArea.x;
    }

    void FixedUpdate()
    {
        for(int foodSpawns = Mathf.Max(StartNumberOfFood/1000,1); foodSpawns > 0; foodSpawns--)
        {
            Vector2 spawnPoint = new Vector2(Random.Range(-gameArea.x, gameArea.x), Random.Range(-gameArea.y, gameArea.y));
            SpawnFood(spawnPoint);
        }
    }

    public Bacterium SpawnBacterium(Vector2 spawnPoint, Genome genome)
    {
        GameObject newObject = Instantiate(bacterium, spawnPoint, Quaternion.identity);
        Bacterium newBacterium = newObject.GetComponent<Bacterium>();
        newBacterium.Init(genome);

        //Names
        newObject.name = $"Bacterium {recentBacteriumID}";
        ++recentBacteriumID;
        newObject.transform.SetParent(bacteriumHolder);

        return newBacterium;
    }
    public void SpawnFood(Vector2 spawnPoint)
    {
        GameObject food = Instantiate(foodPrefab, spawnPoint, Quaternion.identity);

        food.name = $"Food";
        food.transform.SetParent(foodHolder);
    }
}
