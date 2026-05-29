using UnityEngine;

public class EffectObject : MonoBehaviour
{
    [Header("Unity Setup")]
    public float time;

    public bool shakeCamera;
    [Range(0f, 1f)]
    public float duration;
    [Range(0f, 1f)]
    public float magnetude;

    void Start()
    {
        var cameraShake = FindAnyObjectByType<CameraShake>();
        if (shakeCamera && cameraShake != null)
            StartCoroutine(cameraShake.Shake(duration, magnetude));

        Destroy(gameObject, time);
    }
}
