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
        message =  $"{bacterium.Age}";
    }
}
