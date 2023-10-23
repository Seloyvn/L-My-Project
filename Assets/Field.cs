using System;
using UnityEngine;

public class Field
{
    public (int,int) coordinates;
    public FieldType fieldType;
    public Unit unit;

    public bool hasVision;
    public SerializeableField serializeField => new SerializeableField(this);

    GameObject hover;
    GameObject fog;
    public Field(int x,int y, FieldType f)
    {
        coordinates = (x,y);
        fieldType = f;

        hover = GameObject.Instantiate(GameManager.instance.HoverObjectPrefab);
        hover.transform.position = new Vector3(coordinates.Item1 + 0.5f, 0, coordinates.Item2 + 0.5f);
        hover.SetActive(false);

        fog = GameObject.Instantiate(GameManager.instance.FogObjectPrefab);
        fog.transform.position = new Vector3(coordinates.Item1 + 0.5f, 0, coordinates.Item2 + 0.5f);
        fog.SetActive(false);
    }
    public void StartHighlight()
    {
        hover.SetActive(true);
    }
    public void StopHighlight()
    {
        hover.SetActive(false);
    }
    public override string ToString()
    {
        return base.ToString() + coordinates;
    }
    public void SetVision(bool b)
    {
        hasVision = b;
        fog.SetActive((!b)&&GameManager.instance.VisionEnabled&&fieldType!=FieldType.Wall);
    }
}
public enum FieldType
{
    Ground,
    Wall
}
[Serializable]
public class SerializeableField
{
    public int x;
    public int y;
    public SerializeableField() { }
    public SerializeableField(Field f) 
    {
        x = f.coordinates.Item1;
        y = f.coordinates.Item2;
    }
    public Field field => GameManager.instance.getField((x,y));
}