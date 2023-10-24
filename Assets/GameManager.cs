using Mirror;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System;
using System.Collections;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour
{
    public bool VisionEnabled;
    public bool AITurns;
    public static GameManager instance;
    public Field[,] fields;
    public List<Field> allFields = new List<Field>();

    public int TimeTotal;

    public GameObject[] AllChampionPrefabs;
    public GameObject MinionPrefab;
    public GameObject BasePrefab;
    public GameObject MinionSpawnPrefab;

    [HideInInspector]
    public Entity CurrentTurn;

    public Team team1 = new Team(0);
    public Team team2 = new Team(1);

    public Slider CurrentInitativeSlider;
    public GameObject InitativeBar;

    public GameObject HoverObjectPrefab;
    public GameObject FogObjectPrefab;
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
        SetupGame();

        StartCoroutine(start());
    }
    void SetupGame()
    {
        NetworkServer.connections[0].identity.GetComponent<Player>().setTeam(0);
        NetworkServer.connections[NetworkServer.connections.Last().Key].identity.GetComponent<Player>().setTeam(1);


        SpawnBase();

        for (int i = 0; i < 10; i++)
        {
            SpawnChampion(SelectedChampions[i],
                getCloseField(getField((i < 5 ? 4 : 123, i < 5 ? 4 : 123)),
                getField((i < 5 ? 4 : 123, i < 5 ? 4 : 123)), 2).coordinates,
                i < 5 ? 0 : 1);
        }


        NetworkServer.Spawn(Instantiate(MinionSpawnPrefab), NetworkServer.connections[0]);

        foreach (Entity e in AllEntity)
            e.setVisionFields();
    }
    IEnumerator start()
    {
        yield return new WaitForSeconds(1);
        EndTurn(null);
    }
    [ClientRpc]
    public void RpcHidePreGame()
    {
        PreGame.SetActive(false);
    }
    [Server]
    void SpawnChampion(int id,(int,int) cord,int team)
    {
        GameObject g = Instantiate(AllChampionPrefabs[id]);
        NetworkServer.Spawn(g, (team == 0 ? team1 : team2).player.gameObject);
        Unit unit = g.GetComponent<Unit>();
        unit.Initative = Random.Range(0, 5);
        unit.SetInitative();

        unit.setTeam(team);


        getField(cord).unit=unit;

        unit.setfield(cord.Item1, cord.Item2);

    }
    void SpawnBase()
    {
        Base b = Instantiate(BasePrefab).GetComponent<Base>();
        NetworkServer.Spawn(b.gameObject);
        b.team = team1;
        b.field = fields[0, 0];


        b = Instantiate(BasePrefab).GetComponent<Base>();
        NetworkServer.Spawn(b.gameObject);
        b.team = team2;
        b.field = fields[127, 127];
    }
    public void SpawnMinions()
    {
        int c = avgChampLvl();
        
        SpawnMinion(0,new (int, int)[3] {(3,10),(7,119),(117,124)},c);
        SpawnMinion(0,new (int, int)[3] {(3,9),(7,119),(117,124)},c);
        SpawnMinion(0,new (int, int)[3] {(3,8),(7,119),(117,124)},c);
        SpawnMinion(0,new (int, int)[3] { (10, 3),(119,7), (124, 117)},c);
        SpawnMinion(0,new (int, int)[3] { (9, 3),(119,7), (124, 117)},c);
        SpawnMinion(0,new (int, int)[3] { (8, 3),(119,7), (124, 117)},c);
        SpawnMinion(0,new (int, int)[4] { (10,10),(55,70), (72, 57),(117,117)},c);
        SpawnMinion(0,new (int, int)[4] { (9,10),(55,70), (72, 57),(117,117)},c);
        SpawnMinion(0,new (int, int)[4] { (10,9),(55,70), (72, 57),(117,117)},c);
        SpawnMinion(1,new (int, int)[3] { (117, 124), (7,119),(3, 10) },c);
        SpawnMinion(1,new (int, int)[3] { (118, 124), (7,119),(3, 10) },c);
        SpawnMinion(1,new (int, int)[3] { (119, 124), (7,119),(3, 10) },c);
        SpawnMinion(1,new (int, int)[3] { (124, 117), (119,7), (10, 3)},c);
        SpawnMinion(1,new (int, int)[3] { (124, 118), (119,7), (10, 3)},c);
        SpawnMinion(1,new (int, int)[3] { (124, 119), (119,7), (10, 3)},c);
        SpawnMinion(1,new (int, int)[4] { (117, 117),  (72, 57), (55, 70), (10, 10)},c);
        SpawnMinion(1,new (int, int)[4] { (118, 117),  (72, 57), (55, 70), (10, 10)},c);
        SpawnMinion(1,new (int, int)[4] { (117, 118),  (72, 57), (55, 70), (10, 10)},c);
    }
    void SpawnMinion(int team,(int,int)[] t,int l)
    {
        GameObject g = Instantiate(MinionPrefab);
        NetworkServer.Spawn(g);
        Minion minion = g.GetComponent<Minion>();
        minion.SetLevel(l);
        minion.Initative = Random.Range(0,5);
        minion.SetInitative();

        minion.setTeam(team);

        minion.setfield(t[0].Item1, t[0].Item2);

        minion.setPath(t);

    }
    public void setCurrentTurn(Entity e)
    {
        e.IncreaseInitative(e.StartDelay);
        e.SetInitative();
        CurrentTurn = e;
        RpcsetCurrentTurn(e.netId);
        e.GetTurn();
    }
    [ClientRpc]
    public void RpcsetCurrentTurn(uint id)
    {
        if (isServer)
            return;
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
    public void EndTurn(Entity u)
    {
        if (CurrentTurn != u)
            return;
        Entity nextentity = AllEntity.OrderBy(u => u.Initative).First();
        int i = nextentity.Initative;
        TimeTotal += i;
        foreach (Entity e in AllEntity)        
            e.DecreaseInitative(i);

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
    public void UpdateVision()
    {
        foreach (Entity e in AllEntity)
            e.UpdateVision();
    }
    public static int Dist(Field f1,Field f2)
    {
        return Mathf.Abs(f1.coordinates.Item1 - f2.coordinates.Item1) + Mathf.Abs(f1.coordinates.Item2 - f2.coordinates.Item2);
    }
    public static float SDist(Field f1,Field f2)
    {
        return Mathf.Pow(Mathf.Abs(f1.coordinates.Item1 - f2.coordinates.Item1),2) + Mathf.Pow(Mathf.Abs(f1.coordinates.Item2 - f2.coordinates.Item2),2);
    }
    public List<Field> BetweenFields(Field f1, Field f2)
    {
        int x1 = Mathf.Min(f1.coordinates.Item1, f2.coordinates.Item1);
        int x2 = Mathf.Max(f1.coordinates.Item1, f2.coordinates.Item1);
        int y1 = Mathf.Min(f1.coordinates.Item2, f2.coordinates.Item2);
        int y2 = Mathf.Max(f1.coordinates.Item2, f2.coordinates.Item2);
        List<Field> fields = new List<Field>();
        IEnumerable<Field> ef= allFields.Where(f => f.coordinates.Item1 >= x1 && f.coordinates.Item1 <= x2 && f.coordinates.Item2 >= y1 && f.coordinates.Item2 <= y2);

        foreach (Field f in ef)
        {
            float e1 = f.coordinates.Item2 - ((((float)f2.coordinates.Item2 - f1.coordinates.Item2) / (f2.coordinates.Item1 - f1.coordinates.Item1)) * (f.coordinates.Item1 - f1.coordinates.Item1-0.5f) + f1.coordinates.Item2 + 0.5f);
            float e2 = f.coordinates.Item2 - ((((float)f2.coordinates.Item2 - f1.coordinates.Item2) / (f2.coordinates.Item1 - f1.coordinates.Item1)) * (f.coordinates.Item1+1 - f1.coordinates.Item1 - 0.5f) + f1.coordinates.Item2 + 0.5f);
            float e3 = f.coordinates.Item2+1 - ((((float)f2.coordinates.Item2 - f1.coordinates.Item2) / (f2.coordinates.Item1 - f1.coordinates.Item1)) * (f.coordinates.Item1 - f1.coordinates.Item1 - 0.5f) + f1.coordinates.Item2 + 0.5f);
            float e4 = f.coordinates.Item2+1 - ((((float)f2.coordinates.Item2 - f1.coordinates.Item2) / (f2.coordinates.Item1 - f1.coordinates.Item1)) * (f.coordinates.Item1+1 - f1.coordinates.Item1 - 0.5f) + f1.coordinates.Item2 + 0.5f);

            if (!((e1 < 0 && e2 < 0 && e3 < 0 && e4 < 0) || (e1 > 0 && e2 > 0 && e3 > 0 && e4 > 0)))
                fields.Add(f);            
        }
        return fields;

    }
    int avgChampLvl() {
        int c = 0;
        int t = 0;
        foreach (Entity u in AllEntity)
        {
            if (u is Champion ch)
            {
                t += ch.Level;
                c++;
            }
        }
        if (c == 0)
            return 1;
        else
            return t / c;
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
