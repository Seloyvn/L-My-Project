using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : NetworkBehaviour
{
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
}
