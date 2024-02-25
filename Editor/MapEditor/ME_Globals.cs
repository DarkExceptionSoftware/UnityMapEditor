using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.PMA1980.MapEditor
{
    // global varrables
    public static class ME_Globals
    {
        public static GameObject _ref, _instance;
        public static int width = 1, height = 1;
        public static MG_MODE modifier_mode = MG_MODE.NOTHING;
        public static Texture2D texture = null;
        public static Color[] pixels = null;
    }

    // mouse-Manipulation modes
    public enum MG_MODE { NOTHING = 0, ADD = 1, DELETE = 2, ROTATE = 3 }
}
