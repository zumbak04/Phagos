using System;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class Genome
{
    public int genomeID;
    [SerializeField]
    private int mutationCount = 0;

    public float[] weights;

    public Color color;

    [SerializeField]
    private Skill[] skills =
    {
        new Skill("Speed", GameManager.instance.minSkill,GameManager.instance.maxSkill, 0.1f),
        new Skill("Vision", GameManager.instance.minSkill,GameManager.instance.maxSkill, 1f),
        new Skill("Food", GameManager.instance.minSkill,GameManager.instance.maxSkill, 0.5f),
        new Skill("Attack", GameManager.instance.minSkill,GameManager.instance.maxSkill, 0.5f),
        new Skill("Size", 5, 5, 0.2f) //Disabled for now
    };

    public int SkillLevelSum
    {
        get => skills.Sum(x => x.Level);
    }

    public Skill SpeedSkill { get { return skills[0]; } set { skills[0] = value; } }
    public Skill VisionSkill { get { return skills[1]; } set { skills[1] = value; } }
    public Skill FoodSkill { get { return skills[2]; } set { skills[2] = value; } }
    public Skill AttackSkill { get { return skills[3]; } set { skills[3] = value; } }
    public Skill SizeSkill { get { return skills[4]; } set { skills[4] = value; } }

    public Genome(int size)
    {
        UpdateGenomeID();

        //Generates weights
        weights = new float[size];
        for (int i = 0; i < size; i++)
        {
            weights[i] = UnityEngine.Random.Range(-1f, 1f);
        }

        //Generates color
        color = GenerateColor();

        //Randomizes skills
        SpeedSkill.ChangeLevel(SkillLevelSum,UnityEngine.Random.Range(SpeedSkill.Min, SpeedSkill.Max));
        VisionSkill.ChangeLevel(SkillLevelSum, UnityEngine.Random.Range(VisionSkill.Min, VisionSkill.Max));
        FoodSkill.ChangeLevel(SkillLevelSum, UnityEngine.Random.Range(FoodSkill.Min, VisionSkill.Max));
        AttackSkill.ChangeLevel(SkillLevelSum, UnityEngine.Random.Range(AttackSkill.Min, VisionSkill.Max));
    }

    public Genome(Genome parent)
    {
        genomeID = parent.genomeID;
        mutationCount = parent.mutationCount;

        weights = new float[parent.weights.Length];
        Array.Copy(parent.weights, 0, weights, 0, parent.weights.Length);

        color = parent.color;

        skills = new Skill[parent.skills.Length];
        for(int i = 0;i < skills.Length; i++)
        {
            skills[i] = new Skill(parent.skills[i]);
        }
    }

    public void Mutate(float mutationFactor)
    {
        mutationCount++;
        if(mutationCount % GameManager.instance.mutationBeforeNewID == 0)
            UpdateGenomeID();

        for (int i = 0; i < weights.Length; i++)
        {
            if(UnityEngine.Random.value < 0.1) weights[i] += UnityEngine.Random.Range(-mutationFactor, mutationFactor);
        }
        foreach (Skill skill in skills)
        {
            int skillChange = 0;
            if (UnityEngine.Random.value < 0.1) skillChange = Convert.ToInt32(UnityEngine.Random.Range(-mutationFactor * skill.Max, mutationFactor * skill.Max));
            skill.ChangeLevel(SkillLevelSum, skillChange);
        }

        //color = Color.Lerp(color, GenerateColor(), 1f/GameManager.instance.mutationBeforeNewID);
        color = GenerateColor();
    }
    public void UpdateGenomeID()
    {
        GameManager.instance.recentGenomeID++;
        genomeID = GameManager.instance.recentGenomeID;
    }
    public Color GenerateColor()
    {
        Color color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f);
        color.r = 0.2f + 0.8f * skills[3].Percent;
        color.g = 0.2f + 0.8f * skills[2].Percent;
        color.b = 0.2f;
        return color;
    }
}
