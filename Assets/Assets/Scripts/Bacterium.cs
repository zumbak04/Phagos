﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bacterium : MonoBehaviour
{
    //Stats
    [SerializeField]
    private float age = 0f;
    [SerializeField]
    private float energy = 0f;
    [SerializeField]
    private float energyCost = 0f;

    private float startEnergy = 10f;
    [SerializeField]
    private float divisionEnergy = 15f;

    [SerializeField]
    private float immunityCooldown;
    [SerializeField]
    private float attackCooldown = 0f;

    Rigidbody2D rigidBody;
    [SerializeField]
    NN nn;

    [SerializeField]
    private Genome genome;

    private float[] inputs;
    [SerializeField]
    private Target[] targets;

    private Vector2 acceleration = new Vector2();
    [SerializeField]
    private Vector2 birthPlace;

    private GameObject filterMouthObject;
    private GameObject jawObject;
    private GameObject flagellasObject;
    private GameObject eyesObject;

    private bool takingDamageAnimation = false;

    [SerializeField]
    private float _size = 1;

    public float Age => age;
    public float Energy => energy;
    public Genome Genome => genome;
    public float Size
    {
        get
        {
            return _size;
        }
        set
        {
            rigidBody.transform.localScale = new Vector3(value, value, value);
            _size = value;
        }
    }

    private void Awake()
    {
        rigidBody = gameObject.GetComponent<Rigidbody2D>();
        filterMouthObject = gameObject.transform.GetChild(0).gameObject;
        jawObject = gameObject.transform.GetChild(1).gameObject;
        flagellasObject = gameObject.transform.GetChild(2).gameObject;
        eyesObject = gameObject.transform.GetChild(3).gameObject;

        inputs = new float[GameManager._instance.maxObject];
        targets = new Target[GameManager._instance.maxObject];

        immunityCooldown = GameManager._instance.immunityCooldown;

        if (energy == 0) energy = startEnergy;
    }
    private void Start()
    {
        StartCoroutine(AITick());
    }

    // Update is called once per frame
    void Update()
    {
        //Change angel depend on velocity
        transform.eulerAngles = new Vector3(0f, 0f, Mathf.Atan2(rigidBody.velocity.y, rigidBody.velocity.x) * Mathf.Rad2Deg - 90);

        //Timers
        age += Time.deltaTime;
        immunityCooldown = Mathf.Max(0, immunityCooldown - Time.deltaTime);
        attackCooldown = Mathf.Max(0, attackCooldown - Time.deltaTime);
    }

    private void FixedUpdate()
    {
        //Moves bacterium to its target
        Vector2 velocity = rigidBody.velocity;
        velocity += acceleration;
        //To avoid skidding
        velocity *= 0.95f;
        rigidBody.velocity = velocity;

        SpendEnergy();
    }

    private IEnumerator AITick()
    {
        while (Application.isPlaying)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, genome.VisionSkill.Effect);

            //Clears count and vectors
            for (int i = 0; i < targets.Length; i++)
            {
                targets[i] = new Target(Vector3.zero, 0);
            }
            //Analyzes every object, finds vectors to centers of mass
            for (int i = 0; i < colliders.Length; i++)
            {
                Vector3 vectorToObject = colliders[i].gameObject.transform.position - transform.position;

                //Skips itself
                if (colliders[i].gameObject == gameObject) continue;
                //Food is exception, finds direct vector to food unless you can't eat it
                if (colliders[i].gameObject.tag == "Food" && genome.FoodSkill.Effect > 0)
                {
                    targets[0].count++;
                    if (vectorToObject.magnitude < targets[0].vector.magnitude || targets[0].vector.magnitude == 0)
                        targets[0].vector = vectorToObject;
                }
                else if (colliders[i].gameObject.TryGetComponent<Bacterium>(out Bacterium other))
                {
                    if (other.genome.genomeID == genome.genomeID)
                    {
                        targets[3].count++;
                        targets[3].vector += vectorToObject;
                    }
                    //Pray is another exception, finds direct vector to pray
                    else if (other.genome.AttackSkill.Effect < genome.AttackSkill.Effect)
                    {
                        targets[2].count++;
                        if (vectorToObject.magnitude < targets[2].vector.magnitude || targets[2].vector.magnitude == 0)
                            targets[2].vector = vectorToObject;
                    }
                    else
                    {
                        targets[1].count++;
                        targets[1].vector += vectorToObject;
                    }
                }
            }
            targets[4].vector = birthPlace;
            targets[4].count = 1;
            //Merges object count and vectors to create inputs for NN
            for (int i = 0; i < GameManager._instance.maxObject; i++)
            {
                if (targets[i].count > 0)
                {
                    //Food and pray are exceptions
                    if (i != 0 && i != 2)
                        targets[i].vector /= targets[i].count;
                    inputs[i] = targets[i].vector.magnitude;
                }
                else
                {
                    inputs[i] = 0f;
                }
            }

            //NN tells how to react to every object vector, merges them to set target
            float[] outputs = nn.Calculate(inputs);
            Vector2 target = new Vector2(0, 0);
            for (int i = 0; i < GameManager._instance.maxObject; i++)
            {
                if (targets[i].count > 0)
                {
                    Vector2 objectDirection = new Vector2(targets[i].vector.x, targets[i].vector.y);
                    objectDirection.Normalize();
                    target += objectDirection * outputs[i];
                }
            }
            if (target.magnitude > 1f) target.Normalize();

            acceleration = target * genome.SpeedSkill.Effect;

            yield return new WaitForSeconds(GameManager._instance.AITickDelay);
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (genome.AttackSkill.Effect == 0) return;
        if (collider.gameObject.tag == "Food")
        {
            Eat(genome.FoodSkill.Effect);
            Destroy(collider.gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collider)
    {
        //Skips collision if can't attack
        if (genome.AttackSkill.Effect <= 0) return;
        //Skips collision if attack is on cooldown
        if (attackCooldown > 0) return;
        //if (immunityCooldown > 0) return;
        if (collider.gameObject.TryGetComponent<Bacterium>(out Bacterium other))
        {
            //Skips if target is immune
            if (other.immunityCooldown > 0) return;
            //Doesn't attack its own kind
            if (genome.genomeID == other.genome.genomeID) return;

            //Adds attack cooldown
            attackCooldown += GameManager._instance.attackCooldown;

            //Deals damage, steals energy
            float damage = Mathf.Max(0f, genome.AttackSkill.Effect);
            damage = Mathf.Min(damage, other.energy);
            Eat(damage);
            other.TakeDamage(damage);
        }
    }

    private void Eat(float energyGain)
    {
        energy += energyGain;
        if (energy > divisionEnergy)
        {
            Division();
        }
    }
    public void Init(Genome _genome)
    {
        genome = _genome;

        //Body
        RecolorBody(genome.color);
        //Eyes
        //Color eyesColor = new Color(0.1f,0.1f, genome.visionSkill.percent); //Maybe later
        eyesObject.GetComponent<SpriteRenderer>().color = Color.black;
        eyesObject.transform.localScale = Vector3.one * genome.VisionSkill.Percent;

        //Disables/enables body parts
        if (genome.AttackSkill.Percent >= genome.FoodSkill.Percent)
        {
            jawObject.SetActive(true);
            filterMouthObject.SetActive(false);
        }
        else
        {
            jawObject.SetActive(false);
            filterMouthObject.SetActive(true);
        }

        //Size
        Size = genome.SizeSkill.Effect;
        rigidBody.mass = GameManager._instance.CountMass(rigidBody);
        divisionEnergy = GameManager._instance.defaultDivisionEnergy * rigidBody.mass;

        //Sets NN based on genome
        nn = new NN(GameManager._instance.maxObject, GameManager._instance.maxObject * 2, GameManager._instance.maxObject);
        int genomeWeightCounter = 0;
        for (int l = 0; l < nn.layers.Length - 1; l++)
        {
            for (int i = 0; i < nn.layers[l].neurons.Length; i++)
            {
                for (int j = 0; j < nn.layers[l].neurons[i].weights.Length; j++)
                {
                    nn.layers[l].neurons[i].weights[j] = genome.weights[genomeWeightCounter];
                    genomeWeightCounter++;
                }
            }
        }

        birthPlace = gameObject.transform.position;
    }
    public void Division()
    {
        energy *= 0.5f;
        Genome childGenome = new Genome(genome);
        childGenome.Mutate(0.5f);
        Bacterium childBacterium = GameManager._instance.SpawnBacterium(rigidBody.transform.position, childGenome);
        childBacterium.energy = energy;
    }
    public void SpendEnergy()
    {
        //Spends energy
        energyCost = Time.deltaTime * (GameManager._instance.energyPerSecondLoss + rigidBody.mass * GameManager._instance.energyPerSizeLoss);
        energy -= energyCost;

        //If energy is low, dies
        if (energy <= 0f)
            Die();
    }
    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            switch (i)
            {
                case 0:
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(gameObject.transform.position, targets[i].vector + gameObject.transform.position);
                    break;
                case 1:
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(gameObject.transform.position, targets[i].vector + gameObject.transform.position);
                    break;
                case 2:
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(gameObject.transform.position, targets[i].vector + gameObject.transform.position);
                    break;
                case 3:
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(gameObject.transform.position, targets[i].vector + gameObject.transform.position);
                    break;
                case 4:
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(gameObject.transform.position, targets[i].vector);
                    break;
                default:
                    Gizmos.color = Color.white;
                    break;
            }
        }
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(gameObject.transform.position, genome.VisionSkill.Effect);
        Vector3 acceleration3 = acceleration;
        Gizmos.DrawLine(gameObject.transform.position, acceleration3 + gameObject.transform.position);
    }
    public void RecolorBody(Color color)
    {
        //Base body
        gameObject.GetComponent<SpriteRenderer>().color = color;
        //Jaw
        jawObject.GetComponent<SpriteRenderer>().color = color;
        //Filter Mouth
        filterMouthObject.GetComponent<SpriteRenderer>().color = color;
        //Flagellas
        flagellasObject.GetComponent<SpriteRenderer>().color = color;

    }

    public void TakeDamage(float damage)
    {
        immunityCooldown += GameManager._instance.immunityCooldown;

        energy -= damage;

        if (energy <= 0f)
        {
            Die();
            return;
        }

        if (!takingDamageAnimation) StartCoroutine(SizeOnDamage());
    }
    IEnumerator SizeOnDamage()
    {
        takingDamageAnimation = true;

        float oldSize = Size;
        float newSize = Size * 0.85f;
        float changePerTick = (oldSize - newSize);
        //Grows to new size
        while (Size > newSize)
        {
            Size = Size - changePerTick;
            yield return new WaitForSeconds(0.1f);
        }
        //Shrinks to old size
        while (Size < oldSize)
        {
            Size = Size + changePerTick;
            yield return new WaitForSeconds(0.1f);
        }
        Size = oldSize;

        takingDamageAnimation = false;
    }
}
