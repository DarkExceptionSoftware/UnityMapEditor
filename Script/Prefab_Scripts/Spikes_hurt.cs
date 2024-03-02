using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes_hurt : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            Vector3 push = Vector3.Normalize(new(-rb.velocity.x,0,-rb.velocity.z));
            push.y = 0.5f;
            rb.AddForce(push * 20,ForceMode.VelocityChange);
        }
    }
}
