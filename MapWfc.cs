
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.FilePathAttribute;
using static UnityEditor.PlayerSettings;
using static UnityEditor.VersionControl.Asset;
using Random = UnityEngine.Random;



public class MapWfc : MonoBehaviour
{
    public GameObject library;
    public Vector2Int size, pos;
    public Texture2D map;
    public Color[] map_pixels;
    public Dictionary<Color32, GameObject> instance_slots = new Dictionary<Color32, GameObject>();
    public GameObject[] prefab_objects;
    public Color32[] pixels;

    private Vector3 move_pos, start_transform_pos;
    private float elapsed, elapsed_movement;
    private Vector3 lastPosition = Vector3.zero;

    public Tile_data[,] wfc_map;
    public List<Tile_data> worker_list = new List<Tile_data>();

    public Vector2Int actual_position;

    public enum TILEID
    {
        VACUUM = 0,
        FLOOR = 1,
        WALL = 2,
    }
    public class TileInfo
    {
        public TILEID[,] tileid;
        public int index;

        public TileInfo(TILEID[,] _tileID, int _index)
        {
            this.tileid = _tileID;
            this.index = _index;
        }
    }

    public TILEID[,] CORNER_NE = new TILEID[,]  {
        { TILEID.WALL, TILEID.WALL },
        { TILEID.FLOOR, TILEID.WALL } };

    public TILEID[,] CORNER_NW = new TILEID[,]    {
        { TILEID.WALL, TILEID.WALL },
        { TILEID.WALL, TILEID.FLOOR } };

    public TILEID[,] CORNER_SE = new TILEID[,]    {
        { TILEID.FLOOR, TILEID.WALL },
        { TILEID.WALL, TILEID.WALL } };

    public TILEID[,] CORNER_SW = new TILEID[,]    {
        { TILEID.WALL, TILEID.FLOOR },
        { TILEID.WALL, TILEID.WALL } };

    public TILEID[,] WALL_NORTH = new TILEID[,]    {
        { TILEID.WALL, TILEID.WALL },
        { TILEID.FLOOR, TILEID.FLOOR } };

    public TILEID[,] WALL_EAST = new TILEID[,]    {
        { TILEID.FLOOR, TILEID.WALL },
        { TILEID.FLOOR, TILEID.WALL } };

    public TILEID[,] WALL_SOUTH = new TILEID[,]    {
        { TILEID.FLOOR, TILEID.FLOOR },
        { TILEID.WALL, TILEID.WALL } };

    public TILEID[,] WALL_WEST = new TILEID[,]    {
        { TILEID.WALL, TILEID.FLOOR },
        { TILEID.WALL, TILEID.FLOOR } };

    public TILEID[,] EDGE_NE = new TILEID[,]    {
        { TILEID.FLOOR, TILEID.WALL },
        { TILEID.FLOOR, TILEID.FLOOR } };

    public TILEID[,] EDGE_NW = new TILEID[,]    {
        { TILEID.WALL, TILEID.FLOOR },
        { TILEID.FLOOR, TILEID.FLOOR } };

    public TILEID[,] EDGE_SE = new TILEID[,]    {
        { TILEID.FLOOR, TILEID.FLOOR },
        { TILEID.FLOOR, TILEID.WALL } };

    public TILEID[,] EDGE_SW = new TILEID[,]    {
        { TILEID.FLOOR, TILEID.FLOOR },
        { TILEID.WALL, TILEID.FLOOR } };

    public TILEID[,] BLANK = new TILEID[,]    {
        { TILEID.FLOOR, TILEID.FLOOR },
        { TILEID.FLOOR, TILEID.FLOOR } };

    public List<TILEID[,]> tileList = new List<TILEID[,]>();


    public class Tile_data
    {
        public Tile_data(Vector2Int _position)
        {
            this.position = _position;
        }

        public Vector2Int position = new(0, 0);
        public bool collapsed = false;
        public TILEID[,] tileData;
        public int tileIndex;
        public List<TileInfo> posibilities = new List<TileInfo>();
        public int entropy = 0;
        public int distance_from_center = 0;
    }


    void Start()
    {
        tileList.AddRange(new List<TILEID[,]>
        {
            BLANK, WALL_NORTH,WALL_EAST,WALL_SOUTH,WALL_WEST,
            CORNER_NE,CORNER_SE, CORNER_SW, CORNER_NW,
            EDGE_NE, EDGE_SE,EDGE_SW, EDGE_NW,
        });
        reset_map();
    }

    public void reset_map()
    {
        wfc_map = new Tile_data[size.x, size.y];
        worker_list.Clear();
       
        actual_position = size / 2;

        Tile_data begin = new Tile_data(actual_position);
        wfc_map[actual_position.x, actual_position.y] = begin;
        worker_list.Add(begin);

    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed - elapsed_movement > 2)
        {
            elapsed_movement = elapsed;
            foreach (Transform child in transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            reset_map();
            
        }
        generate();
        //  if (Input.GetKeyDown(KeyCode.Space))


    }

    // Update is called once per frame
    void generate()
    {
        float x_scale = 1f / size.x;
        float y_scale = 1f / size.y;

        int handled = 0;
        Debug.Log(worker_list.Count());
        foreach (Tile_data t in worker_list)
        {
            Tile_data inspect_worker = t;
            Vector2Int ap = inspect_worker.position;
            var placement_rule = get_placement_rule(inspect_worker);
            inspect_worker = refresh_tile(inspect_worker, placement_rule);
        }

        for (int i = worker_list.Count - 1; i >= 0; i--)
        {
            if (worker_list[i].collapsed)
                worker_list.RemoveAt(i);
        }

        var sorted_worker_list = worker_list.OrderByDescending(t => t.entropy).ThenBy(t => t.distance_from_center);
        Stack<Tile_data> worker = new Stack<Tile_data>();

        foreach (Tile_data t in sorted_worker_list)
            worker.Push(t);

        while (worker.Count > 0)
        {
            Tile_data inspect_worker = worker.Pop();
            Vector2Int ap = inspect_worker.position;
            TileInfo picked_tile = inspect_worker.posibilities[Random.Range(0, inspect_worker.posibilities.Count)];
            inspect_worker.tileData = picked_tile.tileid;
            inspect_worker.tileIndex = picked_tile.index;

            Spawn_block(getOption(inspect_worker),
                transform,
                new Vector2(ap.x * 4,
                ap.y * 4),
                new Vector2(x_scale / 4, y_scale / 4), inspect_worker.tileIndex + "_" + inspect_worker.position);

            inspect_worker.collapsed = true;
            hire_new_workers(inspect_worker);

        }
    }

    TILEID[,] get_placement_rule(Tile_data ap)
    {


        TILEID[,] tilecheck = new TILEID[2, 2] {
            { TILEID.VACUUM, TILEID.VACUUM },
            { TILEID.VACUUM, TILEID.VACUUM } };


        Tile_data cP;

        if (ap.position.y - 1 > -1)
        {
            cP = wfc_map[ap.position.x, ap.position.y - 1];
            if (cP != null)
            {
                if (cP.tileData != null)
                {
                    tilecheck[0, 0] = cP.tileData[0, 1];
                    tilecheck[1, 0] = cP.tileData[1, 1];
                }
            }
        }

        if (ap.position.y + 1 < size.y)
        {
            cP = wfc_map[ap.position.x, ap.position.y + 1];
            if (cP != null)
            {
                if (cP.tileData != null)
                {
                    tilecheck[0, 1] = cP.tileData[0, 0];
                    tilecheck[1, 1] = cP.tileData[1, 0];
                }
            }
        }

        if (ap.position.x - 1 > -1)
        {
            cP = wfc_map[ap.position.x - 1, ap.position.y];
            if (cP != null)
            {
                if (cP.tileData != null)
                {
                    tilecheck[1, 0] = cP.tileData[0, 0];
                    tilecheck[1, 1] = cP.tileData[0, 1];
                }
            }
        }

        if (ap.position.x + 1 < size.x)
        {
            cP = wfc_map[ap.position.x + 1, ap.position.y];
            if (cP != null)
            {
                if (cP.tileData != null)
                {
                    tilecheck[0, 0] = cP.tileData[1, 0];
                    tilecheck[0, 1] = cP.tileData[1, 1];
                }
            }
        }
        return tilecheck;
    }

    public Tile_data refresh_tile(Tile_data tileData, TILEID[,] rule)
    {
        tileData.distance_from_center = Math.Abs(
            (tileData.position.x * tileData.position.y) -
            (int)Mathf.Pow(size.x / 2, 2));

        tileData.posibilities = new List<TileInfo>();

        int index = 0;
        foreach (TILEID[,] t in tileList)
        {
            if (t[0, 0] == rule[0, 0] || rule[0, 0] == TILEID.VACUUM)
                if (t[1, 0] == rule[1, 0] || rule[1, 0] == TILEID.VACUUM)
                    if (t[0, 1] == rule[0, 1] || rule[0, 1] == TILEID.VACUUM)
                        if (t[1, 1] == rule[1, 1] || rule[1, 1] == TILEID.VACUUM)
                            tileData.posibilities.Add(new TileInfo(t, index));
            index++;
        }
        tileData.entropy = tileData.posibilities.Count;

        return tileData;
    }

    void hire_new_workers(Tile_data ap)
    {
        create_worker(ap.position + new Vector2Int(-1, 0));
        create_worker(ap.position + new Vector2Int(1, 0));
        create_worker(ap.position + new Vector2Int(0, -1));
        create_worker(ap.position + new Vector2Int(0, 1));
    }

    void create_worker(Vector2Int pos)
    {
        Tile_data td = new Tile_data(pos);
        if (pos.x > -1 && pos.y > -1 && pos.x < size.x && pos.y < size.y &&
            wfc_map[pos.x, pos.y] == null)
        {
            wfc_map[pos.x, pos.y] = td;
            worker_list.Add(td);
        }
    }

    public GameObject getOption(Tile_data option)
    {
        int instance = 0; float rot = 0;

        switch (option.tileIndex)
        {
            case 0:
                instance = 0; break;
            case 1:
                instance = 1; rot = 180; break;
            case 2:
                instance = 1; rot = 90; break;
            case 3:
                instance = 1; rot = 0; break;
            case 4:
                instance = 1; rot = 270; break;
            case 5:
                instance = 2; rot = 180; break;
            case 6:
                instance = 2; rot = 90; break;
            case 7:
                instance = 2; rot = 0; break;
            case 8:
                instance = 2; rot = 270; break;
            case 9:
                instance = 3; rot = 180; break;
            case 10:
                instance = 3; rot = 90; break;
            case 11:
                instance = 3; rot = 0; break;
            case 12:
                instance = 3; rot = 270; break;
        }

        GameObject go = prefab_objects[instance];
        go.transform.eulerAngles = new(0, rot, 0);
        return go;
    }

    public GameObject Spawn_block(GameObject go, Transform transform, Vector2 position, Vector2 scale, string _name, bool originalName = false)
    {
        float x = position.x; float y = position.y;
        float x_scale = scale.x; float y_scale = scale.y;

        if (go != null)
        {
            var block = Instantiate(go);

            //if (block.GetComponent<BoxCollider>() == null)
            //   block.AddComponent<BoxCollider>();

            block.tag = "Generated";
            float min_scale = (x_scale < y_scale) ? x_scale : y_scale;
            block.transform.parent = transform;

            block.transform.localPosition = new(x * x_scale, 0f, y * y_scale);

            if (!originalName)
                block.name = "B" + min_scale.ToString();
            block.transform.localScale = new(x_scale, min_scale, y_scale);
            block.name = _name;
            return block;
        }
        else { return null; }
    }
}
