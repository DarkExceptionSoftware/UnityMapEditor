
using Unity.VisualScripting;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    PlayerControls playerControls;
    public Vector2 movementInput;
    public Vector2 camInput;
    public float verticalInput;
    public float horizontalInput;
    public float vertCamInput;
    public float horCamInput;

    public void Update()
    {
    }



    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();
            playerControls.PlayerMovement.Movement.performed += 
                i => movementInput = i.ReadValue<Vector2>();

            playerControls.CameraMovement.LookAround.performed +=
                i => camInput = i.ReadValue<Vector2>(); 
        }
        playerControls.Enable();
    }


    public void HandleAllInputs()
    {
        HandleMovementInput();
        HandleCameraInput();
    }

    private void HandleCameraInput()
    {
        vertCamInput = camInput.x;
        horCamInput = camInput.y;   
    }

    private void HandleMovementInput()
    {
        verticalInput = movementInput.y;
        horizontalInput = movementInput.x;
    }


    private void OnDisable()
    {
        playerControls.Disable();
    }
}
