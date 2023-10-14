using System;
using UnityEngine;

public class Field
{
    public (int,int) coordinates;
    public FieldType fieldType;
    public Unit unit;

    GameObject hover;
    public Field(int x,int y, FieldType f)
    {
        coordinates = (x,y);
        fieldType = f;
    }
    public void StartHighlight()
    {
        GameObject.Destroy(hover);
        hover = GameObject.Instantiate(GameManager.instance.HoverObjectPrefab);
        hover.transform.position = new Vector3(coordinates.Item1+0.5f, 0, coordinates.Item2+0.5f);
    }
    public void StopHighlight()
    {
        GameObject.Destroy(hover);
    }
    public override string ToString()
    {
        return base.ToString() + coordinates;
    }
}
public enum FieldType
{
    Ground,
    Wall
}