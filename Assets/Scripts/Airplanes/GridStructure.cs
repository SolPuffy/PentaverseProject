using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridBaseStructure
{
    public List<ColumnStructure> Row = new List<ColumnStructure>();
}
[System.Serializable]
public class ColumnStructure
{
    public List<int> Column = new List<int>();
}
