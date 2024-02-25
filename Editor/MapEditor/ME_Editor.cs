using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using UnityEditor;
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

    public class ME_Editor : EditorWindow
    {
        [SerializeField]
        public ME_Globals me;
        public GameObject _ref, _instance;
        public int width = 1, height = 1;
        public MG_MODE modifier_mode = MG_MODE.NOTHING;
        public Texture2D texture = null;
        public Color[] pixels = null;

        bool showRef, showTexture = true, showModifier = true, showHelp = true, showScale = true;
        GameObject lastRef;

        Vector2 scrollPosition;

        // Entry to Unitys topmenu

        [MenuItem("Window/MapEditor")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(ME_Editor), false, "PMA1980 Map Editor");
        }

        // Defines the layout of the Editor-Window

        public void OnGUI()
        {
            if (me == null) { me = new ME_Globals(); }
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false);

            showRef = EditorGUILayout.Foldout(showRef, "Ref & Instance");
            GuiLine(1);

            if (showRef)
            {

                GUILayout.BeginHorizontal();
                GUILayout.Label("ref");
                me._ref = (GameObject)EditorGUILayout.ObjectField(me._ref, typeof(GameObject), true);
                GUILayout.EndHorizontal();

                if (me._ref != null)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Instance");
                    me._instance = (GameObject)EditorGUILayout.ObjectField(me._instance, typeof(GameObject), true);
                    GUILayout.EndHorizontal();
                }
                GuiLine();

            }

            // Start a code block to check for GUI changes
            if (me._ref != null)
            {
                _refadded();

                if (me._ref != lastRef && me._ref != null)
                {
                    showHelp = false;
                    lastRef = me._ref;
                }
            }

            showHelp = EditorGUILayout.Foldout(showHelp, "Help");
            GuiLine(1);

            if (showHelp)
            {
                _noref();
                GuiLine();
            }


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


            // MODIVIED PARAMETERS CAUSING REGENERATION
            EditorGUI.BeginChangeCheck();

            showTexture = EditorGUILayout.Foldout(showTexture, "Level Map"); GuiLine(1);

            if (showTexture)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("LevelMap");
                me.texture = (Texture2D)EditorGUILayout.ObjectField(me.texture, typeof(Texture2D), false);
                GUILayout.EndHorizontal();

                if (me.texture != null)
                {
                    Texture2D cover = me.texture;
                    float imageWidth = Mathf.Clamp(EditorGUIUtility.currentViewWidth - 40, 10, 128);
                    float imageHeight = Mathf.Clamp(imageWidth * cover.height / cover.width, 10, 128);

                    Rect rect = GUILayoutUtility.GetRect(imageWidth, imageHeight);
                    GUI.DrawTexture(rect, cover, ScaleMode.ScaleToFit);
                }
                GuiLine();

            }

            showScale = EditorGUILayout.Foldout(showScale, "Scale"); GuiLine(1);

            if (showScale)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Width");
                me.width = (int)EditorGUILayout.Slider(me.width, 1, 200);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Height");
                me.height = (int)EditorGUILayout.Slider(me.height, 1, 200);
                GUILayout.EndHorizontal(); GuiLine();

            }



            if (EditorGUI.EndChangeCheck())
            {
                Generate_map(me);
            }
        }

        // Hierachy node not referenced. Show Help.

        private void _noref()
        {
            GUIStyle style = new GUIStyle(GUI.skin.textArea);
            style.richText = true;

            style.wordWrap = true;
            style.margin = new RectOffset(10, 10, 10, 10);
            EditorGUILayout.TextArea("Welcome to PMA1980´s Map Editor.\r\n\r\nThere is no <color=red>reference</color> selected. Create a Empty Gameobject in the hierachy and give it the <color=yellow>tag 'MapGenerator'</color>. Afterwards drag it to the Objectfield '_ref'" +
                "\r\n\r\nAll generated Objects get a BoxCollider by default to make the Raycast work.\r\n\r\n" +
                "All generated Objects get a 'Generated' tag. MapEditor only manipulates Objects with this tag to not mistakenly manipule Objects in scene not affecting MapEditor.", style);


        }

        // Add a button to the Editor-window and switch the modifier mode (enum).
        // Attach the Raycast-Script if any mode is active

        void GuiLine(int i_height = 2)

        {

            Rect rect = EditorGUILayout.GetControlRect(false, i_height);

            rect.height = i_height * 1f;



            EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f, 1));

            if (i_height == 2)
            {
                rect.x -= 6;rect.height = 1;
                EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f, 1));

            }

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
    }


}
