using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Minion : Unit
{
    (int, int)[] paths;
    int path = 0;
    (int, int) dest {
        get
        {
            IEnumerable<Field> f = gamemanager.allFields.Where(f => GameManager.Dist(f, field) <= 4 && f.unit != null && 
            f.unit.team != team&&f.unit.hasVision).OrderBy(f => GameManager.Dist(f, field));
            if (f.Any())
                return f.First().coordinates;
            return paths[path];
        }
    }
    (int, int) start => gamemanager.allFields.Where(f => GameManager.Dist(f, field) <=4 && 
    f.unit != null && f.unit.team != team).Any()?field.coordinates: paths[path - 1];
    (int, int) vec => (dest.Item1 - start.Item1, dest.Item2 - start.Item2);
    GameManager gamemanager => GameManager.instance;
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
        if(gamemanager.AITurns)
            yield return new WaitForSeconds(0.01f);

        if (GameManager.Dist(field, gamemanager.fields[paths[path].Item1, paths[path].Item2]) <= 2 && path < paths.Length - 1)
            path++;
        IEnumerable<Field> f= gamemanager.allFields.Where(f => GameManager.Dist(f, field) <= 1 && f.unit != null && f.unit.team != team);

        if (f.Any())
            tryAttack(f.First());
        else
            TryMove();
    }
    public void TryMove()
    {
        int x = dest.Item1 - field.coordinates.Item1;
        int y = dest.Item2 - field.coordinates.Item2;


        if (vec.Item1 >= 0 && vec.Item2 >= 0)
        {
            if (field.coordinates.Item2 >= ((float)vec.Item2 / vec.Item1) * (field.coordinates.Item1 - start.Item1) + start.Item2)
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
            if (field.coordinates.Item2 >= ((float)vec.Item2 / vec.Item1) * (field.coordinates.Item1 - start.Item1) + start.Item2)
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
            else if (field.coordinates.Item2 > ((float)vec.Item2 / vec.Item1) * (field.coordinates.Item1 - start.Item1) + start.Item2)
            {
                if (!tryMove(gamemanager.getField((field.coordinates.Item1 - 1, field.coordinates.Item2 ))))
                    if (!tryMove(gamemanager.getField((field.coordinates.Item1, field.coordinates.Item2 + 1))))
                    {
                        EndTurn();
                        return;
                    }
            }
            else if (!tryMove(gamemanager.getField((field.coordinates.Item1 , field.coordinates.Item2 + 1))))
                if (!tryMove(gamemanager.getField((field.coordinates.Item1 - 1, field.coordinates.Item2 ))))
                {
                    EndTurn();
                    return;
                }
        }

        else if (vec.Item1 < 0 && vec.Item2 < 0)
        {
            if (field.coordinates.Item2 >= ((float)vec.Item2 / vec.Item1) * (field.coordinates.Item1 - start.Item1) + start.Item2)
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
    public override void Die(Unit attacker)
    {
       List<Champion> ch=GetExpChampions(attacker);

        foreach (Champion c in ch)
            c.getExp((int)(40*(1+0.3f*ch.Count)/ch.Count));

        field.unit = null;

        RPCDie();
    }
    public void setPath((int, int)[] t)
    {
        paths = t;
    }
    public override void EndTurn()
    {
        Initative += Random.Range(0,5);
        base.EndTurn();
    }
}
