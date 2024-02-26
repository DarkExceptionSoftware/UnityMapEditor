using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro.SpriteAssetUtilities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static Codice.CM.Common.Serialization.PacketFileReader;

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


                float x_scale = 1f / me.width;
                float y_scale = 1f / me.height;

                int image_width = me.texture.width;

                for (int y = 0; y < me.height; y++)
                {
                    for (int x = 0; x < me.width; x++)
                    {

                        int multidim_to_chain = (x + me.pos_x) + (image_width * (y + me.pos_y));
                        Color seek_color = me.tex_pixels[multidim_to_chain];
                        var go_to_spawn = me.instance_slot[seek_color];

                        if (go_to_spawn != null)
                            spawn_block(go_to_spawn, transform, new Vector2(x, y), new Vector2(x_scale, y_scale));

                        else
                                if (me._instance.transform.childCount > 0)
                            spawn_block(me._instance.transform.GetChild(1).gameObject, transform, new Vector2(x, y), new Vector2(x_scale, y_scale));
                    }
                }
            }
        }

        public static void refresh_palette(ME_Globals me)
        {
            if (me._instance == null)

                return;

            try
            {
                me.tex_pixels = me.texture.GetPixels();
            }
            catch (Exception e)
            {
                Debug.Log("Texture not readable! Change texture import settings to read/write and pointfilter. " + e);
                return;
            }
            me.pixels = fetch_palette(me.texture);

            if (me._instance.transform.childCount < me.pixels.Count())
            {
                Debug.Log("MapEditor: We have " + me.pixels.Count() + " colors and " + me._instance.transform.childCount
            + " possible Instances to spawn. Please add/remove Instances from library-node.");
                return;
            }
            me.instance_slot.Clear();

            for (int i = 0; i < me.pixels.Length; i++)
            {
                me.instance_slot.Add(me.pixels[i], me._instance.transform.GetChild(i).gameObject);
            }


        }




        public static Color[] fetch_palette(Texture2D tex)
        {
            Texture2D palette_tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);

            var contentPath = Application.dataPath + "/../Assets/Ressources/LevelPalettes/";

            if (!Directory.Exists(contentPath))
                Directory.CreateDirectory(contentPath);

            var dirPath = Application.dataPath + "/../Assets/Ressources/LevelPalettes/"
                + tex.name + "_palette" + ".png";

            byte[] fileData;

            if (File.Exists(dirPath))
            {
                fileData = File.ReadAllBytes(dirPath);
                palette_tex.LoadImage(fileData);
            }
            else
            {
                // new file
                Color[] cols = tex.GetPixels();
                List<Color> palette = new List<Color>();

                foreach (Color col in cols)
                    if (!palette.Contains(col))
                        palette.Add(col);

                palette_tex.filterMode = FilterMode.Point;
                palette_tex.name = tex.name;
                palette_tex.alphaIsTransparency = true;
                palette_tex.Reinitialize(palette.Count, 1);

                Color[] palette_cols = palette.ToArray();
                // Array.Resize(ref palette_cols, 1024);

                palette_tex.SetPixels(palette_cols);
                palette_tex.Apply();

                save_palette(palette_tex);
            }

            Color[] _cols = palette_tex.GetPixels();

            int countColors = 0;
            foreach (Color col in _cols)
            {
                if (col.a == 0)
                    break;
                countColors++;

            }
            return _cols;
        }

        public static void save_palette(Texture2D tex)
        {
            byte[] bytes = tex.EncodeToPNG();
            var dirPath = Application.dataPath + "/../Assets/Ressources/LevelPalettes/";
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            File.WriteAllBytes(dirPath + tex.name + "_palette" + ".png", bytes);
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

                        spawn_block(me._instance, transform, new Vector2(x, y), new Vector2(x_scale, y_scale));
                    }
                }
            }
        }

        public static GameObject spawn_block(GameObject go, Transform transform, Vector2 position, Vector2 scale, bool originalName = false)
        {
            float x = position.x; float y = position.y;
            float x_scale = scale.x; float y_scale = scale.y;

            if (go != null)
            {


                var block = Instantiate(go);

                if (block.GetComponent<BoxCollider>() == null)
                    block.AddComponent<BoxCollider>();

                block.tag = "Generated";
                float min_scale = (x_scale < y_scale) ? x_scale : y_scale;
                block.transform.localScale = new(x_scale, min_scale, y_scale);

                block.transform.parent = transform;
                block.transform.localPosition = new((x + 0.5f) * x_scale - 0.5f, 0f, (y + 0.5f) * y_scale - 0.5f);

                if (!originalName)
                    block.name = "B" + min_scale.ToString();

                return block;

            }
            else
            {
                Debug.Log("MapEditor: No Instance specified!");
                return null;
            }
        }

        static void CleanUp(Transform transform)
        {
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }

        public static void cleanup_library(ME_Globals me, int num_of_go) 
        {
            while (me._instance.transform.childCount > num_of_go)
            {
                DestroyImmediate(me._instance.transform.GetChild(me._instance.transform.childCount - 1).gameObject);
            }
        }

    }
}