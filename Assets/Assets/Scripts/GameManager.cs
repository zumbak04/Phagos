using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager _instance = null;

    public Vector2 gameArea;
    public int StartNumberOfFood = 2000;
    public int StartNumberOfBacteria = 400;
    public int mutationBeforeNewID = 10;
    public float energyPerSecondLoss = 1f;
    public float energyPerSizeLoss = 0.2f;
    public float defaultDivisionEnergy = 15f;
    public int maxSkill = 10;
    public int minSkill = 0;
    public int maxSkillSum = 30;

    //Balance factors
    //Attack deals more damage
    //Mass spends more energy
    float massFactor = 0.75f;
    public float immunityCooldown = 0.1f;
    public float attackCooldown = 1f;

    public int recentGenomeID = 0;
    public int recentBacteriumID = 0;

    Transform foodHolder;
    Transform bacteriumHolder;

    private float spawnFoodTickDelay = 0.25f;
    public float AITickDelay = 0.5f;

    //0 is food
    //1 is enemy
    //2 is pray
    //3 is friend
    //4 is birth place
    public int maxObject = 5;

    private int SpawnFoodPerTick => Mathf.RoundToInt(Mathf.Max(StartNumberOfFood / 15, 1) * spawnFoodTickDelay);
    public int NumberOfNeurons => maxObject * maxObject * 4;
    public CameraController camCtrl;
    public float LargestGameAreaEdge
    {
        get
        {
            if(gameArea.x > gameArea.y)
            {
                return gameArea.x;
            }
            else
            {
                return gameArea.y;
            }
        }
    }

    void Start()
    {
        if (_instance == null)
            _instance = this;
        else if (_instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        camCtrl = gameObject.GetComponent<CameraController>();

        foodHolder = new GameObject("Food Holder").transform;
        bacteriumHolder = new GameObject("Bacterium Holder").transform;

        for (int i = 0; i < StartNumberOfBacteria; i++)
        {
            Genome genome = new Genome(NumberOfNeurons);
            Vector2 spawnPoint = new Vector2(Random.Range(-gameArea.x, gameArea.x), Random.Range(-gameArea.y, gameArea.y));
            SpawnBacterium(spawnPoint, genome);
        }
        for (int i = 0; i < StartNumberOfFood; i++)
        {
            SpawnFoodAtRandomLocation();
        }
        StartCoroutine(SpawnFoodTick());
    }

    private IEnumerator SpawnFoodTick()
    {
        while (Application.isPlaying)
        {
            for(int i = 0; i < SpawnFoodPerTick; i++)
                SpawnFoodAtRandomLocation();

            yield return new WaitForSeconds(spawnFoodTickDelay);
        }
    }

    public Bacterium SpawnBacterium(Vector2 spawnPoint, Genome genome)
    {
        GameObject newObject = Instantiate(GameAssets.instance.bacterium, spawnPoint, Quaternion.identity);
        Bacterium newBacterium = newObject.GetComponent<Bacterium>();
        newBacterium.Init(genome);

        //Names
        newObject.name = $"Bacterium {recentBacteriumID}";
        ++recentBacteriumID;
        newObject.transform.SetParent(bacteriumHolder);

        return newBacterium;
    }
    public void SpawnFoodAtRandomLocation()
    {
        Vector2 spawnPoint = new Vector2(Random.Range(-gameArea.x, gameArea.x), Random.Range(-gameArea.y, gameArea.y));

        GameObject food = Instantiate(GameAssets.instance.food, spawnPoint, Quaternion.identity);

        food.name = $"Food";
        food.transform.SetParent(foodHolder);
    }
    public float CountMass(Rigidbody2D rigidBody)
    {
        float mass = rigidBody.transform.localScale.x * rigidBody.transform.localScale.y * massFactor;
        return mass;
    }
}
