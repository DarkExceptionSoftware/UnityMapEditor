using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class CameraManager : MonoBehaviour
{
    public float moveSpeed = 2000;
    public GameObject camAttractor;
    Vector3 followPositioin;
    float clampAngle = 30f;
    public float inputSensivity = 1f;
    public GameObject camera;
    public GameObject player;
    private float camDistanceX2Player;
    private float camDistanceY2Player;
    private float camDistanceZ2Player;
    private float mouseX, mouseY;
    private float finalInputX, finalInputZ;
    private float smoothX, smoothY;
    private float roty = 0;
    private float rotx = 0;

    public void Start()
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        rotx = rot.x;
        roty = rot.y;
        
    }

    public void LateUpdate()
    {
       Transform target = camAttractor.transform;

        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);    
    }

    public void HandleMovement(InputManager inputmanager)
    {
        finalInputX = inputmanager.vertCamInput;
            finalInputZ = inputmanager.horCamInput;
        roty += finalInputX * inputSensivity * Time.deltaTime;
        rotx += finalInputZ * inputSensivity * Time.deltaTime;
        rotx = Mathf.Clamp(rotx, -clampAngle * 0.5f, clampAngle * 1.5f);
        Quaternion localRotation = Quaternion.Euler(rotx, roty,0);
        transform.rotation = localRotation;

    }
}
