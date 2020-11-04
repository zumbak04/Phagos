using UnityEngine;

[System.Serializable]
public class Skill
{
    [HideInInspector]
    public string name;
    public float currentSkill;
    public float minSkill;
    public float maxSkill;

    public float staticMinSkill;
    public float staticMaxSkill;

    public Skill(string _name, float _currentSkill, float _minSkill, float _maxSkill)
    {
        name = _name;
        currentSkill = _currentSkill;
        minSkill = _minSkill;
        maxSkill = _maxSkill;
        staticMinSkill = minSkill;
        staticMaxSkill = maxSkill;
    }

    public float skillPercent
    {
        get
        {
            return currentSkill / maxSkill;
        }
        set
        {
            currentSkill = value * maxSkill;
        }
    }
}
