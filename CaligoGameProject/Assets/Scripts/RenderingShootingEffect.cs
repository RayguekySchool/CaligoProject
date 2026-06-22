using UnityEngine;

public class RenderingShootingEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private GameObject targetShaderCanvas;
    [SerializeField] private PistolCanvasGun pistolaCanvasGun;

    [Header("Settings")]
    [SerializeField] public bool StartWithTextureOn = false;

    private bool isTextureActive;

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        SetRenderTextureState(StartWithTextureOn);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleRenderTexture();
        }
    }

    public void ToggleRenderTexture()
    {
        SetRenderTextureState(!isTextureActive);
    }

    public void SetRenderTextureState(bool activateTexture)
    {
        isTextureActive = activateTexture;

        if (targetCamera != null)
        {
            targetCamera.targetTexture = activateTexture ? renderTexture : null;
        }

        if (targetShaderCanvas != null)
        {
            targetShaderCanvas.SetActive(activateTexture);
        }
        else
        {
            Debug.LogWarning("CameraTextureCanvasToggler: No Canvas GameObject assigned!");
        }
    }
}
