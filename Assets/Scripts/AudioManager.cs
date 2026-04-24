using UnityEngine;

/// <summary>
/// Singleton centralizado para música y SFX.
/// Agregar a un GameObject en la escena de juego.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Música")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip levelMusic;
    [SerializeField][Range(0f, 1f)] private float musicVolume = 0.4f;

    [Header("SFX — Jugador")]
    [SerializeField] private AudioClip playerShootSFX;
    [SerializeField] private AudioClip playerHurtSFX;
    [SerializeField] private AudioClip playerDeathSFX;

    [Header("SFX — Enemigo")]
    [SerializeField] private AudioClip enemyShootSFX;
    [SerializeField] private AudioClip enemyHurtSFX;
    [SerializeField] private AudioClip enemyDeathSFX;

    [Header("SFX — Mundo")]
    [SerializeField] private AudioClip collectibleSFX;

    [Header("Volúmenes individuales")]
    [SerializeField][Range(0f, 1f)] private float playerShootVol = 0.8f;
    [SerializeField][Range(0f, 1f)] private float enemyShootVol = 0.5f;
    [SerializeField][Range(0f, 1f)] private float hurtVol = 1.0f;
    [SerializeField][Range(0f, 1f)] private float collectibleVol = 0.9f;

    // AudioSource dedicado para SFX (PlayOneShot para no cortar sonidos)
    private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Crear fuente SFX en código para no olvidar asignarla en el Inspector
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f; // 2D
    }

    private void Start()
    {
        if (musicSource != null && levelMusic != null)
        {
            musicSource.clip = levelMusic;
            musicSource.loop = true;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
    }

    // ── API pública ──────────────────────────────────────────

    public void PlayPlayerShoot() => Play(playerShootSFX, playerShootVol);
    public void PlayEnemyShoot() => Play(enemyShootSFX, enemyShootVol);
    public void PlayPlayerHurt() => Play(playerHurtSFX, hurtVol);
    public void PlayPlayerDeath() => Play(playerDeathSFX, hurtVol);
    public void PlayEnemyDeath() => Play(enemyDeathSFX, 0.8f);
    public void PlayEnemyHurt() => Play(enemyHurtSFX, 0.8f);
    public void PlayCollectible() => Play(collectibleSFX, collectibleVol);

    public void SetMusicVolume(float v)
    {
        musicVolume = v;
        if (musicSource != null) musicSource.volume = v;
    }

    // ── Interno ──────────────────────────────────────────────

    private void Play(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }
}