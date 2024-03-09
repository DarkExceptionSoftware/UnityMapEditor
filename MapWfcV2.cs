
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace PMA1980.MapGenerator
{

    public class MapWfcV2 : MonoBehaviour
    {
        public Vector2Int size, pos;
        public GameObject[] prefab_objects;
        [Range(0, 100)] public int[] Weights;
        private Vector3 move_pos, start_transform_pos;
        private float elapsed, elapsed_movement;
        private Vector3 lastPosition = Vector3.zero;

        public Tile_data[,] wfc_map;
        public List<Tile_data> worker_list = new List<Tile_data>();
        public List<Tile_data> worker_list_retry = new List<Tile_data>();

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
            3, DIRECTION.WEST, SPAM.ON_FITTING_SIDE) ,

            new TileInfo(
            "wall_east",
            TILEID.WALL,
            new TILEID[]
            {TILEID.FLOOR,TILEID.WALL, TILEID.FLOOR,TILEID.FLOOR },
            3, DIRECTION.NORTH, SPAM.ON_FITTING_SIDE) ,

            new TileInfo(
            "wall_south",
            TILEID.WALL,
            new TILEID[]
            {TILEID.FLOOR,TILEID.FLOOR, TILEID.WALL,TILEID.FLOOR },
            3, DIRECTION.EAST, SPAM.ON_FITTING_SIDE) ,

            new TileInfo(
            "wall_west",
            TILEID.WALL,
            new TILEID[]
            {TILEID.FLOOR,TILEID.FLOOR, TILEID.FLOOR,TILEID.WALL },
            3, DIRECTION.SOUTH, SPAM.ON_FITTING_SIDE) ,

            new TileInfo(
            "wall_north_east",
            TILEID.WALL,
            new TILEID[]
            {TILEID.WALL, TILEID.WALL ,TILEID.FLOOR,TILEID.FLOOR},
            4, DIRECTION.EAST, SPAM.ON_FITTING_SIDE) ,

            new TileInfo(
            "wall_north_west",
            TILEID.WALL,
            new TILEID[]
            {TILEID.WALL,TILEID.FLOOR,TILEID.FLOOR, TILEID.WALL },
            4, DIRECTION.NORTH, SPAM.ON_FITTING_SIDE) ,

            new TileInfo(
            "wall_south_east",
            TILEID.WALL,
            new TILEID[]
            {TILEID.FLOOR,TILEID.WALL, TILEID.WALL,TILEID.FLOOR },
            4, DIRECTION.SOUTH, SPAM.ON_FITTING_SIDE),

            new TileInfo(
            "wall_south_west",
            TILEID.WALL,
            new TILEID[]
            {TILEID.FLOOR,TILEID.FLOOR, TILEID.WALL,TILEID.WALL },
            4, DIRECTION.WEST, SPAM.ON_FITTING_SIDE),

            new TileInfo(
            "wall_north_south",
            TILEID.WALL,
            new TILEID[]
            {TILEID.WALL,TILEID.FLOOR, TILEID.WALL,TILEID.FLOOR },
            5, DIRECTION.NORTH, SPAM.ON_FITTING_SIDE),

            new TileInfo(
            "wall_east_west",
            TILEID.WALL,
            new TILEID[]
            {TILEID.FLOOR,TILEID.WALL, TILEID.FLOOR,TILEID.WALL },
            5, DIRECTION.EAST, SPAM.ON_FITTING_SIDE),

            new TileInfo(
            "wall_north_east_south",
            TILEID.WALL,
            new TILEID[]
            {TILEID.WALL,TILEID.WALL, TILEID.WALL,TILEID.FLOOR },
            6, DIRECTION.SOUTH, SPAM.ON_FITTING_SIDE),

            new TileInfo(
            "wall_east_south_west",
            TILEID.WALL,
            new TILEID[]
            {TILEID.FLOOR,TILEID.WALL, TILEID.WALL,TILEID.WALL },
            6, DIRECTION.WEST, SPAM.ON_FITTING_SIDE),

            new TileInfo(
            "wall_south_west_north",
            TILEID.WALL,
            new TILEID[]
            {TILEID.WALL,TILEID.FLOOR, TILEID.WALL,TILEID.WALL },
            6, DIRECTION.NORTH, SPAM.ON_FITTING_SIDE),

            new TileInfo(
            "wall_west_north_east",
            TILEID.WALL,
            new TILEID[]
            {TILEID.WALL,TILEID.WALL, TILEID.FLOOR,TILEID.WALL },
            6, DIRECTION.EAST, SPAM.ON_FITTING_SIDE),

            new TileInfo(
            "wall_full_cross",
            TILEID.WALL,
            new TILEID[]
            {TILEID.WALL,TILEID.WALL, TILEID.WALL,TILEID.WALL },
            7, DIRECTION.NORTH, SPAM.ON_NO_SIDE),

            new TileInfo(
            "gate_east_west",
            TILEID.WALL,
            new TILEID[]
            {TILEID.FLOOR,TILEID.WALL, TILEID.FLOOR,TILEID.WALL },
            8, DIRECTION.EAST, SPAM.ON_NO_SIDE),

            new TileInfo(
            "gate_north_south",
            TILEID.WALL,
            new TILEID[]
            {TILEID.WALL,TILEID.FLOOR, TILEID.WALL,TILEID.FLOOR },
            8, DIRECTION.NORTH, SPAM.ON_NO_SIDE),
        };

        #endregion

        public class Tile_data
        {
            public Tile_data(Vector2Int _position)
            {
                this.position = _position;
            }

            private Vector2Int _position;


            public Vector2Int position   // property
            {
                get { return _position; }   // get method
                set
                {
                    _position =
                        new Vector2Int(Math.Clamp(value.x, 0, 16),
                        Math.Clamp(value.y, 0, 16));
                }
            }



            public GameObject go = null;
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


            using (var writer = new StreamWriter(Application.dataPath +  "path\\to\\file.csv"))
            using (CsvWriter csv = new(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(TileLibrary);
            }
        }

        public void reset_map()
        {
            for (int i = 0; i < TileLibrary.Count(); i++)
            {
                if (Weights.Count() > i)
                {
                    TileLibrary[i].spawnChance = Weights[i];
                }
            }


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


            // if (Input.GetKeyDown(KeyCode.Space))
            if (true)
            //     if (elapsed - elapsed_movement > 0.2)
            {
                elapsed_movement = elapsed;
                generate();

            }

        }

        // Update is called once per frame
        void generate()
        {
            if (worker_list.Count == 0) return;

            float x_scale = 1f / size.x;
            float y_scale = 1f / size.y;

            // Debug.Log(worker_list.Count());
            foreach (Tile_data t in worker_list)
            {
                if (!t.collapsed)
                {


                    Tile_data inspect_worker = t;
                    Vector2Int ap = inspect_worker.position;
                    setup_posibilities(inspect_worker);
                }
            }



            //     while (worker.Count > 0)
            //    {
            collapse_tiles();
            foreach (Tile_data t in worker_list_retry)
                recreate_tiles(t);

            worker_list_retry.Clear();
        }

        public void collapse_tiles()
        {
            float x_scale = 1f / size.x;
            float y_scale = 1f / size.y;

            var sorted_worker_list = worker_list.OrderByDescending(t => t.entropy);//.ThenBy(t => t.distance_from_center);
            Stack<Tile_data> worker = new Stack<Tile_data>();

            foreach (Tile_data t in sorted_worker_list)
                worker.Push(t);

            while (worker.Count > 0)
            {

                Tile_data inspect_worker = worker.Pop();
                Vector2Int ap = inspect_worker.position;
                // Debug.Log(inspect_worker.entropy);

                if (inspect_worker.posibilities.Count > 0)
                {
                    if (!inspect_worker.collapsed)
                    {
                        int totalChance = 0;
                        foreach (var pb in inspect_worker.posibilities)
                            totalChance += pb.spawnChance;

                        int chance = Random.Range(0, totalChance + 1);
                        int cumulative = 0;

                        TileInfo picked_tile = new();

                        foreach (var pb in inspect_worker.posibilities)
                        {
                            cumulative += pb.spawnChance;
                            if (chance <= cumulative)
                            {
                                picked_tile = pb;
                                break;
                            }
                        }
                        inspect_worker.tileData = picked_tile;
                    }
                }

                GameObject go = getOption(inspect_worker.tileData);


                if (go != null)
                    inspect_worker.go = Spawn_block(go,
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

        void setup_posibilities(Tile_data ap)
        {
            TileInfo tileInfo = new TileInfo();

            var allowed_sides = tileInfo.tileid;
            var surrounding_tiles = new TILEID[4] { TILEID.VACUUM, TILEID.VACUUM, TILEID.VACUUM, TILEID.VACUUM };
            var surrounding_names = new string[4];

            TileInfo cP = new TileInfo();

            if (ap.position.y - 1 > -1)
            {
                if (wfc_map[ap.position.x, ap.position.y - 1] != null)
                {
                    cP = wfc_map[ap.position.x, ap.position.y - 1].tileData;

                    if (cP != null)
                    {
                        allowed_sides[2] = cP.tileid[0];
                        surrounding_tiles[2] = cP.tile;
                        surrounding_names[2] = cP.name;
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

                        allowed_sides[0] = cP.tileid[2];
                        surrounding_tiles[0] = cP.tile;
                        surrounding_names[0] = cP.name;
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

                        allowed_sides[3] = cP.tileid[1];
                        surrounding_tiles[3] = cP.tile;
                        surrounding_names[3] = cP.name;
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

                        allowed_sides[1] = cP.tileid[3];
                        surrounding_tiles[1] = cP.tile;
                        surrounding_names[1] = cP.name;
                    }
                }
            }
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

            /*      Debug.Log("RULE:" + ap.position.ToString() + " " + allowed_sides[0] + "\r\n" +
                      allowed_sides[3] + " - " + allowed_sides[1] + "\r\n             " +
                      allowed_sides[2]);
            */

            ap.distance_from_center = Math.Abs(
                (ap.position.x * ap.position.y) -
                (int)Mathf.Pow(size.x / 2, 2));

            ap.posibilities = new List<TileInfo>();

            int index = 0;
            foreach (TileInfo tl in TileLibrary)
            {
                bool add_candidate = false;
                TILEID[] t = tl.tileid;
                TILEID ti = tl.tile;
                // check sides

                if (t[0] == allowed_sides[0] || allowed_sides[0] == TILEID.VACUUM)
                    if (t[1] == allowed_sides[1] || allowed_sides[1] == TILEID.VACUUM)
                        if (t[2] == allowed_sides[2] || allowed_sides[2] == TILEID.VACUUM)
                            if (t[3] == allowed_sides[3] || allowed_sides[3] == TILEID.VACUUM)
                                add_candidate = true;

                if (tl.spam == SPAM.ON_FITTING_SIDE || tl.spam == SPAM.ON_NO_SIDE)
                    for (int i = 0; i < 4; i++)
                        if (ti != t[i] && ti == surrounding_tiles[i])
                        {
                            add_candidate = false;
                        }

                if (tl.spam == SPAM.ON_NO_SIDE)
                    for (int i = 0; i < 4; i++)
                        if (tl.name.Equals(surrounding_names[i]))
                        {
                            add_candidate = false;
                        }




                if (add_candidate)
                {
                    ap.posibilities.Add(tl);

                }


                index++;
            }
            ap.entropy = ap.posibilities.Count;

            if (ap.posibilities.Count == 0)
            {
                worker_list_retry.Add(ap);

            }
        }

        void recreate_tiles(Tile_data ap)
        {

            Debug.Log("RECREATING: " + ap.position.ToString());
            clear_wfc_position(ap.position.x - 1, ap.position.y);
            clear_wfc_position(ap.position.x + 1, ap.position.y);
            clear_wfc_position(ap.position.x, ap.position.y - 1);
            clear_wfc_position(ap.position.x, ap.position.y + 1);
            clear_wfc_position(ap.position.x, ap.position.y);


        }

        void clear_wfc_position(int x, int y)
        {
            if (x < 0) return;
            if (y < 0) return;
            if (x >= size.x) return;
            if (y >= size.y) return;

            if (wfc_map[x, y].go != null)
                if (!wfc_map[x, y].go.IsDestroyed())
                    Destroy(wfc_map[x, y].go);

            wfc_map[x, y] = null;
            create_worker(new Vector2Int(x, y));

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
            if (option.instance != -1)
            {

                GameObject go = prefab_objects[option.instance];
                go.transform.eulerAngles = new(0, (int)option.direction, 0);
                return go;
            }
            else return null;
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