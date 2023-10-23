using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Champion : Unit
{
    public GameObject ControllUi;
    public Slider Healthbar;
    [HideInInspector]
    public int SelectedAction = 0;

    [SyncVar]
    public int Experience;

    public int requiredExperience => Level * 50 + 100;

    GameManager gamemanager => GameManager.instance;
    public int MoveDist => (MaxInitative- Initative) / MoveCost;
    public override void Select()
    {
        base.Select();
        ControllUi.SetActive(isOwned && selectedunit == this);
        SelectedAction = 0;
    }
    public override void GetTurn()
    {
        base.GetTurn();
    }
    public override void hoverField()
    {
        Field f = gamemanager.getCurrentField();
        if (f == null)
        {
            gamemanager.Hover(null, HoverMode.None);
            return;
        }
        switch (SelectedAction)
        {
            case 0:
                if (GameManager.Dist(f, field) <= MoveDist && f.unit == null && f.fieldType == FieldType.Ground && Initative + MoveCost <= MaxInitative)
                    gamemanager.Hover(f, HoverMode.CanMove);
                else
                    gamemanager.Hover(f, HoverMode.NotMove);
                return;
            case 1:
                if (GameManager.Dist(f, field) == 1 && f.unit != null && f.unit.team != team && Initative + AttackCost <= MaxInitative)
                    gamemanager.Hover(f, HoverMode.CanMove);
                else
                    gamemanager.Hover(f, HoverMode.NotMove);
                return;
        }
        gamemanager.Hover(null, HoverMode.None);
    }
    public override bool tryMove(Field f)
    {
        if (f == field)
            return true;
        if (GameManager.Dist(f, field) > MoveDist)
            return false;

        int x = f.coordinates.Item1 - field.coordinates.Item1;
        int y = f.coordinates.Item2 - field.coordinates.Item2;
        if (x != 0)
        {
            if (!base.tryMove(gamemanager.getField((field.coordinates.Item1 + (x > 0 ? 1 : -1), field.coordinates.Item2))))
                if (y != 0)
                    if (!base.tryMove(gamemanager.getField((field.coordinates.Item1, field.coordinates.Item2 + (y > 0 ? 1 : -1)))))
                        return false;
        }
        else if (y != 0)
                if (!base.tryMove(gamemanager.getField((field.coordinates.Item1, field.coordinates.Item2 + (y > 0 ? 1 : -1)))))
                    return false;

        StartCoroutine(DelayMove(f));
        return true;
    }
    IEnumerator DelayMove(Field f)
    {
        yield return new WaitForSeconds(0.02f);
        tryMove(f);
    }
    public virtual void setSelectedAction(int id)
    {
        SelectedAction = id;
    }
    
    public override void LoseHealth(Unit attacker,int amount)
    {
        base.LoseHealth(attacker, amount);
        Healthbar.value =1f* CurrentHealth / MaxHealth;
    }
    [ClientRpc]
    public virtual void SetCooldowns() { }
    public void getExp(int amount)
    {
        Experience += amount;

        while (Experience >= requiredExperience)
            LevelUp();
    }
}
public enum ChampionEnum:int
{
    Herbert=0,
}
