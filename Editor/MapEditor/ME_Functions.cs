using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TMPro.SpriteAssetUtilities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.Windows;
using static Codice.CM.Common.Serialization.PacketFileReader;
using static log4net.Appender.ColoredConsoleAppender;
using static System.Net.Mime.MediaTypeNames;
using Application = UnityEngine.Application;
using Directory = UnityEngine.Windows.Directory;
using File = UnityEngine.Windows.File;

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

                        int multidim_to_chain = (x + me.pos_x) + (me.texture.width * (y + me.pos_y));
                        Color32 seek_color = me.tex_pixels[multidim_to_chain];

                        /*     seek_color = new Color((float)Math.Round(seek_color.r, 2),
                                 (float)Math.Round(seek_color.g, 2),
                                 (float)Math.Round(seek_color.b, 2),
                                 (float)Math.Round(seek_color.a, 2)); */

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
            try
            {
                me.tex_pixels = me.texture.GetPixels32();
            }
            catch (System.Exception e)
            {
                Debug.Log("Texture not readable! Change texture import settings to read/write and pointfilter. " + e);
                return;
            }

            if (me._instance == null)
                return;

            string texture_path = AssetDatabase.GetAssetPath(me.texture);

            ME_Helper.decimate_colors(texture_path, 16);

            string palettepath = texture_path.Substring(0, texture_path.LastIndexOf("_dec")) + "_pal16.png";
            Texture2D palette = new(2, 2, me.texture.format, false, true, false);
            var rawData = System.IO.File.ReadAllBytes(palettepath);
            palette.LoadImage(rawData);
            palette.filterMode = FilterMode.Point;
            palette.alphaIsTransparency = true;
            palette.anisoLevel = 0;
            palette.Apply();
            me.pixels = palette.GetPixels32();

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

        public static bool IsEqualTo(Color me, Color other)
        {
            return me.r == other.r && me.g == other.g && me.b == other.b;
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

        public static void OnPostprocessTexture(Texture2D texture)
        {
            TextureImporter importer = new TextureImporter();
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(importer.assetPath, typeof(Texture2D));
            if (asset)
            {
                EditorUtility.SetDirty(asset);
            }
            else
            {
                texture.anisoLevel = 0;
                texture.filterMode = FilterMode.Point;
            }
        }
    }
}