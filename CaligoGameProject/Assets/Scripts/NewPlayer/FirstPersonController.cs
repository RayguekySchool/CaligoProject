using Unity.VisualScripting;
using UnityEngine;
public class FirstPersonController : MonoBehaviour
{
    public bool CanMove { get; private set; } = true;
    private bool IsSprinting => canSprint && Input.GetKey(sprintKey);
    private bool ShouldJump => Input.GetKeyDown(jumpKey) && characterController.isGrounded;

    [Header("Functional Options")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canJump = true;

    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;

    [Header("Movement Paramenters")]
    [SerializeField] private float walkSpeed = 3.0f; 
    [SerializeField] private float sprintSpeed = 6.0f;

    [Header("Jumping Parameters")]
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float gravity = 30.0f;

    [Header("Look Parameters")]
    [SerializeField, UnityEngine.Range(1, 10)] private float lookSpeedX = 2.0f; 
    [SerializeField, UnityEngine.Range(1, 10)] private float lookSpeedY = 2.0f; 
    [SerializeField, UnityEngine.Range(1, 100)] private float upperLookLimit = 50.0f; 
    [SerializeField, UnityEngine.Range(1, 100)] private float lowerLookLimit = 30.0f; 
    
    private Camera playerCamera; 
    private CharacterController characterController; 

    private Vector3 moveDirection; 
    private Vector2 currentInput;
    
    private float rotationX = 0; 
    
    void Awake() 
    { playerCamera = GetComponentInChildren<Camera>(); 
        characterController = GetComponent<CharacterController>(); 
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; 
    } 
    void Update() 
    { 
        if (CanMove) 
        {
            HandleMovementInput(); 
            HandleMouseLook();

            if (canJump)
                HandleJump();
            
            ApplyFineMovements();
        }
    } 
    private void HandleMovementInput() 
    { 
        currentInput = new Vector2((IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical"), (IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal")); 

        float moveDirectionY = moveDirection.y; 
        moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.x) + (transform.TransformDirection(Vector3.right) * currentInput.y); 
        moveDirection.y = moveDirectionY; 
    }
    
    private void HandleMouseLook() 
    {
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY; 
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit); 
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0); 
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0); 
    } 

    private void HandleJump()
    {
        if (ShouldJump)
        {
            moveDirection.y = jumpForce;
        }
    }

    private void ApplyFineMovements()
    {
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        else if (moveDirection.y < 0)
        {
            moveDirection.y = -2f;
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }


}