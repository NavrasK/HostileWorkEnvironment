using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StructureDefinitions
{
    public static List<T> Shuffle<T>(List<T> coll)
    {
        int n = coll.Count;
        for (int i = 0; i < n; i++)
        {
            int r = UnityEngine.Random.Range(i, n);
            T t = coll[r];
            coll[r] = coll[i];
            coll[i] = t;
        }
        return coll;
    }

    public enum TileTypes
    {
        Empty = 0,
        Deadend = 1,
        Straight = 2,
        Elbow = 3,
        T = 4,
        Cross = 5
    }

    public static List<(int, int)> TileList = new List<(int, int)>();
}