using Mirror;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    public Field[,] fields;
    public List<Field> allFields = new List<Field>();
    public GameObject[] AllChampionPrefabs;
    [HideInInspector]
    public Entity CurrentTurn;

    public Team team1 = new Team(0);
    public Team team2 = new Team(1);

    public Slider CurrentInitativeSlider;
    public GameObject InitativeBar;

    public GameObject HoverObjectPrefab;
    public GameObject HoverObject;

    public Material canMoveMaterial;
    public Material notMoveMaterial;

    public List<Entity> AllEntity = new List<Entity>();

    public Terrain terrain;

    public int[] SelectedChampions = new int[10];
    public GameObject PreGame;
    private void Start()
    {
        instance = this;
        setField();        
    }
    void setField()
    {
        setField(MapBuilder.Load(Application.persistentDataPath + "/map.map"));
    }
    void setField(MapSerializable map)
    {
        fields = new Field[map.size, map.size];
        for (int i = 0; i < map.size; i++)
            for (int j = 0; j < map.size; j++)
                fields[i, j] = new Field(i, j, map.heights[i, j] <= 0 ? FieldType.Ground : FieldType.Wall);


        foreach (Field f in fields)
            allFields.Add(f);

        terrain.terrainData.size = new Vector3(map.size, 1, map.size);

        int res = terrain.terrainData.heightmapResolution - 1;
        float[,] heights = new float[res, res];
        for (int i = 0; i < res; i++)
            for (int j = 0; j < res; j++)
                heights[i, j] = map.heights[(int)(1f * map.size / res * j), (int)(1f * map.size / res * i)];
        terrain.terrainData.SetHeights(0, 0, heights);

        terrain.terrainData.SetHeights(0, 0, heights);
    }
    [Command(requiresAuthority =false)]
    public void CmdSelectChampion(int id,int c)
    {
        SelectedChampions[c] = id;
    }
    public void StartGame()
    {
        if (!isServer)
            return;
        RpcHidePreGame();
        for (int i = 0; i < 10; i++)
        {
            SpawnChampion(SelectedChampions[i],
                getCloseField(getField((i < 5 ? 4 : 123, i < 5 ? 4 : 123)),
                getField((i < 5 ? 4 : 123, i < 5 ? 4 : 123)),2).coordinates,
                NetworkServer.connections[i < 5 ? 0 : NetworkServer.connections.Last().Key].identity.GetComponent<Player>(), i < 5 ? 0 : 1);
        }

        setCurrentTurn(nextunit);
    }
    [ClientRpc]
    public void RpcHidePreGame()
    {
        PreGame.SetActive(false);
    }
    Unit nextunit;
    [Server]
    public void SpawnChampion(int id,(int,int) cord,Player player,int team)
    {
        GameObject g = Instantiate(AllChampionPrefabs[id]);
        NetworkServer.Spawn(g, player.gameObject);
        Unit unit = g.GetComponent<Unit>();
        unit.Initative = 0;
        unit.SetInitative();

        unit.setTeam(team);


        getField(cord).unit=unit;

        unit.setfield(cord.Item1, cord.Item2);

        nextunit = unit;
    }
    public void setCurrentTurn(Entity e)
    {
        e.IncreaseInitative(e.StartDelay);
        e.SetInitative();
        RpcsetCurrentTurn(e.netId);
        e.GetTurn();
    }
    [ClientRpc]
    public void RpcsetCurrentTurn(uint id)
    {
        CurrentTurn = AllEntity.Where(u => u.netId == id).FirstOrDefault();
    }
    public void SetInitativeBar(GameObject o,int i)
    {
        o.transform.SetParent(InitativeBar.transform);
        o.transform.localPosition = new Vector3(i*5-300,0,0);
    }
    [ClientRpc]
    public void ShowCurrentInitative(int c,int m)
    {
        CurrentInitativeSlider.maxValue = m;
        CurrentInitativeSlider.value = c;
    }
    public void EndTurn(Unit u)
    {
        if (CurrentTurn != u)
            return;
        
        Entity nextentity = AllEntity.OrderBy(u => u.Initative).First();
        int i = nextentity.Initative;
        foreach (Unit unit in AllEntity)        
            unit.DecreaseInitative(i);

        setCurrentTurn(nextentity);

    }
    public void Hover(Field f,HoverMode mode)
    {
        if (mode == HoverMode.None||f==null)
        {
            HoverObject.SetActive(false);
            return;
        }
        HoverObject.SetActive(true);
        HoverObject.transform.position = new Vector3(f.coordinates.Item1+0.5f, 0, f.coordinates.Item2 + 0.5f);
        HoverObject.GetComponent<MeshRenderer>().material = getMaterial(mode);
    }
    [ClientRpc]
    public void StartHighlight(int x,int y)
    {
        fields[x, y].StartHighlight();
    }
    [ClientRpc]
    public void StopHighlight(int x, int y)
    {
        fields[x, y].StopHighlight();
    }
    public Material getMaterial(HoverMode mode)
    {
        switch (mode)
        {
            case HoverMode.CanMove:
                return canMoveMaterial;
            case HoverMode.NotMove:
                return notMoveMaterial;
        }
        return null;
    }
    public Field getCurrentField()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100.0f, 8))
        {
            return getField(hit.point);
        }
        return null;
    }
    public Field getField(Vector3 c)
    {
        return getField(getfieldcords(c));
    }
    public Field getField((int,int) c)
    {
        return fields[c.Item1,c.Item2];
    }
    public Field getCloseField(Field target,Field start,int maxd=1)
    {
        int c = 0;

        while (c <= maxd)
        {
            foreach (Field f in allFields.Where(f=>(f.fieldType==FieldType.Ground&&f.unit==null)).
                OrderBy(fl => Dist(fl,start)))
            {
                if (Dist(f,target) == c)
                    return f;
            }
            c++;
        }
        return null;

    }
    public static int Dist(Field f1,Field f2)
    {
        return Mathf.Abs(f1.coordinates.Item1 - f2.coordinates.Item1) + Mathf.Abs(f1.coordinates.Item2 - f2.coordinates.Item2);
    }
    public static (int,int) getfieldcords(Vector3 position)
    {
        int x = (int)Mathf.Round(position.x - 0.5f);
        int y = (int)Mathf.Round(position.z - 0.5f);
        return (x, y);
    }
}
public enum HoverMode
{
    None,
    CanMove,
    NotMove
}
