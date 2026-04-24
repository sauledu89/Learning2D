using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Secuencia cinemática de muerte estilo Enter the Gungeon.
/// Agregar a un GameObject en la escena (ej: "DeathSequence").
/// Requiere un Canvas dedicado con Sort Order alto (ej: 20).
/// </summary>
public class SniperDeathSequence : MonoBehaviour
{
    public static SniperDeathSequence Instance { get; private set; }

    [Header("Mira (Crosshair UI)")]
    [SerializeField] private RectTransform crosshairRect;
    [SerializeField] private Canvas overlayCanvas; // Canvas dedicado, Sort Order = 20

    [Header("Tiempos")]
    [Tooltip("Pausa dramática antes de que entre la mira.")]
    [SerializeField] private float preDelay = 0.45f;

    [Tooltip("Duración del deslizamiento de la mira hacia el jugador.")]
    [SerializeField] private float slideInDuration = 1.2f;

    [Tooltip("Tiempo que la mira se queda apuntando antes de disparar.")]
    [SerializeField] private float aimHoldDuration = 0.7f;

    [Tooltip("Delay entre el sonido del disparo y la animación de muerte.")]
    [SerializeField] private float postShotDelay = 0.12f;

    [Tooltip("Tiempo tras la animación de muerte hasta mostrar Game Over.")]
    [SerializeField] private float gameOverDelay = 2.0f;

    [Header("Audio")]
    [SerializeField] private AudioClip sniperShotClip;
    [SerializeField][Range(0f, 1f)] private float shotVolume = 1f;

    [Header("Camera Shake al disparo")]
    [SerializeField] private float shakeDuration = 0.45f;
    [SerializeField] private float shakeMagnitude = 0.32f;

    [Header("Overlay de muerte")]
    [SerializeField] private UnityEngine.UI.Image deathOverlay;
    [SerializeField][Range(0f, 1f)] private float overlayAlpha = 0.55f;

    private bool isPlaying = false;

    private AudioSource sfxSource;
    private Transform playerTransform;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        if (crosshairRect != null)
            crosshairRect.gameObject.SetActive(false);
    }

    /// <summary>
    /// Inicia la secuencia. PlayerHealth la llama al morir.
    /// onAnimTrigger  → activa la animación de muerte del jugador.
    /// onComplete     → muestra el panel de Game Over.
    /// </summary>
    public void Play(Transform player, Action onAnimTrigger, Action onComplete)
    {
        if (isPlaying) return;
        isPlaying = true;
        playerTransform = player;
        StartCoroutine(Sequence(onAnimTrigger, onComplete));
    }
    private IEnumerator Sequence(Action onAnimTrigger, Action onComplete)
    {
        // 0. Pausa dramática
        yield return new WaitForSecondsRealtime(preDelay);

        // Si no hay UI configurada, fallback directo
        if (crosshairRect == null || overlayCanvas == null)
        {
            if (deathOverlay != null)
            {
                Color c = deathOverlay.color;
                c.a = overlayAlpha;
                deathOverlay.color = c;
                deathOverlay.gameObject.SetActive(true);
            }
            onAnimTrigger?.Invoke();
            yield return new WaitForSecondsRealtime(gameOverDelay);
            onComplete?.Invoke();
            yield break;
        }

        // 1. Activar mira y colocarla fuera de pantalla
        crosshairRect.gameObject.SetActive(true);

        Vector2 playerCanvasPos = WorldToCanvas(playerTransform.position);

        // Entrada desde esquina aleatoria (arriba-derecha o abajo-izquierda)
        bool fromTopRight = UnityEngine.Random.value > 0.5f;
        Vector2 offScreenScreen = fromTopRight
            ? new Vector2(Screen.width + 200f, Screen.height + 200f)
            : new Vector2(-200f, -200f);

        Vector2 startCanvasPos;
        RectTransform canvasRT = overlayCanvas.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRT, offScreenScreen,
            overlayCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : overlayCanvas.worldCamera,
            out startCanvasPos);

        crosshairRect.anchoredPosition = startCanvasPos;

        // 2. Deslizar hacia el jugador con SmoothStep
        float elapsed = 0f;
        while (elapsed < slideInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / slideInDuration));

            // Actualizamos la posición del jugador en tiempo real por si la cámara se movió
            playerCanvasPos = WorldToCanvas(playerTransform.position);
            crosshairRect.anchoredPosition = Vector2.Lerp(startCanvasPos, playerCanvasPos, t);
            yield return null;
        }

        // Ajuste final exacto
        crosshairRect.anchoredPosition = WorldToCanvas(playerTransform.position);

        // 3. Mantener la mira apuntando
        float aimElapsed = 0f;
        while (aimElapsed < aimHoldDuration)
        {
            aimElapsed += Time.unscaledDeltaTime;
            crosshairRect.anchoredPosition = WorldToCanvas(playerTransform.position);
            yield return null;
        }

        // 4. DISPARO
        if (sniperShotClip != null)
            sfxSource.PlayOneShot(sniperShotClip, shotVolume);

        CameraShake.Instance?.Shake(shakeDuration, shakeMagnitude);

        yield return new WaitForSecondsRealtime(postShotDelay);

        // 5. Activar animación de muerte del jugador
        onAnimTrigger?.Invoke();

        // 6. Ocultar mira poco después
        yield return new WaitForSecondsRealtime(0.4f);
        crosshairRect.gameObject.SetActive(false);

        // 7. Esperar y mostrar Game Over
        yield return new WaitForSecondsRealtime(gameOverDelay - 0.4f);
        onComplete?.Invoke();
    }

    private Vector2 WorldToCanvas(Vector3 worldPos)
    {
        if (Camera.main == null) return Vector2.zero;

        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        RectTransform canvasRT = overlayCanvas.GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRT, screenPos,
            overlayCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : overlayCanvas.worldCamera,
            out Vector2 localPos);

        return localPos;
    }
}