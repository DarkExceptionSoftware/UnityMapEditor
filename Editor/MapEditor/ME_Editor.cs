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
   
        public GameObject _ref, _instance;
        public int width = 1, height = 1;
        public MG_MODE modifier_mode = MG_MODE.NOTHING;

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


            GUILayout.BeginHorizontal();
            GUILayout.Label("ref");


            ME_Globals._ref = (GameObject)EditorGUILayout.ObjectField(ME_Globals._ref, typeof(GameObject), true);



            GUILayout.EndHorizontal();
            // Start a code block to check for GUI changes

            if (ME_Globals._ref != null)
            {


                EditorGUI.BeginChangeCheck();

                GUILayout.BeginHorizontal();


                ME_Globals.modifier_mode = add_modifier_button(ME_Globals.modifier_mode, MG_MODE.ADD, "Add");
                ME_Globals.modifier_mode = add_modifier_button(ME_Globals.modifier_mode, MG_MODE.DELETE, "Del");
                ME_Globals.modifier_mode = add_modifier_button(ME_Globals.modifier_mode, MG_MODE.ROTATE, "Rot");


                GUILayout.EndHorizontal();



                // MODIVIED PARAMETERS CAUSING REGENERATION
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Width");
                ME_Globals.width = (int)EditorGUILayout.Slider(ME_Globals.width, 1, 10);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Height");
                ME_Globals.height = (int)EditorGUILayout.Slider(ME_Globals.height, 1, 10);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("GameObject");
                ME_Globals._instance = (GameObject)EditorGUILayout.ObjectField(ME_Globals._instance, typeof(GameObject), true);
                GUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck())
                {
                    Generate_map();
                }
            }
            else
            {
                GUIStyle style = new GUIStyle(GUI.skin.textArea);
                style.richText = true;
                
                style.wordWrap = true;
                style.margin = new RectOffset(10,10,10,10);
                EditorGUILayout.TextArea("Welcome to PMA1980´s Map Editor.\r\n\r\nThere is no <color=red>reference</color> selected. Create a Empty Gameobject in the hierachy and give it the <color=yellow>tag 'MapGenerator'</color>. Afterwards drag it to the Objectfield '_ref'" +
                    "\r\n\r\nAll generated Objects get a BoxCollider by default to make the Raycast work.\r\n\r\n" +
                    "All generated Objects get a 'Generated' tag. MapEditor only manipulates Objects with this tag to not mistakenly manipule Objects in scene not affecting MapEditor.", style);

            }
        }

        // Add a button to the Editor-window and switch the modifier mode (enum).
        // Attach the Raycast-Script if any mode is active

        public MG_MODE add_modifier_button(MG_MODE mod, MG_MODE mod_state, string text)
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

            GUI.color = new Color32(25, 255, 255, 255);
            return mod;
        }
    }
}
