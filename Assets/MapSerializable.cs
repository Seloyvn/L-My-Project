using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapSerializable
{
    public int size => heights.GetLength(0);
    public int[,] heights;
    public MapSerializable() { }
    public MapSerializable(int x)
    {
        heights = new int[x,x];
    }
}
