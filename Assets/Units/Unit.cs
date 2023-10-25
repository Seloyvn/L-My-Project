using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public abstract class Unit : Entity
{
    public static Unit selectedunit;
    public GameObject Ui;
    public GameObject Model;
    public Slider Healthbar;

    #region stats
    [SyncVar]
    public int Level;
    [SyncVar]
    public int MaxHealth;
    public int HealthPerLevel;
    [SyncVar]
    public int CurrentHealth;

    [SyncVar]
    public int Damage;
    public int DamagePerLevel;

    [SyncVar]
    public int Armor;
    public int ArmorPerLevel;

    [SyncVar]
    public int MagicResistance;
    public int MagicResistancePerLevel;

    [SyncVar]
    public float MoveSpeed;
    public int MoveCost => Mathf.CeilToInt(100 / MoveSpeed);
    [SyncVar]
    public float AttackSpeed;
    public int AttackCost => Mathf.CeilToInt(100 / AttackSpeed);
    #endregion

    GameManager gamemanager => GameManager.instance;

    private void Start()
    {
        gamemanager.AllEntity.Add(this);
    }
    [ClientRpc]
    public void setTeam(int i)
    {
        team = i == 0 ? gamemanager.team1 : gamemanager.team2;
        InitativeIcon.GetComponent<Image>().color = team.color;
    }
    public void setfield(int x,int y)
    {
        if(field!=null)
            field.unit = null;
        field = gamemanager.getField((x, y));
        field.unit = this;

        transform.position = new Vector3(x + 0.5f, 0, y + 0.5f);
        Rpcsetfield(x, y);
    }
    [ClientRpc]
    public void Rpcsetfield(int x,int y)
    {
        if (isServer)
            return;
        if(field!=null)
            field.unit = null;
        field = gamemanager.getField((x, y));
        field.unit = this;

        transform.position = new Vector3(x + 0.5f, 0, y + 0.5f);
    }
    public virtual void hoverField() { }
    public virtual void Select()
    {
        selectedunit?.Unselect();
        selectedunit = this;
        Ui.SetActive(true);
        CameraControll.Focus(this);
    }
    public virtual void Unselect()
    {
        selectedunit = null;
        Ui.SetActive(false);
    }
    [Command]
    public virtual void CmdMove(int x,int y)
    {
        Field f = gamemanager.getField((x, y));
        tryMove(f);
    }
    [Server]
    public virtual bool tryMove(Field f)
    {
        if (gamemanager.CurrentTurn != this)
            return false;
        if (f.unit == null && f.fieldType == FieldType.Ground && GameManager.Dist(f, field) == 1)
        {
            if (Initative <= MaxInitative - MoveCost)
            {
                setfield(f.coordinates.Item1, f.coordinates.Item2);
                IncreaseInitative(MoveCost);
                gamemanager.ShowCurrentInitative(Initative, MaxInitative);
                SetInitative();
                setVisionFields();
                return true;
            }
        }
        return false;
    }
    [Command]
    public virtual void CmdtryAttack(int x, int y)
    {
        tryAttack(gamemanager.getField((x, y)));
    }
    [Server]
    public virtual bool tryAttack(Field f)
    {
        if (gamemanager.CurrentTurn != this)
            return false;
        if (f.unit != null && f.unit.team != team && GameManager.Dist(f, field) <= 1)
        {
            if (Initative <= MaxInitative - AttackCost)
            {
                Attack(f.unit);
                IncreaseInitative(AttackCost);
                gamemanager.ShowCurrentInitative(Initative, MaxInitative);
                SetInitative();
                return true;
            }
        }
        return false;
    }
    [ClientRpc]
    public override void RpcGetTurn()
    {
        InitativeIcon.GetComponent<Image>().color = Color.green;
        if(hasVision)
            Select();
    }
    public virtual void Attack(Unit u)
    {
        DealDamage(u, Damage,DamageType.Physical);
    }
    public virtual void DealDamage(Unit u,float amount,DamageType damageType)
    {
        float m = 1;
        switch(damageType)
        {
            case DamageType.Physical:
                m = getDmgMult(u.Armor);
                break;
            case DamageType.Magic:
                m = getDmgMult(u.MagicResistance);
                break;
        }
        u.LoseHealth(this,(int)(amount * m));
    }
    public virtual void LoseHealth(Unit attacker,int amount)
    {
        CurrentHealth -= amount;
        Healthbar.value = 1f * CurrentHealth / MaxHealth;
        if (CurrentHealth <= 0)
            Die(attacker);
    }
    public virtual void Die(Unit attacker)
    {
        throw new System.NotImplementedException();
    }
    [ClientRpc]
    public virtual void RPCDie()
    {
        gamemanager.AllEntity.Remove(this);
        field.unit = null;
        Destroy(gameObject);
    }
    public virtual void LevelUp()
    {
        Level++;
        Damage += DamagePerLevel;
        MaxHealth += HealthPerLevel;
        CurrentHealth += HealthPerLevel;
        Armor += ArmorPerLevel;
        MagicResistance += MagicResistancePerLevel;
    }
    [ClientRpc]
    public override void RpcEndTurn()
    {
        InitativeIcon.GetComponent<Image>().color = team?.color??Color.white;
    }
    public virtual void Click(Field field) { }
    public static float getDmgMult(int r)
    {
        return 100 / (100f + r);
    }
    [ClientRpc]
    public override void UpdateVision()
    {
        Model.SetActive(hasVision);
        InitativeIcon.SetActive(hasVision);
    }
    public List<Champion> GetExpChampions(Unit attacker)
    {
        List<Champion> champions = new List<Champion>();

        foreach (Field f in gamemanager.allFields.Where(f => f.unit is Champion&& f.unit.team != team && GameManager.Dist(field,f)<=8))
            champions.Add(f.unit as Champion);

        if (!champions.Contains(attacker) && attacker is Champion)
            champions.Add(attacker as Champion);

        return champions;
    } 
}
public enum DamageType
{
    Physical,
    Magic
}
