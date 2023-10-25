using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CampUnit : Unit
{
    [HideInInspector]
    public Camp camp;
    public bool MainUnit;
    GameManager gamemanager => GameManager.instance;
    Field startField;
    void Start()
    {
        gamemanager.AllEntity.Add(this);
        startField = field;
    }
    Unit target;
    (int, int) dest
    {
        get
        {
            return target?.field.coordinates ?? startField.coordinates;
        }
    }
    (int, int) vec => (dest.Item1 - field.coordinates.Item1, dest.Item2 - field.coordinates.Item2);

    public void SetLevel(int l)
    {
        while (Level < l)
            LevelUp();
    }
    public override void GetTurn()
    {
        base.GetTurn();
        StartCoroutine(DoTurn());
    }
    IEnumerator DoTurn()
    {
        SetTarget();

        if (!camp.fighting && startField == field)
        {
            EndTurn();
        }
        if (gamemanager.AITurns)
            yield return new WaitForSeconds(0.01f);

        IEnumerable<Field> f = gamemanager.allFields.Where(f => GameManager.Dist(f, field) <= 1 && f.unit != null && f.unit.team != null);

        if (f.Any())
            tryAttack(f.First());
        else
            TryMove();
    }
    void SetTarget()
    {
        target = gamemanager.allFields.Where(f => f.unit != null && f.unit.team != null&&GameManager.Dist(f,field)<=4).FirstOrDefault()?.unit;
    }
    public void TryMove()
    {
        int x = dest.Item1 - field.coordinates.Item1;
        int y = dest.Item2 - field.coordinates.Item2;


        if (vec.Item1 >= 0 && vec.Item2 >= 0)
        {
            if (field.coordinates.Item2 >= ((float)vec.Item2 / vec.Item1) * (field.coordinates.Item1 - field.coordinates.Item1) + field.coordinates.Item2)
            {
                if (!tryMove(gamemanager.getField((field.coordinates.Item1 + 1, field.coordinates.Item2))))
                    if (!tryMove(gamemanager.getField((field.coordinates.Item1, field.coordinates.Item2 + 1))))
                    {
                        EndTurn();
                        return;
                    }
            }
            else if (!tryMove(gamemanager.getField((field.coordinates.Item1, field.coordinates.Item2 + 1))))
                if (!tryMove(gamemanager.getField((field.coordinates.Item1 + 1, field.coordinates.Item2))))
                {
                    EndTurn();
                    return;
                }
        }

        else if (vec.Item1 >= 0 && vec.Item2 < 0)
        {
            if (field.coordinates.Item2 >= ((float)vec.Item2 / vec.Item1) * (field.coordinates.Item1 - field.coordinates.Item1) + field.coordinates.Item2)
            {
                if (!tryMove(gamemanager.getField((field.coordinates.Item1, field.coordinates.Item2 - 1))))
                    if (!tryMove(gamemanager.getField((field.coordinates.Item1 + 1, field.coordinates.Item2))))
                    {
                        EndTurn();
                        return;
                    }
            }
            else if (!tryMove(gamemanager.getField((field.coordinates.Item1 + 1, field.coordinates.Item2))))
                if (!tryMove(gamemanager.getField((field.coordinates.Item1, field.coordinates.Item2 - 1))))
                {
                    EndTurn();
                    return;
                }
        }

        else if (vec.Item1 < 0 && vec.Item2 >= 0)
        {
            if (vec.Item2 == 0)
            {
                if (!tryMove(gamemanager.getField((field.coordinates.Item1 - 1, field.coordinates.Item2))))
                {
                    EndTurn();
                    return;
                }
            }
            else if (field.coordinates.Item2 > ((float)vec.Item2 / vec.Item1) * (field.coordinates.Item1 - field.coordinates.Item1) + field.coordinates.Item2)
            {
                if (!tryMove(gamemanager.getField((field.coordinates.Item1 - 1, field.coordinates.Item2))))
                    if (!tryMove(gamemanager.getField((field.coordinates.Item1, field.coordinates.Item2 + 1))))
                    {
                        EndTurn();
                        return;
                    }
            }
            else if (!tryMove(gamemanager.getField((field.coordinates.Item1, field.coordinates.Item2 + 1))))
                if (!tryMove(gamemanager.getField((field.coordinates.Item1 - 1, field.coordinates.Item2))))
                {
                    EndTurn();
                    return;
                }
        }

        else if (vec.Item1 < 0 && vec.Item2 < 0)
        {
            if (field.coordinates.Item2 >= ((float)vec.Item2 / vec.Item1) * (field.coordinates.Item1 - field.coordinates.Item1) + field.coordinates.Item2)
            {
                if (!tryMove(gamemanager.getField((field.coordinates.Item1, field.coordinates.Item2 - 1))))
                    if (!tryMove(gamemanager.getField((field.coordinates.Item1 - 1, field.coordinates.Item2))))
                    {
                        EndTurn();
                        return;
                    }
            }
            else if (!tryMove(gamemanager.getField((field.coordinates.Item1 - 1, field.coordinates.Item2))))
                if (!tryMove(gamemanager.getField((field.coordinates.Item1, field.coordinates.Item2 - 1))))
                {
                    EndTurn();
                    return;
                }
        }
        StartCoroutine(DoTurn());
    }
    public override bool tryAttack(Field f)
    {
        if (!base.tryAttack(f))
        {
            EndTurn();
            return false;
        }
        StartCoroutine(DoTurn());
        return true;
    }
    public override void EndTurn()
    {
        Initative = MaxInitative;
        base.EndTurn();
    }
    public override void LoseHealth(Unit attacker, int amount)
    {
        camp.fighting = true;
        base.LoseHealth(attacker, amount);
    }
    public override void Die(Unit attacker)
    {
        if (MainUnit)
            camp.DieCamp();

        if(attacker is Champion c)
            c.getExp(40);

        field.unit = null;

        RPCDie();
    }
}
