using UnityEngine;

[System.Serializable]
public class Skill
{
    [HideInInspector]
    public string name;
    public float value;
    public float min;
    public float max;

    public float staticMin;
    public float staticMax;

    public Skill(string _name, float _value, float _min, float _max)
    {
        name = _name;
        value = _value;
        min = _min;
        max = _max;
        staticMin = min;
        staticMax = max;
    }

    public float percent
    {
        get
        {
            return value / max;
        }
        set
        {
            this.value = value * max;
        }
    }
}
