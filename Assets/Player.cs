using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : NetworkBehaviour
{
    GameManager gamemanager=>GameManager.instance;
    void Update()
    {
        if (!isLocalPlayer)
            return;

        Unit.selectedunit?.hoverField();
        if (Input.GetMouseButtonDown(0))        
            ClickField(GameManager.instance.getCurrentField());
        if (Input.GetMouseButtonDown(1))
            Unit.selectedunit?.Unselect();
    }
    void ClickField(Field field)
    {
        if(EventSystem.current.IsPointerOverGameObject())
            return;
        if (field == null)
            return;
        if (Unit.selectedunit == null || !Unit.selectedunit.isOwned)
            field.unit?.Select();
        else
            Unit.selectedunit.Click(field);
    }
    public void setTeam(int i)
    {
        if (i == 0)
            gamemanager.team1.player = this;
        if (i == 1)
            gamemanager.team2.player = this;
        RpcsetTeam(i);
    }
    [ClientRpc]
    public void RpcsetTeam(int i)
    {
        if (isServer)
            return;
        if (i == 0)
            gamemanager.team1.player = this;
        if (i == 1)
            gamemanager.team2.player = this;
    }
    [TargetRpc]
    public void SetVision(SerializeableField[] sfield)
    {
        List<Field> fields = new List<Field>();
        foreach (SerializeableField f in sfield)
            fields.Add(f.field);

        foreach (Field f in gamemanager.allFields)
            f.SetVision(fields.Contains(f));
        
    }
}
