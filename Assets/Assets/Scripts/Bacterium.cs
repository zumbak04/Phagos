using UnityEngine;
using System.Collections;

public class Bacterium : MonoBehaviour
{
    //Stats
    [SerializeField]
    float age = 0f;
    public float energy = 0f;
    [SerializeField]
    float energyCost = 0f;

    float startEnergy = 10f;
    [SerializeField]
    float divisionEnergy = 15f;

    static float immunityCooldownAdd = 0.1f;
    static float attackCooldownAdd = 1f;
    [SerializeField]
    float immunityCooldown = immunityCooldownAdd;
    [SerializeField]
    float attackCooldown = 0f;

    Rigidbody2D rigidBody;
    [SerializeField]
    NN nn;

    public Genome genome;

    //Objects around
    //0 is food
    //1 is enemy
    //2 is pray
    //3 is friend
    static int maxObject = 4;
    float[] inputs = new float[maxObject];
    float[] objectCount = new float[maxObject];
    Vector3[] objectVectors = new Vector3[maxObject];

    Vector2 acceleration;

    GameObject filterMouthObject;
    GameObject jawObject;
    GameObject flagellasObject;
    GameObject eyesObject;

    private bool takingDamageAnimation = false;

    [SerializeField]
    private float _size = 1;
    public float size
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

    void Awake()
    {
        rigidBody = gameObject.GetComponent<Rigidbody2D>();
        filterMouthObject = gameObject.transform.GetChild(0).gameObject;
        jawObject = gameObject.transform.GetChild(1).gameObject;
        flagellasObject = gameObject.transform.GetChild(2).gameObject;
        eyesObject = gameObject.transform.GetChild(3).gameObject;

        if (energy == 0) energy = startEnergy;
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
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, genome.VisionSkill.Effect);

        //Clears count and vectors
        for (int i = 0; i < maxObject; i++)
        {
            objectCount[i] = 0;
            objectVectors[i] = new Vector3(0f, 0f, 0f);
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
                objectCount[0]++;
                if (vectorToObject.magnitude < objectVectors[0].magnitude || objectVectors[0].magnitude == 0)
                    objectVectors[0] = vectorToObject;
            }
            else if (colliders[i].gameObject.TryGetComponent<Bacterium>(out Bacterium other))
            {
                if (other.genome.genomeID == genome.genomeID)
                {
                    objectCount[3]++;
                    objectVectors[3] += vectorToObject;
                }
                //Pray is another exception, finds direct vector to pray
                else if (other.genome.AttackSkill.Effect < genome.AttackSkill.Effect)
                {
                    objectCount[2]++;
                    if (vectorToObject.magnitude < objectVectors[2].magnitude || objectVectors[2].magnitude == 0)
                        objectVectors[2] = vectorToObject;
                }
                else
                {
                    objectCount[1]++;
                    objectVectors[1] += vectorToObject;
                }
            }
        }
        //Merges object count and vectors to create inputs for NN
        for (int i = 0; i < maxObject; i++)
        {
            if (objectCount[i] > 0)
            {
                //Food and pray are exceptions
                if (i != 0 && i != 2)
                    objectVectors[i] /= objectCount[i];
                inputs[i] = objectVectors[i].magnitude;
            }
            else
            {
                inputs[i] = 0f;
            }
        }

        //NN tells how to react to every object vector, merges them to set target
        float[] outputs = nn.Move(inputs);
        Vector2 target = new Vector2(0, 0);
        for (int i = 0; i < maxObject; i++)
        {
            if (objectCount[i] > 0)
            {
                Vector2 objectDirection = new Vector2(objectVectors[i].x, objectVectors[i].y);
                objectDirection.Normalize();
                target += objectDirection * outputs[i];
            }
        }
        if (target.magnitude > 1f) target.Normalize();

        //Moves bacterium to its target
        Vector2 velocity = rigidBody.velocity;
        acceleration = target * genome.SpeedSkill.Effect;
        velocity += acceleration;
        //To avoid skidding
        velocity *= 0.95f;
        rigidBody.velocity = velocity;

        SpendEnergy();
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
            attackCooldown += attackCooldownAdd;

            //Deals damage, steals energy
            float damage = Mathf.Max(0f,genome.AttackSkill.Effect);
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
        size = genome.SizeSkill.Effect;
        rigidBody.mass = GameManager.instance.CountMass(rigidBody);
        divisionEnergy = GameManager.instance.defaultDivisionEnergy * rigidBody.mass;

        //Sets NN based on genome
        nn = new NN(maxObject, 8, maxObject);
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
    }
    public void Division()
    {
        energy *= 0.5f;
        Genome childGenome = new Genome(genome);
        childGenome.Mutate(0.5f);
        Bacterium childBacterium = GameManager.instance.SpawnBacterium(rigidBody.transform.position,childGenome);
        childBacterium.energy = energy;
    }
    public void SpendEnergy()
    {
        //Spends energy
        energyCost = Time.deltaTime * (GameManager.instance.energyPerSecondLoss + rigidBody.mass * GameManager.instance.energyPerSizeLoss);
        energy -= energyCost;

        //If energy is low, dies
        if (energy <= 0f)
            Die();
    }
    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < objectVectors.Length; i++)
        {
            switch(i)
            {
                case 0:
                    Gizmos.color = Color.green;
                    break;
                case 1:
                    Gizmos.color = Color.red;
                    break;
                case 2:
                    Gizmos.color = Color.yellow;
                    break;
                case 3:
                    Gizmos.color = Color.blue;
                    break;
                default:
                    Gizmos.color = Color.white;
                    break;
            }
            Gizmos.DrawLine(gameObject.transform.position, objectVectors[i] + gameObject.transform.position);
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
        immunityCooldown += immunityCooldownAdd;

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

        float oldSize = size;
        float newSize = size * 0.85f;
        float changePerTick = (oldSize - newSize);
        //Grows to new size
        while (size > newSize)
        {
            size = size - changePerTick;
            yield return new WaitForSeconds(0.1f);
        }
        //Shrinks to old size
        while (size < oldSize)
        {
            size = size + changePerTick;
            yield return new WaitForSeconds(0.1f);
        }
        size = oldSize;

        takingDamageAnimation = false;
    }
}
