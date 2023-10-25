using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camp : Entity
{
    GameManager gamemanager => GameManager.instance;
    public GameObject[] campUnit;

    [HideInInspector]
    public bool fighting;

    private void Start()
    {
        gamemanager.AllEntity.Add(this);
    }
    public override void GetTurn()
    {
        base.GetTurn();
        SpawnCamp();
        IncreaseInitative(MaxInitative);
        EndTurn();
    }
    public virtual void SpawnCamp()
    {
        foreach(GameObject o in campUnit)
        {
            CampUnit c = Instantiate(o).GetComponent<CampUnit>();
            NetworkServer.Spawn(c.gameObject);
            (int, int) i = gamemanager.getCloseField(field, gamemanager.fields[64, 64]).coordinates;
            c.setfield(i.Item1,i.Item2);
            c.camp = this;
            c.SetLevel(gamemanager.avgChampLvl());
        }
    }

    public void DieCamp()
    {
        Initative = 600;
        SetInitative();
    }
}
