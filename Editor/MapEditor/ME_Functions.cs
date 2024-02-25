using System;
using Unity.VisualScripting;
using UnityEngine;

namespace UnityEditor.PMA1980.MapEditor
{
    public class ME_Functions : MonoBehaviour
    {
        static Texture2D lastTex;
        public ME_Globals me;



        public static void spawn_game_object(RaycastHit hit, ME_Globals me)
        {
            if (Selection.activeGameObject == null && me._instance == null)
            {
                Debug.Log("MapEditor: Nothing to spawn. Select a GameObject or reference one in the Inspector!");
                return;
            }
            GameObject go = GameObject.Instantiate(Selection.activeGameObject, me._ref.transform);

            if (Selection.activeGameObject.tag == "MapGenerator")
            {
                if (me._instance != null && !me._instance.IsDestroyed())
                    go = GameObject.Instantiate(me._instance, me._ref.transform);
                else
                {
                    Debug.Log("MapEditor: Nothing to spawn. Check 'GameObject' in MapEditor-Inspector");
                    return;
                }
            }
            else
            {
                me._instance = go;
            }

            if (go.GetComponent<BoxCollider>() == null)
                go.AddComponent<BoxCollider>();

            me._instance = go;


            go.transform.parent = me._ref.transform;
            go.transform.position = hit.point;

            float x_scale = 1f / me.width;
            float y_scale = 1f / me.height;
            float min_scale = (x_scale < y_scale) ? x_scale : y_scale;
            go.transform.localScale = new(x_scale, min_scale, y_scale);
            go.tag = "Generated";


            float x = Mathf.Floor((go.transform.localPosition.x + 0.5f) * me.width);
            float y = Mathf.Floor((go.transform.localPosition.z + 0.5f) * me.height);

            go.transform.localPosition = new((x + 0.5f) * x_scale - 0.5f, hit.point.y, (y + 0.5f) * y_scale - 0.5f);
        }

        public static void Generate_map(ME_Globals me)
        {
            Transform transform = me._ref.transform;
            CleanUp(transform);

            if (me.texture == null)
                Fill_map(me);
            else
            {
                if (lastTex != me.texture)
                {
                    me.pixels = me.texture.GetPixels(0);
                    lastTex = me.texture;
                }



                float x_scale = 1f / me.width;
                float y_scale = 1f / me.height;

                int image_width = me.texture.width;

                for (int y = 0; y < me.height; y++)
                {
                    for (int x = 0; x < me.width; x++)

                        if (x < me.texture.width && y < me.texture.height)
                            if (me.pixels[x + image_width * y].r > 0)
                                spawn_block(me, transform, new Vector2(x, y), new Vector2(x_scale, y_scale));

                }
            }
        }




        public static void Fill_map(ME_Globals me)
        {
            Transform transform = me._ref.transform;

            if (me._instance != null)
            {

                float x_scale = 1f / me.width;
                float y_scale = 1f / me.height;

                for (int y = 0; y < me.height; y++)
                {
                    for (int x = 0; x < me.width; x++)
                    {

                        spawn_block(me, transform, new Vector2(x, y), new Vector2(x_scale, y_scale));
                    }
                }
            }
        }

        static void spawn_block(ME_Globals me, Transform transform, Vector2 position, Vector2 scale)
        {
            float x = position.x; float y = position.y;
            float x_scale = scale.x; float y_scale = scale.y;

            if (me._instance != null)
            {


                var block = Instantiate(me._instance);

                if (block.GetComponent<BoxCollider>() == null)
                    block.AddComponent<BoxCollider>();

                block.tag = "Generated";
                float min_scale = (x_scale < y_scale) ? x_scale : y_scale;
                block.transform.localScale = new(x_scale, min_scale, y_scale);

                block.transform.parent = transform;
                block.transform.localPosition = new((x + 0.5f) * x_scale - 0.5f, 0f, (y + 0.5f) * y_scale - 0.5f);

                block.name = "B" + min_scale.ToString();
            }
            else
            {
                Debug.Log("MapEditor: No Instance specified!");
            }
        }

        static void CleanUp(Transform transform)
        {
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }
    }
}