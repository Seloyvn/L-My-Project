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

    GameManager gamemanager => GameManager.instance;
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
                if (GameManager.Dist(f, field) <= 1 && f.unit == null && f.fieldType == FieldType.Ground && Initative + MoveCost <= MaxInitative)
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
    public virtual void setSelectedAction(int id)
    {
        SelectedAction = id;
    }
    public override void LoseHealth(int amount)
    {
        base.LoseHealth(amount);
        Healthbar.value =1f* CurrentHealth / MaxHealth;
    }
    [ClientRpc]
    public virtual void SetCooldowns() { }
}
public enum ChampionEnum:int
{
    Herbert=0,
}
