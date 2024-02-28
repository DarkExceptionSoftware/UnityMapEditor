using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditorInternal;
using UnityEngine;
using static UnityEditor.PMA1980.MapEditor.ME_Functions;


namespace UnityEditor.PMA1980.MapEditor
{
    [InitializeOnLoad]
    // Some usefull additions to Tags on Unity
    public class Startup
    {
        static Startup()
        {
            InternalEditorUtility.AddTag("Generated");
            InternalEditorUtility.AddTag("GeneratorBase");
            Debug.Log("PMA1980's MapEditorScript Active.\r\nOpen Topmenu -> Windows -> MapEditor to use.");
        }
    }

    // Defines the layout of the Editor-Window

    public partial class ME_Editor : EditorWindow
    {
        public string[] tabs = new string[] { "Control", "TileMap", "Help" };
        public int tab_sel = 0;

        [SerializeField]
        public ME_Globals me;
        public GameObject _ref, _instance;
        public int width = 1, height = 1;
        public MG_MODE modifier_mode = MG_MODE.NOTHING;
        public Texture2D texture = null;
        public Color[] pixels = null;
        public Texture2D lastTex;

        bool showRef, showColors = false, showTexture = true, showModifier = true, showHelp = true, showScale = true, showOptions = true;
        GameObject lastRef;

        Vector2 scrollPosition;

        // Entry to Unitys topmenu

        [MenuItem("Window/MapEditor")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            ME_Editor t = (ME_Editor)GetWindow(typeof(ME_Editor), false, "PMA1980 Map Editor");
            //t.minSize = new(300, 200);
            //t.maxSize = new(300, 200);
        }

        // Defines the layout of the Editor-Window

        public void OnGUI()
        {
            if (me == null)
            {
                me = new ME_Globals();
            }

            DontDestroyOnLoad(this);

            tab_sel = GUILayout.Toolbar(tab_sel, tabs);

            switch (tab_sel)
            {
                case 0: Control_tab(); break;
                case 1: Tilemap_tab(); break;
                case 2: Help_tab(); break;
            }
        }

        public void Control_tab()
        {


            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false);

            showRef = EditorGUILayout.Foldout(showRef, "Ref & Instance");
            GuiLine(1);

            if (showRef)
            {

                GUILayout.BeginHorizontal();
                GUILayout.Label("ref");
                me._ref = (GameObject)EditorGUILayout.ObjectField(me._ref, typeof(GameObject), true);

                if (me._ref != null)
                    if (me._ref.tag != "MapGenerator")
                    {
                        me._ref = null;
                        Debug.Log("Wrong node or node not having MapGenerator tag attached.");
                    }

                GUILayout.EndHorizontal();


            }

            // Start a code block to check for GUI changes
            if (me._ref != null)
            {
                _refadded();

                if (me._ref != lastRef && me._ref != null)
                {
                    lastRef = me._ref;
                }
            }
            _options();
            GUILayout.EndScrollView();
        }


        // Hierachy node referenced

        private void _refadded()
        {

            EditorGUI.BeginChangeCheck();

            showModifier = EditorGUILayout.Foldout(showModifier, "Modifiers"); GuiLine(1);

            if (showModifier)
            {
                GUILayout.BeginHorizontal();
                me.modifier_mode = add_modifier_button(me.modifier_mode, MG_MODE.ADD, "Add");
                me.modifier_mode = add_modifier_button(me.modifier_mode, MG_MODE.DELETE, "Del");
                me.modifier_mode = add_modifier_button(me.modifier_mode, MG_MODE.ROTATE, "Rot");
                GUILayout.EndHorizontal(); GuiLine();

            }

            showScale = EditorGUILayout.Foldout(showScale, "Alignment"); GuiLine(1);

            if (showScale)
            {

                Vector2 maxsize = new(100, 100);
                Vector2 maxoffset = new(0, 0);
                if (texture != null)
                {
                    maxsize.x = texture.width; maxsize.y = texture.height;
                    maxoffset.x = texture.width - maxsize.x; maxoffset.y = texture.height - maxsize.y;
                }
                GUILayout.BeginHorizontal();
                GUILayout.Label("X"); me.pos_x = (int)EditorGUILayout.Slider(me.pos_x, 0, 512);
                GUILayout.EndHorizontal(); GuiLine(); GUILayout.BeginHorizontal();

                GUILayout.Label("Y"); me.pos_y = (int)EditorGUILayout.Slider(me.pos_y, 0, 512);
                GUILayout.EndHorizontal(); GuiLine(); GUILayout.BeginHorizontal();

                GUILayout.Label("W"); me.width = (int)EditorGUILayout.Slider(me.width, 1, maxsize.x);
                GUILayout.EndHorizontal(); GuiLine(); GUILayout.BeginHorizontal();

                GUILayout.Label("H"); me.height = (int)EditorGUILayout.Slider(me.height, 1, maxsize.y);
                GUILayout.EndHorizontal();
                GuiLine();
            }



            if (EditorGUI.EndChangeCheck())
            {
                Generate_map(me);
            }
        }


        // Hierachy node not referenced. Show Help.


        // Add a button to the Editor-window and switch the modifier mode (enum).
        // Attach the Raycast-Script if any mode is active

        void GuiLine(int i_height = 2)

        {

            Rect rect = EditorGUILayout.GetControlRect(false, i_height);

            rect.height = i_height * 1f;

            EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f, 1));

            if (i_height == 2)
            {
                rect.x -= 6; rect.height = 1;
                EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f, 1));

            }

        }

        void GuiBox(Color c)

        {
            Rect rect = GUILayoutUtility.GetRect(16f, 16f);
            rect.xMax = 16f;
            EditorGUI.DrawRect(rect, c);
        }


        private MG_MODE add_modifier_button(MG_MODE mod, MG_MODE mod_state, string text)
        {
            if (mod == mod_state)
                GUI.color = new Color32(255, 255, 0, 255);

            if (GUILayout.Button(text))
            {
                if (mod == mod_state)
                {
                    SceneView.duringSceneGui -= OnSceneGUI;

                    mod = MG_MODE.NOTHING;
                }
                else
                {
                    SceneView.duringSceneGui += OnSceneGUI;
                    mod = mod_state;
                }
            }

            GUI.color = new Color32(255, 255, 255, 255);
            return mod;
        }


        // react to mouse raycasting from editorcamera

        public void OnSceneGUI(SceneView sceneView)
        {
            int controlId = GUIUtility.GetControlID(FocusType.Passive);
            Event current = Event.current;


            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit) && !current.alt)
                {

                    GameObject cGo = hit.collider.gameObject;
                    bool consume_mouse_click = false;

                    // Clicked on the map-generator. Spawn go at clicked position

                    if (cGo.tag == "MapGenerator" && hit.normal == Vector3.up)
                    {
                        spawn_game_object(hit, me);
                        consume_mouse_click = true;
                    }
                    else
                    {
                        // Go up the hierachy and look for 'Generated' Tag in ancestors

                        Transform currentTarget = cGo.transform;
                        while ((currentTarget != null) && (!currentTarget.CompareTag("Generated")))
                        {
                            currentTarget = currentTarget.parent;
                        }

                        // We clicked on generated GameObject. perform a task

                        if (currentTarget != null)
                        {
                            if (me.modifier_mode == MG_MODE.DELETE)
                            {
                                DestroyImmediate(currentTarget.gameObject);
                                consume_mouse_click = true;
                            }

                            if (me.modifier_mode == MG_MODE.ADD)
                            {
                                spawn_game_object(hit, me);
                                consume_mouse_click = true;

                            }

                            if (me.modifier_mode == MG_MODE.ROTATE)
                            {
                                hit.collider.gameObject.transform.Rotate(Vector3.up, 90);
                                consume_mouse_click = true;

                            }
                        }
                        else
                        {
                            // Clicked anywhere... Disable the script

                            SceneView.duringSceneGui -= OnSceneGUI;

                        }
                    }
                    if (consume_mouse_click)
                    {
                        GUIUtility.hotControl = controlId;
                        Event.current.Use();
                    }
                }
            }
        }


        public bool resizeInstance = true, UpdateGenerator = true;

        void _options()
        {
            showOptions = EditorGUILayout.Foldout(showOptions, "Settings");
            GuiLine(1);
       
            GUIStyle style = GUI.skin.toggle;
            style.alignment = TextAnchor.MiddleRight;
            if (showOptions)
            {
                EditorGUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();
                resizeInstance = EditorGUILayout.Toggle("Resize Library", resizeInstance, style);
               

                GUILayout.EndHorizontal(); GuiLine(); EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                UpdateGenerator = EditorGUILayout.Toggle("Update Generator", resizeInstance, style);
                GUILayout.EndHorizontal();
                GuiLine();

            }
        }
    }
}



