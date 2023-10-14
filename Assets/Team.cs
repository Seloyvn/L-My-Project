using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team
{
    public Color color;
    public (int, int) spawnPoint;
    public Team(int id)
    {
        color = id == 1 ? Color.blue : Color.red;
    }
}
