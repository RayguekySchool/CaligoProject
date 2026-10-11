
using UnityEngine;
using System.Collections;

public class RenderingShootingEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private GameObject targetShaderCanvas;
    [SerializeField] private PistolCanvasGun pistolaCanvasGun;

    [Header("Flash Settings")]
    [SerializeField] private float flashDuration = 0.7f;
    [SerializeField] private float fadeInDuration = 0.08f;
    [SerializeField] private float fadeOutDuration = 0.2f;

    private bool isTextureActive;
    private Coroutine flashCoroutine;

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (pistolaCanvasGun == null)
            pistolaCanvasGun = FindObjectOfType<PistolCanvasGun>();

        SetRenderTextureState(false);
    }

    public void PlayShootingFlash()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(ShootingFlashRoutine());
    }

    private IEnumerator ShootingFlashRoutine()
    {
        SetRenderTextureState(true);

        // Fade-in rápido
        yield return new WaitForSeconds(fadeInDuration);

        // Mantém o efeito visível durante o restante do flash
        float holdDuration = Mathf.Max(
            0f,
            flashDuration - fadeInDuration - fadeOutDuration
        );

        yield return new WaitForSeconds(holdDuration);

        // Fade-out
        yield return new WaitForSeconds(fadeOutDuration);

        SetRenderTextureState(false);
        flashCoroutine = null;
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
            targetCamera.targetTexture =
                activateTexture ? renderTexture : null;
        }

        if (targetShaderCanvas != null)
        {
            targetShaderCanvas.SetActive(activateTexture);
        }
    }
}
