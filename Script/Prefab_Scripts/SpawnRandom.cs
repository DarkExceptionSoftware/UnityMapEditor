using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnRandom : MonoBehaviour
{
    int r = 0;
    int selection = -1;

    // Start is called before the first frame update
    void Update()
    {



        if (selection == -1)
        {
            r = Random.Range(0, 101);


            switch (r)
            {
                case < 40:
                    selection = 0;
                    break;
                case < 80:
                    selection = 1;
                    break;
                case < 90:
                    selection = 2;
                    break;
                default:
                    selection = 3;
                    break;
            }

            GameObject selected = transform.GetChild(selection).gameObject;
            selected.SetActive(true);

            for (int i  = 3; i >= 0; i--)
            {
                if (i != selection)
                Destroy(transform.GetChild(i).gameObject);
            }


            if (selected.transform.childCount > 0)
                transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    // Update is called once per frame

}
