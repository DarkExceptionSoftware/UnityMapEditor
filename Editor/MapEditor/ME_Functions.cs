using System;
using Unity.VisualScripting;
using UnityEngine;

namespace UnityEditor.PMA1980.MapEditor
{
    public class ME_Functions : MonoBehaviour
    {
        static Texture2D lastTex;


        public static void OnSceneGUI(SceneView sceneView)
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
                        spawn_game_object(hit);
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
                            if (ME_Globals.modifier_mode == MG_MODE.DELETE)
                            {
                                DestroyImmediate(currentTarget.gameObject);
                                consume_mouse_click = true;
                            }

                            if (ME_Globals.modifier_mode == MG_MODE.ADD)
                            {
                                spawn_game_object(hit);
                                consume_mouse_click = true;

                            }

                            if (ME_Globals.modifier_mode == MG_MODE.ROTATE)
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

        public static void spawn_game_object(RaycastHit hit)
        {
            if (Selection.activeGameObject == null && ME_Globals._instance == null)
            {
                Debug.Log("MapEditor: Nothing to spawn. Select a GameObject or reference one in the Inspector!");
                return;
            }
            GameObject go = GameObject.Instantiate(Selection.activeGameObject, ME_Globals._ref.transform);

            if (Selection.activeGameObject.tag == "MapGenerator")
            {
                if (ME_Globals._instance != null && !ME_Globals._instance.IsDestroyed())
                    go = GameObject.Instantiate(ME_Globals._instance, ME_Globals._ref.transform);
                else
                {
                    Debug.Log("MapEditor: Nothing to spawn. Check 'GameObject' in MapEditor-Inspector");
                    return;
                }
            }
            else
            {
                ME_Globals._instance = go;
            }

            if (go.GetComponent<BoxCollider>() == null)
                go.AddComponent<BoxCollider>();

            ME_Globals._instance = go;


            go.transform.parent = ME_Globals._ref.transform;
            go.transform.position = hit.point;

            float x_scale = 1f / ME_Globals.width;
            float y_scale = 1f / ME_Globals.height;
            float min_scale = (x_scale < y_scale) ? x_scale : y_scale;
            go.transform.localScale = new(x_scale, min_scale, y_scale);
            go.tag = "Generated";


            float x = Mathf.Floor((go.transform.localPosition.x + 0.5f) * ME_Globals.width);
            float y = Mathf.Floor((go.transform.localPosition.z + 0.5f) * ME_Globals.height);

            go.transform.localPosition = new((x + 0.5f) * x_scale - 0.5f, hit.point.y, (y + 0.5f) * y_scale - 0.5f);
        }

        public static void Generate_map()
        {
            Transform transform = ME_Globals._ref.transform;
            CleanUp(transform);

            if (ME_Globals.texture == null)
                Fill_map();
            else
            {
                if (lastTex != ME_Globals.texture)
                {
                    ME_Globals.pixels = ME_Globals.texture.GetPixels(0);
                    lastTex = ME_Globals.texture;
                }



                float x_scale = 1f / ME_Globals.width;
                float y_scale = 1f / ME_Globals.height;

                int image_width = ME_Globals.texture.width;

                for (int y = 0; y < ME_Globals.height; y++)
                {
                    for (int x = 0; x < ME_Globals.width; x++)
                    {
                        if (x < ME_Globals.texture.width && y < ME_Globals.texture.height)
                            if (ME_Globals.pixels[x + image_width * y].r > 0)
                                spawn_block(transform, new Vector2(x, y), new Vector2(x_scale, y_scale));

                    }
                }
            }
        }



        public static void Fill_map()
        {
            Transform transform = ME_Globals._ref.transform;

            if (ME_Globals._instance != null)
            {

                float x_scale = 1f / ME_Globals.width;
                float y_scale = 1f / ME_Globals.height;

                for (int y = 0; y < ME_Globals.height; y++)
                {
                    for (int x = 0; x < ME_Globals.width; x++)
                    {

                        spawn_block(transform, new Vector2(x, y), new Vector2(x_scale, y_scale));
                    }
                }
            }
        }

        static void spawn_block(Transform transform, Vector2 position, Vector2 scale)
        {
            float x = position.x; float y = position.y;
            float x_scale = scale.x; float y_scale = scale.y;

            var block = Instantiate(ME_Globals._instance);

            if (block.GetComponent<BoxCollider>() == null)
                block.AddComponent<BoxCollider>();

            block.tag = "Generated";
            float min_scale = (x_scale < y_scale) ? x_scale : y_scale;
            block.transform.localScale = new(x_scale, min_scale, y_scale);

            block.transform.parent = transform;
            block.transform.localPosition = new((x + 0.5f) * x_scale - 0.5f, 0f, (y + 0.5f) * y_scale - 0.5f);

            block.name = "B" + min_scale.ToString();
        }

        static void CleanUp(Transform transform)
        {
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }
    }
}