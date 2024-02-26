using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEditor.PMA1980.MapEditor.ME_Functions;

namespace UnityEditor.PMA1980.MapEditor
{
    public partial class ME_Editor : EditorWindow
    {
        // Start is called before the first frame update
        public void Tilemap_tab()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false);

            GuiLine(2);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Instance");
            me._instance = (GameObject)EditorGUILayout.ObjectField(me._instance, typeof(GameObject), true);

            GUILayout.EndHorizontal();

            if (me._instance != null)
                if (me._instance.tag == "MapGenerator")
                {
                    me._instance = null;
                    Debug.Log("Put MapGenerator to _ref! This is the wrong place...");
                }
            GuiLine();


            showTexture = EditorGUILayout.Foldout(showTexture, "Level Map"); GuiLine(1);

            if (showTexture)
            {
                EditorGUI.BeginChangeCheck();
                me.texture = (Texture2D)EditorGUILayout.ObjectField(me.texture, typeof(Texture2D), false);

                if (EditorGUI.EndChangeCheck())
                {
                    refresh_palette(me);
                }

                if (me.texture != null)
                {
                    Texture2D cover = me.texture;
                    float imageWidth = Mathf.Clamp(EditorGUIUtility.currentViewWidth - 40, 10, 128);
                    float imageHeight = Mathf.Clamp(imageWidth * cover.height / cover.width, 10, 128);

                    Rect rect = GUILayoutUtility.GetRect(imageWidth, imageHeight);
                    GUI.DrawTexture(rect, cover, ScaleMode.ScaleToFit);
                }
            }
            GuiLine();

            if (me.pixels != null)
                _color_grid();
            else
            {
                GUIStyle style = new GUIStyle(GUI.skin.textArea);
                style.richText = true;

                style.wordWrap = true;
                style.margin = new RectOffset(10, 10, 10, 10);

                if (me._instance == null)
                {
                    EditorGUILayout.TextArea("<color=red>Missing instance\r\nSet library node to instance in control.</color>", style);
                }

            }

            GUILayout.EndScrollView();
        }


        private void _color_grid()
        {
            showColors = EditorGUILayout.Foldout(showColors, "Order (" + me.pixels.Count() + ")"); GuiLine(1);

            int num_of_go = 0;
            if (me._instance != null)
            {
                num_of_go = me._instance.transform.childCount;

                while (num_of_go < me.pixels.Count() - 1 && resizeInstance)
                {
                    num_of_go = me._instance.transform.childCount;
                    GameObject empty = new GameObject("placeholder");
                    empty.transform.parent = me._instance.transform;
                }
            }

            if (showColors)
            {
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.alignment = TextAnchor.MiddleLeft;

                bool cleanup = false;
                int count = 0;
                foreach (Color c in me.pixels)
                {
                    GUILayout.BeginHorizontal();

                    GuiBox(me.pixels[count]);

                    EditorGUILayout.LabelField(count.ToString().PadLeft(3,'0'), style);
                    EditorGUI.BeginChangeCheck();

                    if (count < num_of_go)
                    {
                        me.instance_slot[me.pixels[count]] = (GameObject)EditorGUILayout.ObjectField(
                            me._instance.transform.GetChild(count).gameObject, typeof(GameObject), true);

                    }
                    else
                        me.instance_slot[me.pixels[count]] = (GameObject)EditorGUILayout.ObjectField(null, typeof(GameObject), true);


                    if (EditorGUI.EndChangeCheck() && me.instance_slot[me.pixels[count]] != null)
                    {
                        GameObject go = spawn_block(me.instance_slot[me.pixels[count]], me._instance.transform, Vector2.zero, Vector2.one, true);

                        if (me._instance.transform.childCount > count)
                        {
                            DestroyImmediate(me._instance.transform.GetChild(count).gameObject);
                            go.transform.SetSiblingIndex(count);

                        }
                        cleanup = true;
                    }
                    GUILayout.EndHorizontal();
                    count++;

                    if (cleanup)
                    {
                        if (resizeInstance)
                        cleanup_library(me, me.pixels.Count());

                        if (UpdateGenerator)
                            Generate_map(me);

                    }

                    GuiLine();
                }


            }

            GuiLine(1);
        }
    }

}



