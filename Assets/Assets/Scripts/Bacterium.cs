using UnityEngine;

public class Bacterium : MonoBehaviour
{
    //Stats
    public float age = 0f;
    public float energy = 0f;
    public float energyCost = 0f;

    float startEnergy = 10f;
    public float divisionEnergy = 15f;

    static float immunityTimerMax = 0.1f;
    public float immunityTimer = immunityTimerMax;

    Rigidbody2D rigidBody;
    Collider2D collider;
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

    void Awake()
    {
        rigidBody = gameObject.GetComponent<Rigidbody2D>();
        collider = gameObject.GetComponent<Collider2D>();
        if (energy == 0) energy = startEnergy;
        divisionEnergy = GameManager.instance.defaultDivisionEnergy;
    }

    // Update is called once per frame
    void Update()
    {
        //Change angel depend on velocity
        transform.eulerAngles = new Vector3(0f, 0f, Mathf.Atan2(rigidBody.velocity.y, rigidBody.velocity.x) * Mathf.Rad2Deg - 90);

        //Timers
        age += Time.deltaTime;
        immunityTimer = Mathf.Max(0, immunityTimer - Time.deltaTime);
    }

    private void FixedUpdate()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, genome.skills[1].currentSkill);

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
            if (colliders[i].gameObject.tag == "Food" && genome.skills[2].currentSkill > 0)
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
                if (other.genome.skills[3].currentSkill < genome.skills[3].currentSkill)
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
        for (int i = 1; i < maxObject; i++)
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
        velocity += target * genome.skills[0].currentSkill;
        //To avoid skidding
        velocity *= 0.98f;
        rigidBody.velocity = velocity;

        SpendEnergy();
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (genome.skills[2].currentSkill == 0) return;
        if (collider.gameObject.tag == "Food")
        {
            Eat(genome.skills[2].currentSkill);
            Destroy(collider.gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collider)
    {
        //Is immune attack for second
        if (immunityTimer > 0) return;
        if (genome.skills[3].currentSkill == 0) return;
        if (collider.gameObject.TryGetComponent<Bacterium>(out Bacterium other))
        {
            //Is immune attack for second
            if (other.immunityTimer > 0) return;

            //Doesn't attack its own kind
            if (genome.genomeID == other.genome.genomeID) return;

            //Makes immuny to prevent counter attack
            other.immunityTimer += immunityTimerMax;

            //Steals energy
            float damage = Mathf.Max(0f, genome.skills[3].currentSkill);
            damage = Mathf.Min(damage, other.energy);
            other.energy -= damage;
            Eat(damage);

            if (other.energy <= 0f)
            {
                Debug.Log($"{gameObject.name}, {genome.genomeID} kills {collider.gameObject.name}, {other.genome.genomeID}");
                other.Die();
            }
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

        //Color
        gameObject.GetComponent<SpriteRenderer>().color = genome.color;

        //Size
        rigidBody.transform.localScale = new Vector3(genome.skills[4].currentSkill, genome.skills[4].currentSkill, genome.skills[4].currentSkill);
        rigidBody.mass = rigidBody.transform.localScale.x * rigidBody.transform.localScale.y * rigidBody.transform.localScale.z;
        divisionEnergy = GameManager.instance.defaultDivisionEnergy * rigidBody.mass;

        //Sets NN based on genome
        nn = new NN(maxObject, 8, maxObject);
        for(int i = 0; i < maxObject; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                nn.layers[0].weights[i, j] = genome.weights[i + j * maxObject];
            }
        }
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                nn.layers[1].weights[i, j] = genome.weights[i + j * 8 + maxObject * 8];
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
        energyCost = Time.deltaTime * (GameManager.instance.energyPerSecondLoss + rigidBody.velocity.magnitude * rigidBody.mass * GameManager.instance.energyPerSizeLoss);
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
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(gameObject.transform.position, genome.skills[1].currentSkill);
        }
    }
}
