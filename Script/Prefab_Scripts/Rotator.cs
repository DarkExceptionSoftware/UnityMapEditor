using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var randomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        transform.rotation = randomRotation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
