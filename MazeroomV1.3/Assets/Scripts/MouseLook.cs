using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float sensitivity = 1.5f;
    public float smoothing = 10f;
    public float minY = -90f;
    public float maxY = 90f;

    private float xMousePos;
    private float yMousePos;
    private float smoothedMousePosX;
    private float smoothedMousePosY;
    private float currentLookingPosX;
    private float currentLookingPosY;

    private void Start()
    {
        //lock and hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        GetInput();
        ModifyInput();
        MovePlayer();
    }

    void GetInput()
    {
        xMousePos = Input.GetAxisRaw("Mouse X");
        yMousePos = Input.GetAxisRaw("Mouse Y");
    }

    void ModifyInput()
    {
        xMousePos *= sensitivity * smoothing;
        yMousePos *= sensitivity * smoothing;

        smoothedMousePosX = Mathf.Lerp(smoothedMousePosX, xMousePos, 1f / smoothing);
        smoothedMousePosY = Mathf.Lerp(smoothedMousePosY, yMousePos, 1f / smoothing);
    }

    void MovePlayer()
    {
        currentLookingPosX += smoothedMousePosX;
        currentLookingPosY -= smoothedMousePosY; // Inverte para olhar para cima/baixo corretamente
        currentLookingPosY = Mathf.Clamp(currentLookingPosY, minY, maxY);

        // Rotaciona o objeto no eixo Y (olhar para os lados) e no eixo X (olhar para cima/baixo)
        transform.localRotation = Quaternion.Euler(currentLookingPosY, currentLookingPosX, 0f);
    }
}
