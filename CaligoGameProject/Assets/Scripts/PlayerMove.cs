using System.Xml.Serialization;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float playerSpeed = 10f;
    public float momentumDamping = 5f;
    public float health = 100f;

    private CharacterController characterController;
    public Animator camAnim;
    private bool isWalking;

    private Vector3 inputVector;
    private Vector3 movementVector;
    private float myGravity = -10f;
    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        GetInput();
        MovePlayer();

        camAnim.SetBool("isWalking", isWalking);

    }

    void GetInput()
    {
        //if we're holding down wasd, then give us -1, 0, 1
        if (Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.D))
        {
            inputVector = new Vector3(x: Input.GetAxisRaw("Horizontal"), y: 0f, z: Input.GetAxisRaw("Vertical"));
            inputVector.Normalize();
            inputVector = transform.TransformDirection(inputVector);

            isWalking = true;
        }
        else
        {
            inputVector = Vector3.Lerp(a: inputVector, b: Vector3.zero, t: momentumDamping * Time.deltaTime);

            isWalking = false;
        }

        //if we're not then give us whatever inputVector was at when it was last checked and lerp it towards zero
        inputVector = Vector3.Lerp(a: inputVector, b: Vector3.zero, t: momentumDamping * Time.deltaTime);

        movementVector = (inputVector * playerSpeed) + (Vector3.up * myGravity);
    }

    void MovePlayer()
    {
        characterController.Move(movementVector * Time.deltaTime);
    }
}
