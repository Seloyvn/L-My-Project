using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Entity : NetworkBehaviour
{
    public Field field;

    [HideInInspector]
    [SyncVar]
    public int Initative;
    public int MaxInitative;
    public int StartDelay;
    public GameObject InitativeIcon;
    GameManager gamemanager => GameManager.instance;
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
    [ClientRpc]
    public void SetInitative()
    {
        gamemanager.SetInitativeBar(InitativeIcon, Initative);
    }
}
