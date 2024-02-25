using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.PMA1980.MapEditor
{
    // global varrables
    public class ME_Globals
    {

        public GameObject _ref, _instance;
        public int width = 1, height = 1;
        public MG_MODE modifier_mode = MG_MODE.NOTHING;
        public Texture2D texture = null;
        public Color[] pixels = null;

        public ME_Globals()
        {
#pragma warning disable CS0168 // Variable ist deklariert, wird jedoch niemals verwendet
#pragma warning disable CS0219 // Variable ist zugewiesen, der Wert wird jedoch niemals verwendet
            GameObject _ref, _instance;
            width = 1; height = 1;
            MG_MODE modifier_mode = MG_MODE.NOTHING;
            Texture2D texture = null;
            Color[] pixels = null;
#pragma warning restore CS0168 // Variable ist deklariert, wird jedoch niemals verwendet
#pragma warning restore CS0219 // Variable ist zugewiesen, der Wert wird jedoch niemals verwendet

        }
    }

    // mouse-Manipulation modes
    public enum MG_MODE { NOTHING = 0, ADD = 1, DELETE = 2, ROTATE = 3 }
}
