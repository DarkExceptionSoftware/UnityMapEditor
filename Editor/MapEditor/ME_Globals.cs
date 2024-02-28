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
        public int width = 1, height = 1,pos_x, pos_y;
        public MG_MODE modifier_mode = MG_MODE.NOTHING;
        public Texture2D texture = null;
        public Color32[] pixels, tex_pixels;
        public Dictionary<Color32, GameObject> instance_slot;
        public ME_Globals()
        {
            width = 1; height = 1; pos_x = 0; pos_y = 0;
            modifier_mode = MG_MODE.NOTHING;
            texture = null;
            pixels = null;

            tex_pixels = null;
            instance_slot = new Dictionary<Color32, GameObject>();

        }
    }

    // mouse-Manipulation modes
    public enum MG_MODE { NOTHING = 0, ADD = 1, DELETE = 2, ROTATE = 3 }
}
