using System;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Genome
{
    public int genomeID;
    public int mutationBeforeNewID = 10;
    public int mutationCount = 0;

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
        new Skill(visionName, 5,5,15),
        new Skill(foodName, 2,0,5),
        new Skill(attackName, 2,0,5),
        new Skill(sizeName, 1,1,4f)
    };

    Vector3 localSize = new Vector3();

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
            if (skill.name == attackName)
                skills[2].currentSkill -= skillChange;
            if (skill.name == foodName)
                skills[3].currentSkill -= skillChange;
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
                skill.currentSkill *= skills[4].currentSkill;
                skill.currentSkill = Mathf.Clamp(skill.currentSkill, skill.minSkill, skill.maxSkill);
            }
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
        float attacSkillPercent = skills[3].currentSkill * (1 / skills[3].maxSkill);
        float foodSkillPercent = skills[2].currentSkill * (1 / skills[2].maxSkill);
        color.r = 0.25f + 0.75f * attacSkillPercent;
        color.g = 0.25f + 0.75f * foodSkillPercent;
        color.b = 0.25f;
        return color;
    }
}
