using UnityEngine;

/// <summary>
/// Gestiona la vida del enemigo.
/// Al morir, puede soltar un prefab de runa con una probabilidad configurable.
/// Avisa al EnemyStateManager para que deje de procesar lógica.
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHP = 3;
    private int currentHP;

    [Header("Runa Drop")]
    [Tooltip("Prefab de la runa que cae al morir (el mismo CollectibleItem que ya tienes).")]
    [SerializeField] private GameObject runaPrefab;
    [Tooltip("Probabilidad de 0 a 1 de que suelte la runa al morir.")]
    [SerializeField][Range(0f, 1f)] private float dropChance = 0.6f;

    [Header("Feedback Visual (opcional)")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color hurtColor = Color.red;
    [SerializeField] private float hurtFlashDuration = 0.1f;

    private bool isDead = false;
    private Color originalColor;
    private EnemyStateManager stateManager;

    public event System.Action OnDeath;

    private void Awake()
    {
        stateManager = GetComponent<EnemyStateManager>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    private void Start()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHP -= amount;

        if (spriteRenderer != null)
            StartCoroutine(FlashHurt());

        if (currentHP <= 0)
            Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Desactivamos la IA para que no siga procesando
        if (stateManager != null)
            stateManager.enabled = false;

        // Drop de runa con probabilidad
        if (runaPrefab != null && Random.value <= dropChance)
            Instantiate(runaPrefab, transform.position, Quaternion.identity);

        // Avisamos al spawner antes de destruirnos
        OnDeath?.Invoke();

        Destroy(gameObject);
    }

    private System.Collections.IEnumerator FlashHurt()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = hurtColor;

        yield return new WaitForSeconds(hurtFlashDuration);

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }
}