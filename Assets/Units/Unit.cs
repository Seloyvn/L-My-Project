using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public abstract class Unit : Entity
{
    public static Unit selectedunit;
    public GameObject Ui;

    public Team team;

    #region stats
    [SyncVar]
    public int MaxHealth;
    [SyncVar]
    [HideInInspector]
    public int CurrentHealth;

    [SyncVar]
    public int Damage;

    [SyncVar]
    public int Armor;

    [SyncVar]
    public int MagicResistance;

    [SyncVar]
    public float MoveSpeed;
    public int MoveCost => (int)(100 / MoveSpeed)+1;
    [SyncVar]
    public float AttackSpeed;
    public int AttackCost => (int)(100 / AttackSpeed) + 1;
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
    

    [ClientRpc]
    public void setfield(int x,int y)
    {
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
    public virtual void tryMove(Field field)
    {
        CmdtryMove((int)field.coordinates.Item1, (int)field.coordinates.Item2);
    }
    [Command]
    public virtual void CmdtryMove(int x,int y)
    {
        if (gamemanager.CurrentTurn != this)
            return;
        Field f = gamemanager.getField((x, y));
        if (f.unit == null&&f.fieldType==FieldType.Ground&&GameManager.Dist(f,field) <= 1000)
        {
            if (Initative <= MaxInitative-MoveCost)
            {
                setfield(x, y);
                IncreaseInitative(MoveCost);
                gamemanager.ShowCurrentInitative(Initative, MaxInitative);
                SetInitative();
            }
        }
    }
    public virtual void tryAttack(Field field)
    {
        CmdtryAttack((int)field.coordinates.Item1, (int)field.coordinates.Item2);
    }
    [Command]
    public virtual void CmdtryAttack(int x, int y)
    {
        if (gamemanager.CurrentTurn != this)
            return;
        Field f = gamemanager.getField((x, y));
        if (f.unit != null && f.unit.team!=team && GameManager.Dist(f, field) <= 1)
        {
            if (Initative <= MaxInitative - AttackCost)
            {
                Attack(f.unit);
                IncreaseInitative(AttackCost);
                gamemanager.ShowCurrentInitative(Initative, MaxInitative);
                SetInitative();
            }
        }
    }
    [ClientRpc]
    public override void RpcGetTurn()
    {
        InitativeIcon.GetComponent<Image>().color = Color.green;
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
        u.LoseHealth((int)(amount * m));
    }
    public virtual void LoseHealth(int amount)
    {
        CurrentHealth -= amount;
    }
    public virtual void tryEndTurn()
    {
        CmdEndTurn();
    }
    [Command]
    public virtual void CmdEndTurn()
    {
        EndTurn();
    }
    [Server]
    public virtual void EndTurn()
    {
        gamemanager.EndTurn(this);
        RpcEndTurn();
    }
    [ClientRpc]
    public virtual void RpcEndTurn()
    {
        InitativeIcon.GetComponent<Image>().color = team.color;
    }
    public virtual void Click(Field field) { }
    public static float getDmgMult(int r)
    {
        return 10 / (10f + r);
    }
}
public enum DamageType
{
    Physical,
    Magic
}
