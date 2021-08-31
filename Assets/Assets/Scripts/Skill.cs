using UnityEngine;

[System.Serializable]
public class Skill
{
    [SerializeField]
    private string _name;
    private int _min = 0;
    private int _max = 10;
    [SerializeField]
    private int _level = 5;
    private float _factor;

    public Skill(string name, int min, int max, float factor)
    {
        _name = name;
        _min = min;
        _max = max;
        _factor = factor;
    }
    public Skill(Skill parent)
    {
        _name = parent._name;
        _min = parent._min;
        _max = parent._max;
        _level = parent.Level;
        _factor = parent.Factor;
    }

    public float Percent
    {
        get => (float)Level / (float)_max;
    }
    public float Effect
    {
        get => Level * _factor;
    }
    public int Level {
        get => _level;
    }
    public float Factor
    {
        get => _factor;
    }
    public int Min
    {
        get => _min;
    }
    public int Max
    {
        get => _max;
    }

    public void ChangeLevel(int skillLevelSum, int change)
    {
        if(skillLevelSum + change <= GameManager.instance.maxSkillSum)
        {
            _level += change;
            _level = Mathf.Clamp(_level, _min, _max);
        }
    }
}
