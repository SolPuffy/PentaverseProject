using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimmulationSettings : MonoBehaviour
{
    public int AmountOfSimulations;
    public str Rules;
    public enum str
    {
        Normal = 0,
        Passable = 1,
        Hybrid = 2,
        Bullet = 3
    }
}
