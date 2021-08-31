using UnityEngine;

[System.Serializable]
public class Skill
{
    [HideInInspector]
    public readonly string name;
    public readonly int min = 0;
    public readonly int max = 10;

    [SerializeField]
    private int _level;
    private float _factor;

    public Skill(string name, int level, int min, int max, float factor)
    {
        this.name = name;
        this.min = min;
        this.max = max;

        _level = level;
        _factor = factor;
    }
    public Skill(Skill parent)
    {
        name = parent.name;
        min = parent.min;
        max = parent.max;

        _level = parent.Level;
    }

    public float Percent
    {
        get => (float)Level / (float)max;
    }
    public float Effect
    {
        get => Level * _factor;
    }
    public int Level {
        get => _level;
    }

    public void ChangeLevel(int skillLevelSum, int change)
    {
        if(skillLevelSum + change <= GameManager.instance.maxSkillSum)
            _level += change;
    }
}
