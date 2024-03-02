using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public GameObject TileGenerator;
    public Transform player;
    public Vector3 lastSpawnPos;
    public int spawnDistance = 16;
    public List<GameObject> spawnList = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;

        for (int y = 0; y < 3; y++)
            for (int x = 0; x < 3; x++)
            {
                GameObject go = Instantiate(TileGenerator, new Vector3(x * 16, 0, y * 16), Quaternion.identity);
                go.transform.localScale = Vector3.one * spawnDistance; ;
                spawnList.Add(go);

            }
        Vector3 lastSpawnPos = Snapping.Snap(player.position / spawnDistance, Vector3.one);
    }

    // Update is called once per frame
    void Update()
    {
        bool spawnupdate = false;

        Vector3 playerPos = Snapping.Snap(player.position / spawnDistance, Vector3.one);

        if (playerPos.x - lastSpawnPos.x > 0)
        {
            Debug.Log("X RIGHT");
            spawnupdate = true;


            for (int y = -1; y < 2; y++)
            {
                GameObject go = Instantiate(TileGenerator, new Vector3((playerPos.x + 1) * spawnDistance, 0, (playerPos.z + y) * spawnDistance), Quaternion.identity);
                go.transform.localScale = Vector3.one * 16; ;
                spawnList.Add(go);
            }

            foreach (GameObject go in spawnList)
            {
                if (go.IsDestroyed())
                    continue;

                Vector3 spawnPos = Snapping.Snap(go.transform.position / spawnDistance, Vector3.one);
                if (spawnPos.x +1 < playerPos.x)
                    Destroy(go);
            }
        }






        if (playerPos.x - lastSpawnPos.x < 0)
        {
            Debug.Log("X LEFT");

            spawnupdate = true;

            for (int y = -1; y < 2; y++)
            {
                GameObject go = Instantiate(TileGenerator, new Vector3((playerPos.x - 1) * spawnDistance, 0, (playerPos.z + y) * spawnDistance), Quaternion.identity);
                go.transform.localScale = Vector3.one * 16; ;
                spawnList.Add(go);
            }

            foreach (GameObject go in spawnList)
            {
                if (go.IsDestroyed())
                    continue;

                Vector3 spawnPos = Snapping.Snap(go.transform.position / spawnDistance, Vector3.one);
                if (spawnPos.x > playerPos.x + 1)
                    Destroy(go);
            }
        }


        if (playerPos.z - lastSpawnPos.z > 0)
        {
            Debug.Log("Y UP");

            spawnupdate = true;


            for (int x = -1; x < 2; x++)
            {
                GameObject go = Instantiate(TileGenerator, new Vector3((playerPos.x + x) * spawnDistance, 0, (playerPos.z + 1) * spawnDistance), Quaternion.identity);
                go.transform.localScale = Vector3.one * 16; ;
                spawnList.Add(go);
            }

            foreach (GameObject go in spawnList)
            {
                if (go.IsDestroyed())
                    continue;

                Vector3 spawnPos = Snapping.Snap(go.transform.position / spawnDistance, Vector3.one);
                if (spawnPos.z + 1 < playerPos.z)
                    Destroy(go);
            }
        }







        if (playerPos.x - lastSpawnPos.x < 0)
        {
            Debug.Log("Y DOWN");

            spawnupdate = true;

            for (int x = -1; x < 2; x++)
            {
                GameObject go = Instantiate(TileGenerator, new Vector3((playerPos.x - x) * spawnDistance, 0, (playerPos.z - 1) * spawnDistance), Quaternion.identity);
                go.transform.localScale = Vector3.one * 16; 
                spawnList.Add(go);
            }

            foreach (GameObject go in spawnList)
            {
                if (go.IsDestroyed())
                    continue;

                Vector3 spawnPos = Snapping.Snap(go.transform.position / spawnDistance, Vector3.one);
                if (spawnPos.z > playerPos.z + 1)
                    Destroy(go);
            }
        }

        if (spawnupdate)
            lastSpawnPos = playerPos;

        for (int i = spawnList.Count - 1; i >= 0; i--)
        {
            GameObject go = spawnList[i];
            if (go.IsDestroyed())
                spawnList.RemoveAt(i);
        }
    }
}
