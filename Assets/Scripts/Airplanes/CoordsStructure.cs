using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CoordsStructure
{
    public int X;
    public int Y;

    public override string ToString()
    {
        string ret;
        ret = X + ";" + Y;
        return ret;
    }

}
