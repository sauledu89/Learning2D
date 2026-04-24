using System.Collections;
using UnityEngine;

/// <summary>
/// Agregar al mismo GameObject que CameraFollow (la cámara principal).
/// Se llama desde PlayerHealth al recibir daño.
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("Defaults")]
    [SerializeField] private float defaultDuration = 0.25f;
    [SerializeField] private float defaultMagnitude = 0.18f;

    private Coroutine shakeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    /// <summary>Lanza un shake con los parámetros por defecto.</summary>
    public void Shake() => Shake(defaultDuration, defaultMagnitude);

    /// <summary>Lanza un shake con parámetros personalizados.</summary>
    public void Shake(float duration, float magnitude)
    {
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        Vector3 originalLocalPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Atenuación hacia el final del shake (se siente más natural)
            float damper = 1f - Mathf.Clamp01(elapsed / duration);

            float offsetX = Random.Range(-1f, 1f) * magnitude * damper;
            float offsetY = Random.Range(-1f, 1f) * magnitude * damper;

            transform.localPosition = originalLocalPos + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.unscaledDeltaTime; // unscaled para funcionar en pausa/slow-mo
            yield return null;
        }

        transform.localPosition = originalLocalPos;
    }
}