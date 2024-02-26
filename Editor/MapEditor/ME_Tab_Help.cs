using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityEditor.PMA1980.MapEditor
{
    public partial class ME_Editor : EditorWindow
    {
        // Start is called before the first frame update
        public void Help_tab()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false);

            showHelp = EditorGUILayout.Foldout(showHelp, "Help");
            GuiLine(1);

            if (showHelp)
            {
                GUIStyle style = new GUIStyle(GUI.skin.textArea);
                style.richText = true;

                style.wordWrap = true;
                style.margin = new RectOffset(10, 10, 10, 10);
                EditorGUILayout.TextArea("Welcome to PMA1980´s Map Editor.\r\n\r\nThere is no <color=red>reference</color> selected. Create a Empty Gameobject in the hierachy and give it the <color=yellow>tag 'MapGenerator'</color>. Afterwards drag it to the Objectfield '_ref'" +
                    "\r\n\r\nAll generated Objects get a BoxCollider by default to make the Raycast work.\r\n\r\n" +
                    "All generated Objects get a 'Generated' tag. MapEditor only manipulates Objects with this tag to not mistakenly manipule Objects in scene not affecting MapEditor.", style);

                GUILayout.EndScrollView();
            }
        }
    }
}
