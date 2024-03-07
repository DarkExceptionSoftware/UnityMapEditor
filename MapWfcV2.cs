
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace PMA1980.MapGenerator
{

    public class MapWfcV2 : MonoBehaviour
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

        private GameObject player;



        #region Tile definitions

        TileInfo[] TileLibrary = new TileInfo[]
        {
            new TileInfo(
            "Floor",
            TILEID.FLOOR,
            new TILEID[] {TILEID.FLOOR, TILEID.FLOOR, TILEID.FLOOR, TILEID.FLOOR},
            0, DIRECTION.NORTH) ,

            new TileInfo(
            "Stone",
            TILEID.FLOOR,

            new TILEID[]
            {TILEID.FLOOR,TILEID.FLOOR, TILEID.FLOOR,TILEID.FLOOR},
            1, DIRECTION.NORTH) ,

            new TileInfo(
            "pillar",
            TILEID.FLOOR,
            new TILEID[]
            {TILEID.FLOOR,TILEID.FLOOR, TILEID.FLOOR,TILEID.FLOOR },
            2, DIRECTION.NORTH) ,

            new TileInfo(
            "wall_north",
            TILEID.WALL,
            new TILEID[]
            {TILEID.WALL,TILEID.FLOOR, TILEID.FLOOR,TILEID.FLOOR},
            3, DIRECTION.WEST) ,

            new TileInfo(
            "wall_east",
            TILEID.WALL,
            new TILEID[]
            {TILEID.FLOOR,TILEID.WALL, TILEID.FLOOR,TILEID.FLOOR },
            3, DIRECTION.NORTH) ,

            new TileInfo(
            "wall_south",
            TILEID.WALL,
            new TILEID[]
            {TILEID.FLOOR,TILEID.FLOOR, TILEID.WALL,TILEID.FLOOR },
            3, DIRECTION.EAST) ,

            new TileInfo(
            "wall_west",
            TILEID.WALL,
            new TILEID[]
            {TILEID.FLOOR,TILEID.FLOOR, TILEID.FLOOR,TILEID.WALL },
            3, DIRECTION.SOUTH) ,

            new TileInfo(
            "wall_north_east",
            TILEID.WALL,
            new TILEID[]
            {TILEID.WALL, TILEID.WALL ,TILEID.FLOOR,TILEID.FLOOR},
            4, DIRECTION.EAST) ,

            new TileInfo(
            "wall_north_west",
            TILEID.WALL,
            new TILEID[]
            {TILEID.WALL,TILEID.FLOOR,TILEID.FLOOR, TILEID.WALL },
            4, DIRECTION.NORTH) ,

            new TileInfo(
            "wall_south_east",
            TILEID.WALL,
            new TILEID[]
            {TILEID.FLOOR,TILEID.WALL, TILEID.WALL,TILEID.FLOOR },
            4, DIRECTION.SOUTH),

            new TileInfo(
            "wall_south_west",
            TILEID.WALL,
            new TILEID[]
            {TILEID.FLOOR,TILEID.FLOOR, TILEID.WALL,TILEID.WALL },
            4, DIRECTION.WEST),

            new TileInfo(
            "wall_north_south",
            TILEID.WALL,
            new TILEID[]
            {TILEID.WALL,TILEID.FLOOR, TILEID.WALL,TILEID.FLOOR },
            5, DIRECTION.NORTH),

            new TileInfo(
            "wall_east_west",
            TILEID.WALL,
            new TILEID[]
            {TILEID.FLOOR,TILEID.WALL, TILEID.FLOOR,TILEID.WALL },
            5, DIRECTION.EAST),

            new TileInfo(
            "wall_north_east_south",
            TILEID.WALL,
            new TILEID[]
            {TILEID.WALL,TILEID.WALL, TILEID.WALL,TILEID.FLOOR },
            6, DIRECTION.SOUTH),

            new TileInfo(
            "wall_east_south_west",
            TILEID.WALL,
            new TILEID[]
            {TILEID.FLOOR,TILEID.WALL, TILEID.WALL,TILEID.WALL },
            6, DIRECTION.WEST),

            new TileInfo(
            "wall_south_west_north",
            TILEID.WALL,
            new TILEID[]
            {TILEID.WALL,TILEID.FLOOR, TILEID.WALL,TILEID.WALL },
            6, DIRECTION.NORTH),

            new TileInfo(
            "wall_west_north_east",
            TILEID.WALL,
            new TILEID[]
            {TILEID.WALL,TILEID.WALL, TILEID.FLOOR,TILEID.WALL },
            6, DIRECTION.EAST),

            new TileInfo(
            "wall_full_cross",
            TILEID.WALL,
            new TILEID[]
            {TILEID.WALL,TILEID.WALL, TILEID.WALL,TILEID.WALL },
            7, DIRECTION.NORTH),

            new TileInfo(
            "gate_east_west",
            TILEID.WALL,
            new TILEID[]
            {TILEID.FLOOR,TILEID.WALL, TILEID.FLOOR,TILEID.WALL },
            8, DIRECTION.EAST),

            new TileInfo(
            "gate_north_south",
            TILEID.WALL,
            new TILEID[]
            {TILEID.WALL,TILEID.FLOOR, TILEID.WALL,TILEID.FLOOR },
            8, DIRECTION.NORTH),
        };

        #endregion

        public class Tile_data
        {
            public Tile_data(Vector2Int _position)
            {
                this.position = _position;
            }

            public Vector2Int position = new(0, 0);
            public bool collapsed = false;
            public TileInfo tileData = new TileInfo();
            public int tileIndex;
            public List<TileInfo> posibilities = new List<TileInfo>();
            public int entropy = 0;
            public int distance_from_center = 0;
        }


        void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player");

            string test = TileLibrary[1].name;
            reset_map();
        }

        public void reset_map()
        {
            wfc_map = new Tile_data[size.x, size.y];
            worker_list.Clear();

            actual_position = size / 2;

            Tile_data begin = new Tile_data(actual_position);
            begin.tileData = TileLibrary[0];
            begin.collapsed = true;
            wfc_map[actual_position.x, actual_position.y] = begin;
            worker_list.Add(begin);
            player.GetComponent<Rigidbody>().useGravity = false;
            player.transform.position = new(16, 10, 16);
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.E))
            {
                foreach (Transform child in transform)
                {
                    GameObject.Destroy(child.gameObject);
                }

                reset_map();

            }
            if (Input.GetKeyDown(KeyCode.G))
            {
                player.GetComponent<Rigidbody>().useGravity = true;

            }

            if (elapsed - elapsed_movement > 0.2)
            {
                elapsed_movement = elapsed;
                generate();

            }

        }

        // Update is called once per frame
        void generate()
        {
            float x_scale = 1f / size.x;
            float y_scale = 1f / size.y;

            // Debug.Log(worker_list.Count());
            foreach (Tile_data t in worker_list)
            {
                if (!t.collapsed)
                {


                    Tile_data inspect_worker = t;
                    Vector2Int ap = inspect_worker.position;
                    var placement_rule = get_placement_rule(inspect_worker);
                    inspect_worker = refresh_tile(inspect_worker, placement_rule);
                }
            }


            var sorted_worker_list = worker_list.OrderByDescending(t => t.entropy).ThenBy(t => t.distance_from_center);
            Stack<Tile_data> worker = new Stack<Tile_data>();

            foreach (Tile_data t in sorted_worker_list)
                worker.Push(t);

            while (worker.Count > 0)
            {
                Tile_data inspect_worker = worker.Pop();
                Vector2Int ap = inspect_worker.position;

                if (inspect_worker.posibilities.Count > 0)
                {
                    if (!inspect_worker.collapsed)
                    {
                        TileInfo picked_tile = inspect_worker.posibilities[Random.Range(0, inspect_worker.posibilities.Count)];
                        inspect_worker.tileData = picked_tile;
                    }
                }
                Spawn_block(getOption(inspect_worker.tileData),
                        transform,
                        new Vector2(ap.x * 2f,
                        ap.y * 2f),
                        new Vector2(x_scale, y_scale), inspect_worker.tileIndex + "_" + inspect_worker.tileData.name);

                inspect_worker.collapsed = true;
                hire_new_workers(inspect_worker);

            }

            for (int i = worker_list.Count - 1; i >= 0; i--)
            {
                if (worker_list[i].collapsed)
                    worker_list.RemoveAt(i);
            }

        }

        TILEID[] get_placement_rule(Tile_data ap)
        {
            TileInfo tileInfo = new TileInfo();
            var tilecheck = tileInfo.tileid;

            TileInfo cP = new TileInfo();

            if (ap.position.y - 1 > -1)
            {
                if (wfc_map[ap.position.x, ap.position.y - 1] != null)
                {
                    cP = wfc_map[ap.position.x, ap.position.y - 1].tileData;

                    if (cP != null)
                    {
                        tilecheck[2] = cP.tileid[0];
                    }
                }
            }

            if (ap.position.y + 1 < size.y)
            {
                if (wfc_map[ap.position.x, ap.position.y + 1] != null)
                {
                    cP = wfc_map[ap.position.x, ap.position.y + 1].tileData;
                    if (cP != null)
                    {

                        tilecheck[0] = cP.tileid[2];

                    }
                }
            }

            if (ap.position.x - 1 > -1)
            {
                if (wfc_map[ap.position.x - 1, ap.position.y] != null)
                {
                    cP = wfc_map[ap.position.x - 1, ap.position.y].tileData;
                    if (cP != null)
                    {

                        tilecheck[3] = cP.tileid[1];

                    }
                }
            }

            if (ap.position.x + 1 < size.x)
            {
                if (wfc_map[ap.position.x + 1, ap.position.y] != null)
                {
                    cP = wfc_map[ap.position.x + 1, ap.position.y].tileData;
                    if (cP != null)
                    {

                        tilecheck[1] = cP.tileid[3];

                    }
                }
            }
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

            Debug.Log("RULE:" + ap.position.ToString() + " " + tilecheck[0] + "\r\n" +
                tilecheck[3] + " - " + tilecheck[1] + "\r\n             " +
                tilecheck[2]);


            return tilecheck;
        }

        public Tile_data refresh_tile(Tile_data tileData, TILEID[] rule)
        {
            tileData.distance_from_center = Math.Abs(
                (tileData.position.x * tileData.position.y) -
                (int)Mathf.Pow(size.x / 2, 2));

            tileData.posibilities = new List<TileInfo>();

            int index = 0;
            foreach (TileInfo tl in TileLibrary)
            {
                TILEID[] t = tl.tileid;

                if (t[0] == rule[0] || rule[0] == TILEID.VACUUM)
                    if (t[1] == rule[1] || rule[1] == TILEID.VACUUM)
                        if (t[2] == rule[2] || rule[2] == TILEID.VACUUM)
                            if (t[3] == rule[3] || rule[3] == TILEID.VACUUM)
                                tileData.posibilities.Add(tl);
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

        public GameObject getOption(TileInfo option)
        {
            GameObject go = prefab_objects[option.instance];
            go.transform.eulerAngles = new(0, (int)option.direction, 0);
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
                block.transform.localRotation = go.transform.localRotation;
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
}