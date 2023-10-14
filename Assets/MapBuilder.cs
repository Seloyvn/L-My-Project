using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapBuilder : MonoBehaviour
{
    public MapSerializable Map;
    public int size => Map.size;
    public int setheight;
    public Terrain terrain;
    private void Start()
    {
        Debug.Log(Application.persistentDataPath + "/map.map");
        Load();
    }
    void Update()
    {
        if (Input.GetMouseButton(0))
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                (int,int) c=getCurrentField();
                if (Map.heights[c.Item1, c.Item2] != setheight)
                    SetField(c.Item1, c.Item2, setheight);
            }

    }
    public void SetField(int x,int y,int h)
    {
        Map.heights[x, y] = h;
        SetFieldHeight();
    }
    public void SetFieldHeight()
    {
        int res = terrain.terrainData.heightmapResolution-1;
        float[,] heights = new float[res, res];
        for (int i = 0; i < res; i++)
            for (int j = 0; j < res; j++)
                heights[i, j] = Map.heights[(int)(1f* size / res * j), (int)(1f * size / res * i)];
        terrain.terrainData.SetHeights(0, 0,heights);
    }
    public void sh(int i)
    {
        setheight = i;
    }
    public void Load()
    {
        Map=Load(Application.persistentDataPath + "/map.map");
        terrain.terrainData.size = new Vector3(size, 3, size);
        SetFieldHeight();
    }
    public static MapSerializable Load(string path)
    {
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            MapSerializable map = formatter.Deserialize(stream) as MapSerializable;

            return map;
        }
        else return null;
    }
    public void Save()
    {
        String path = Application.persistentDataPath + "/map.map";

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create);
        formatter.Serialize(stream, Map);
        stream.Close();
    }

    public (int,int) getCurrentField()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100.0f, 8))
        {

            int x = (int)Mathf.Round(hit.point.x - 0.5f);
            int y = (int)Mathf.Round(hit.point.z - 0.5f);
            return (x, y);
        }
        return (0,0);
    }
}
