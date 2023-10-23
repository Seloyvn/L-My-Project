using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public abstract class Entity : NetworkBehaviour
{
    public Field field;

    public Team team;
    [HideInInspector]
    [SyncVar]
    public int Initative;
    public int MaxInitative;
    public int StartDelay;
    public GameObject InitativeIcon;
    public List<Field> visionfields = new List<Field>();
    public virtual bool hasVision
    {
         get
        {
            return team.player.isLocalPlayer||field.hasVision||!gamemanager.VisionEnabled;
        }
    }
    GameManager gamemanager => GameManager.instance; 
    private void Start()
    {
        gamemanager.AllEntity.Add(this);
    }
    public virtual void GetTurn()
    {
        RpcGetTurn();
        gamemanager.ShowCurrentInitative(Initative, MaxInitative);
    }
    [ClientRpc]
    public virtual void RpcGetTurn()
    {
    }
    public virtual void IncreaseInitative(int amount)
    {
        Initative += amount;
        SetInitative();
    }
    public virtual void DecreaseInitative(int amount)
    {
        Initative -= amount;
        SetInitative();
    }

    public virtual void setVisionFields()
    {
        visionfields.Clear();

        visionfields.AddRange(gamemanager.allFields.Where(f=>GameManager.Dist(f,field)<=8&&f.fieldType!=FieldType.Wall&&
        !gamemanager.BetweenFields(field,f).Where(bf=>bf.fieldType!=FieldType.Ground).Any()));


        team.setVision();
    }
    public virtual IEnumerable<Field> getVisionFields()
    {
        return visionfields;
    }
    [ClientRpc]
    public void SetInitative()
    {
        gamemanager.SetInitativeBar(InitativeIcon, Initative);
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
    }
    [ClientRpc]
    public virtual void UpdateVision()
    {

    }
}
