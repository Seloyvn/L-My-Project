using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team
{
    public Color color;
    public (int, int) spawnPoint;
    public Player player;

    GameManager gamemanager => GameManager.instance;
    public Team(int id)
    {
        color = id == 1 ? Color.blue : Color.red;
    }
    public void setVision()
    {
        List<Field> Vision= new List<Field>();

        foreach(Entity e in gamemanager.AllEntity)
        {
            if (e.team == this)
            {
                IEnumerable<Field> ef= e.getVisionFields();
                foreach(Field f in ef)
                {
                    if(!Vision.Contains(f))
                        Vision.Add(f);
                }
            }
        }

        List<SerializeableField> serializeFields = new List<SerializeableField>();

        foreach (Field f in Vision)
            serializeFields.Add(f.serializeField);

        player.SetVision(serializeFields.ToArray());

        gamemanager.UpdateVision();
    }
}
