using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BacteriumTooltip : Tooltip
{
    Bacterium bacterium;

    public void Awake()
    {
        bacterium = GetComponent<Bacterium>();
    }
    public void Update()
    {
        message = string.Empty;
        message += $"Age: {string.Format("{0:0}", bacterium.Age)} sec(s)\n";
        message += $"Energy: {string.Format("{0:0.0}", bacterium.Energy)}\n";
        message += "\n";
        foreach(Skill skill in bacterium.Genome.Skills)
        {
            message += $"{skill.Name}: <b>{skill.Level}</b>\n";
        }
    }
}
