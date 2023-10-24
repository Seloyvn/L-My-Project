using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionSpawn : Entity
{
    GameManager gameManager => GameManager.instance;

    private void Start()
    {
        IncreaseInitative(MaxInitative);
    }
    public override void GetTurn()
    {
        base.GetTurn();
        gameManager.SpawnMinions();
        IncreaseInitative(MaxInitative);
        EndTurn();
    }
}
