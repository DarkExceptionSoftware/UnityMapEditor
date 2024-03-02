using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerLocoMotion : MonoBehaviour
{
    public Transform cameraObject;
    public float movementSpeed = 7;
    public float rotationSpeed = 15;
    InputManager inputManager;
    CameraManager cameraManager;
    Rigidbody rB;
    Vector3 moveDirection;
    public PhysicMaterial pM;

    public float acceleration = 1, deceleration = 1, velPower = 1;

    void Awake()
    {
    }

    private void Start()
    {
        cameraManager = cameraObject.parent.gameObject.GetComponent<CameraManager>();
        inputManager = GetComponent<InputManager>();

        CapsuleCollider cc = gameObject.AddComponent<CapsuleCollider>();

        rB = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        rB.velocity = Vector3.ClampMagnitude(rB.velocity, 8f);
    }
    public void HandleAllMovement()
    {
        HandleMovement();
        handleRotation();
        cameraManager.HandleMovement(inputManager);
    }

    private void HandleMovement()
    {
        Vector3 direction = new(inputManager.horizontalInput, 0, inputManager.verticalInput);
        moveDirection = Quaternion.AngleAxis(cameraObject.eulerAngles.y, Vector3.up) * direction;
        Vector3 targetSpeed = Vector3.Normalize(moveDirection) * movementSpeed;
        
        float speedDif = (targetSpeed - rB.velocity).magnitude;
        float accelRate = (Mathf.Abs(targetSpeed.magnitude)) > 0.01f ? acceleration : deceleration;

        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower)
            * Mathf.Sign(speedDif);
        
        
        rB.AddForce(movement * moveDirection);
    }

    private void handleRotation()
    {
        Vector3 targetDirection = cameraObject.forward * inputManager.verticalInput;
        targetDirection = targetDirection + cameraObject.right * inputManager.horizontalInput;
        targetDirection.Normalize();
        targetDirection.y = 0;

        if (targetDirection == Vector3.zero)
            targetDirection = transform.forward;

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        transform.rotation = playerRotation;
    }
}
