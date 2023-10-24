using Mirror;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HerbertScript : Champion
{
    GameManager gamemanager => GameManager.instance;

    [SyncVar]
    public int ChargeCd = 0;
    public bool CanCharge => ChargeCd <= 0;
    [SyncVar]
    public int ExplodeCd = 0;
    public bool CanExplode => ExplodeCd <= 0;
    [SyncVar]
    public bool Exploding=false;
    public Button ChargeButton;
    public TMP_Text ChargeCdLabel;
    public Button ExplodeButton;
    public TMP_Text ExplodeCdLabel;

    public override void hoverField()
    {
        Field f = gamemanager.getCurrentField();
        if (f == null||!isOwned)
        {
            gamemanager.Hover(null, HoverMode.None);
            return;
        }
        switch (SelectedAction)
        {
            case 2:
                if (f.unit == null || f.unit.team == team)
                {
                    gamemanager.Hover(f, HoverMode.NotMove);
                    return;
                }
                if (GameManager.Dist(f, field) <= 6 && CanCharge)
                {
                    Field fl=gamemanager.getCloseField(f, field);
                    if(fl!=null)
                        gamemanager.Hover(fl, HoverMode.CanMove);
                    else
                        gamemanager.Hover(f, HoverMode.NotMove);
                }
                else
                    gamemanager.Hover(f, HoverMode.NotMove);
                return;
            case 3:
                    gamemanager.Hover(field, HoverMode.CanMove);
                return;
        }
        base.hoverField();
    }
    public override void Click(Field field)
    {
        switch (SelectedAction)
        {
            case 0:
                CmdMove(field.coordinates.Item1, field.coordinates.Item2);
                return;
            case 1:

                CmdtryAttack(field.coordinates.Item1, field.coordinates.Item2);
                return;
            case 2:
                tryAbility0(field);
                return;
            case 3:
                tryAbility1();
                return;
        }
    }
    public override void GetTurn()
    {
        base.GetTurn();
        if (Exploding)
            Ability1();        
    }
    public void tryAbility0(Field field)
    {
        CmdtryAbility0((int)field.coordinates.Item1, (int)field.coordinates.Item2);
    }
    [Command]
    public void CmdtryAbility0(int x, int y)
    {
        if (gamemanager.CurrentTurn != this)
            return;
        Field f = gamemanager.getField((x, y));
        if (f.unit != null && f.unit.team != team && GameManager.Dist(f, field) <= 6&&CanCharge)
        {
            Field df = gamemanager.getCloseField(f, field);
            if (df != null)
                if (Initative <= MaxInitative - 50)
                {
                    IncreaseInitative(50);
                    ChargeCd = 60;
                    SetCooldowns();
                    setfield(df.coordinates.Item1, df.coordinates.Item2);
                    DealDamage(f.unit,Damage+1,DamageType.Physical);
                    gamemanager.ShowCurrentInitative(Initative, MaxInitative);
                    SetInitative();
                }
        }
    }

    public void tryAbility1()
    {
        CmdtryAbility1();
    }
    [Command]
    public void CmdtryAbility1()
    {
        if (gamemanager.CurrentTurn != this)
            return;

        if (CanCharge)
        {
            if (Initative <= MaxInitative - 30)
            {
                IncreaseInitative(30);
                Exploding = true;
                foreach (Field f in gamemanager.allFields.Where(f => GameManager.Dist(f, field) <= 4))
                    gamemanager.StartHighlight(f.coordinates.Item1, f.coordinates.Item2);
                ExplodeCd = 300;
                SetCooldowns();
                EndTurn();
            }
        }
    }
    public void Ability1()
    {
        Exploding = false;
        int d =(int)( Damage * 1.5f+100+Level*20);
        foreach (Field f in gamemanager.allFields.Where(f => GameManager.Dist(f, field) == 1))
        {
            gamemanager.StopHighlight(f.coordinates.Item1, f.coordinates.Item2);
            if (f.unit != null && f.unit.team != team)
                DealDamage(f.unit, d, DamageType.Physical);
        }
        foreach (Field f in gamemanager.allFields.Where(f => GameManager.Dist(f, field) == 2))
        {
            gamemanager.StopHighlight(f.coordinates.Item1, f.coordinates.Item2);
            if (f.unit != null && f.unit.team != team)
                DealDamage(f.unit,d*0.75f, DamageType.Physical);
        }
        foreach (Field f in gamemanager.allFields.Where(f => GameManager.Dist(f, field) == 3))
        {
            gamemanager.StopHighlight(f.coordinates.Item1, f.coordinates.Item2);
            if (f.unit != null && f.unit.team != team)
                DealDamage(f.unit, Damage*0.5f, DamageType.Physical);
        }
        foreach (Field f in gamemanager.allFields.Where(f => GameManager.Dist(f, field) == 4))
        {
            gamemanager.StopHighlight(f.coordinates.Item1, f.coordinates.Item2);
            if (f.unit != null && f.unit.team != team)
                DealDamage(f.unit, d * 0.25f, DamageType.Physical);
        }
    }
    public override void IncreaseInitative(int amount)
    {
        base.IncreaseInitative(amount);
        if (!CanCharge)
        {
            ChargeCd -= amount;
            if (ChargeCd <= 0)            
                ChargeCd = 0;            
        }
        if (!CanExplode)
        {
            ExplodeCd -= amount;
            if (ExplodeCd <= 0)
                ExplodeCd = 0;            
        }
        SetCooldowns();
    }
    [ClientRpc]
    public override void SetCooldowns()
    {
        if(CanCharge)
            ChargeCdLabel.text = "";
        else
            ChargeCdLabel.text = ChargeCd.ToString();
        ChargeButton.interactable = CanCharge;

        if(CanExplode)
            ExplodeCdLabel.text = "";
        else
            ExplodeCdLabel.text = ExplodeCd.ToString();
        ExplodeButton.interactable = CanExplode;
    }
}
