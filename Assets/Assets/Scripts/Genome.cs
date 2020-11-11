using System;
using UnityEngine;

[System.Serializable]
public class Genome
{
    public int genomeID;
    int mutationBeforeNewID = GameManager.instance.mutationBeforeNewID;
    [SerializeField]
    int mutationCount = 0;

    public float[] weights;

    public Color color;

    static string speedName = "Speed Skill";
    static string visionName = "Vision Skill";
    static string foodName = "Food Skill";
    static string attackName = "Attack Skill";
    static string sizeName = "Size Skill";

    public Skill[] skills =
    {
        new Skill(speedName, 0.25f,0.1f,1f),
        new Skill(visionName, 5,2.5f,10f),
        new Skill(foodName, 2.5f,0,5),
        new Skill(attackName, 2.5f,0,5),
        new Skill(sizeName, 1,1,4f)
    };

    public Skill speedSkill { get { return skills[0]; } set { skills[0] = value; } }
    public Skill visionSkill { get { return skills[1]; } set { skills[1] = value; } }
    public Skill foodSkill { get { return skills[2]; } set { skills[2] = value; } }
    public Skill attackSkill { get { return skills[3]; } set { skills[3] = value; } }
    public Skill sizeSkill { get { return skills[4]; } set { skills[4] = value; } }

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
    }

    public Genome(Genome a)
    {
        genomeID = a.genomeID;
        mutationCount = a.mutationCount;

        weights = new float[a.weights.Length];
        Array.Copy(a.weights, 0, weights, 0, a.weights.Length);

        color = a.color;

        skills = new Skill[a.skills.Length];
        for(int i = 0;i < skills.Length; i++)
        {
            skills[i] = new Skill(a.skills[i].name, a.skills[i].currentSkill, a.skills[i].staticMinSkill, a.skills[i].staticMaxSkill);
        }
    }

    public void Mutate(float value)
    {
        mutationCount++;
        if(mutationCount % mutationBeforeNewID == 0)
            UpdateGenomeID();

        for (int i = 0; i < weights.Length; i++)
        {
            if(UnityEngine.Random.value < 0.1) weights[i] += UnityEngine.Random.Range(-value, value);
        }
        foreach (Skill skill in skills)
        {
            float skillChange = 0;
            if (UnityEngine.Random.value < 0.1) skillChange = UnityEngine.Random.Range(-value * skill.maxSkill, value * skill.maxSkill);
            skill.currentSkill += skillChange;
            skill.currentSkill = Mathf.Clamp(skill.currentSkill, skill.minSkill, skill.maxSkill);
        }

        //Size increases food and attack max-min caps
        foreach (Skill skill in skills)
        {
            if (skill.name == attackName || skill.name == foodName)
            {
                skill.maxSkill = skills[4].currentSkill * skill.staticMaxSkill;
                skill.minSkill = skills[4].currentSkill * skill.staticMinSkill;
                skill.currentSkill = skills[4].currentSkill * skill.currentSkill;
                skill.currentSkill = Mathf.Clamp(skill.currentSkill, skill.minSkill, skill.maxSkill); //Just to make sure
            }
        }

        //Food and attack sum shoudn't be above 1
        float limitFactor = GetLimitFactor(1, skills[2].skillPercent, skills[3].skillPercent);
        skills[2].skillPercent *= limitFactor;
        skills[3].skillPercent *= limitFactor;

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
        color.r = 0.1f + 0.9f * skills[3].skillPercent;
        color.g = 0.1f + 0.9f * skills[2].skillPercent;
        color.b = 0.1f;
        return color;
    }
    public float GetLimitFactor(float limit, float firstVar, float secondVar)
    {
        float factor = 1;
        if (firstVar + secondVar > limit)
        {
            factor = limit / (firstVar + secondVar);
        }
        return factor;
    }
}
