
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using static UnityEditor.FilePathAttribute;
using static UnityEditor.PlayerSettings;
using Random = UnityEngine.Random;

[Progress]
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
    public Stack<Tile_data> worker = new Stack<Tile_data>();

    public Vector2Int actual_position;

    public enum OPTIONS
    {
        BLANK = 13,
        WALL_NORTH = 1, WALL_EAST = 2, WALL_SOUTH = 3, WALL_WEST = 4,
        CORNER_NE = 5, CORNER_NW = 6, CORNER_SW = 7, CORNER_SE = 8,
        EDGE_NE = 9, EDGE_NW = 10, EDGE_SW = 11, EDGE_SE = 12
    };

    public class Tile_data
    {
        public Tile_data(Vector2Int _position) { 
            this.position = _position;
        }

        public Vector2Int position = new(0, 0);
        public bool collapsed = false;
        public OPTIONS[] options = {
            OPTIONS.WALL_NORTH, OPTIONS.WALL_EAST,OPTIONS.WALL_SOUTH, OPTIONS.WALL_WEST,
            OPTIONS.CORNER_SE,OPTIONS.CORNER_NE,OPTIONS.CORNER_NW, OPTIONS.CORNER_SW,
            OPTIONS.EDGE_NE,OPTIONS.EDGE_NW,OPTIONS.EDGE_SW,OPTIONS.EDGE_SE,
            OPTIONS.BLANK
        };
    }


    void Start()
    {
        wfc_map = new Tile_data[size.x, size.y];

        actual_position = size / 2;
       
        worker.Push(new Tile_data(actual_position));
    }
    private void Update()
    {

        generate();
        Debug.Log(worker.Count);
    }
    // Update is called once per frame
    void generate()
    {
        float x_scale = 1f / size.x;
        float y_scale = 1f / size.y;

        int handled = 0;

        while (worker.Count > 0 )
        {
            handled++;

            Tile_data inspect_worker = worker.Peek();

            Vector2Int ap = inspect_worker.position;

            if (wfc_map[ap.x, ap.y] == null)
            {
                wfc_map[ap.x, ap.y] = inspect_worker;
                // Check possibilities...

                int draw_test = Random.Range(1, 14);
                wfc_map[ap.x, ap.y].collapsed = true;

                Spawn_block(getOption(draw_test), transform,
                    new Vector2(ap.x * 4, ap.y * 4), new Vector2(x_scale / 4, y_scale / 4));
                worker.Pop();
            }
            else { worker.Pop(); }


            hire_new_workers(inspect_worker);


            if (handled > 0) break;
        }
    }

    void hire_new_workers(Tile_data ap)
    {
        // add to worker stack if tile not visited yet
        // -1 means already added to worker-stack

        for (int y = -1; y < 2; y++)
        {
            for (int x = -1; x < 2; x++)
            {
                if (x == 0 && y == 0) continue;

                Tile_data cP = new Tile_data(ap.position + new Vector2Int(x,y)); ;
                if (cP.position.x >= 0 && cP.position.y >= 0 &&
                        cP.position.x < size.x && cP.position.y < size.y)
                    if (wfc_map[cP.position.x, cP.position.y] == null)
                    {
                        worker.Push(cP);
                    }

            }
        }
    }



    public void Generate_map()
    {
        float x_scale = 1f / size.x;
        float y_scale = 1f / size.y;

        int image_width = map.width;

        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {

                int multidim_to_chain = (x + pos.x) + (image_width * (y + pos.y));
                multidim_to_chain = Mathf.Clamp(multidim_to_chain, 0, map_pixels.Length - 1);
                try
                {
                    Color32 seek_color = map_pixels[multidim_to_chain];

                    var go_to_spawn = instance_slots[seek_color];
                    Spawn_block(go_to_spawn, transform, new Vector2(x, y), new Vector2(x_scale, y_scale));

                }
                catch
                {
                    Debug.Log("Color not found");
                }
            }
        }
    }



    public GameObject getOption(int option)
    {
        int instance = 0; float rot = 0;

        switch ((OPTIONS)option)
        {
            case OPTIONS.BLANK:
                instance = 0; break;
            case OPTIONS.WALL_NORTH:
                instance = 1; break;
            case OPTIONS.WALL_EAST:
                instance = 1; rot = 90; break;
            case OPTIONS.WALL_SOUTH:
                instance = 1; rot = 180; break;
            case OPTIONS.WALL_WEST:
                instance = 1; rot = 270; break;
            case OPTIONS.CORNER_NE:
                instance = 1; rot = 0; break;
            case OPTIONS.CORNER_NW:
                instance = 1; rot = 90; break;
            case OPTIONS.CORNER_SW:
                instance = 1; rot = 180; break;
            case OPTIONS.CORNER_SE:
                instance = 1; rot = 270; break;
            case OPTIONS.EDGE_NE:
                instance = 1; rot = 0; break;
            case OPTIONS.EDGE_NW:
                instance = 1; rot = 90; break;
            case OPTIONS.EDGE_SW:
                instance = 1; rot = 180; break;
            case OPTIONS.EDGE_SE:
                instance = 1; rot = 270; break;
        }

        GameObject go = prefab_objects[instance];
        go.transform.eulerAngles = new(0, rot, 0);
        return go;
    }

    public GameObject Spawn_block(GameObject go, Transform transform, Vector2 position, Vector2 scale, bool originalName = false)
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

            block.transform.position = new(x * x_scale, 0f, y * y_scale);

            if (!originalName)
                block.name = "B" + min_scale.ToString();
            block.transform.localScale = new(x_scale, min_scale, y_scale);

            return block;
        }
        else { return null; }
    }
}
