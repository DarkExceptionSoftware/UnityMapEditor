using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class RaiseSpikes : MonoBehaviour
{
    public GameObject spikes;
    private float elapsed;
    private Vector3 spikePosition;
    private float booster = 1;
    // Start is called before the first frame update
    void Start()
    {
        elapsed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (elapsed > 0 && elapsed < 8) {
            elapsed += Time.deltaTime;
            spikes.transform.localPosition = Vector3.Lerp(spikes.transform.localPosition, spikePosition, Time.deltaTime * booster);

        }

        if (elapsed > 1f)
        {
            booster = 1;
            spikePosition = new(0, -0.25f, 0);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if ( other.gameObject.tag == "Player")
        {
            spikePosition = Vector3.zero;
            elapsed = Time.deltaTime;
            booster = 10;
        }
    }
}
