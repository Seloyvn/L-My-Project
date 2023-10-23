using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Base:Entity
{
    GameManager gameManager => GameManager.instance;

    public override void setVisionFields()
    {
        visionfields.AddRange(gameManager.allFields.Where(f=>GameManager.SDist(f,field)<=Mathf.Pow(39,2)));
    }
    public override IEnumerable<Field> getVisionFields()
    {
        if (visionfields.Count() == 0)
            setVisionFields();
        return base.getVisionFields();
    }
    public override void GetTurn()
    {
        base.GetTurn();
        IncreaseInitative(MaxInitative);
        EndTurn();
    }
}
