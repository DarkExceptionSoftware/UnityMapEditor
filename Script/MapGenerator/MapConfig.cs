using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapConfig : MonoBehaviour
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

    // Start is called before the first frame update
    void Start()
    {
        var scaled = transform.parent.localScale;
        transform.parent.localScale = Vector3.one;

        transform.position = Snapping.Snap(transform.position, Vector3.one);
        pos = new((int)(transform.position.x ),
            (int)(transform.position.z ));

        map_pixels = map.GetPixels();
        CleanUp();
        BuildDictionary();
        Generate_map();
        transform.parent.localScale = scaled;
    }


    public void CleanUp()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
    }

    public void BuildDictionary()
    {
        for (int i = 0; i < pixels.Count(); i++)
            if (prefab_objects[i] != null)
                instance_slots.Add(pixels[i], prefab_objects[i]);
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
            block.transform.localScale = new(x_scale, min_scale, y_scale);

            block.transform.parent = transform;
            block.transform.localPosition = new((x + 0.5f) * x_scale - 0.5f, 0f, (y + 0.5f) * y_scale - 0.5f);

            if (!originalName)
                block.name = "B" + min_scale.ToString();

            return block;
        }
        else { return null; }
    }
}
