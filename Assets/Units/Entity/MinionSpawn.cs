using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionSpawn : Entity
{
    GameManager gamemanager => GameManager.instance;

    private void Start()
    {
        gamemanager.AllEntity.Add(this);
        IncreaseInitative(MaxInitative);
    }
    public override void GetTurn()
    {
        base.GetTurn();
        gamemanager.SpawnMinions();
        IncreaseInitative(MaxInitative);
        EndTurn();
    }
}
