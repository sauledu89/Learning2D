using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHP = 6;
    [SerializeField] private int currentHP;
    public int CurrentHP => currentHP;

    [Header("UI")]
    [SerializeField] private HeartHealthUI heartUI;

    [Header("Player Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float hurtLockTime = 0.6f;

    private bool isDead;
    private float hurtLockTimer;

    // Hashes optimizados
    private static readonly int HashDoRoll = Animator.StringToHash("doRoll");
    private static readonly int HashHurt = Animator.StringToHash("hurt");
    private static readonly int HashIsDead = Animator.StringToHash("isDead");

    public bool IsDead => isDead;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);
    }

    private void Start()
    {
        isDead = false;
        currentHP = maxHP;
        UpdateUI();
    }

    private void Update()
    {
        if (hurtLockTimer > 0f)
            hurtLockTimer -= Time.unscaledDeltaTime;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHP = Mathf.Max(0, currentHP - amount);
        UpdateUI();

        if (currentHP <= 0)
        {
            Die();
            return;
        }

        // ── GAME FEEL ──
        CameraShake.Instance?.Shake();
        AudioManager.Instance?.PlayPlayerHurt();

        PlayHurtAnimation();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // 1. Detener movimiento
        var stateManager = GetComponent<PlayerStateManager>();
        if (stateManager != null) stateManager.OnPlayerDeath();

        // ── NUEVO: deshabilitar arma ──
        var weapon = GetComponentInChildren<WeaponController>();
        if (weapon != null) weapon.enabled = false;

        // 2. Delegar a la secuencia cinemática
        if (SniperDeathSequence.Instance != null)
        {
            SniperDeathSequence.Instance.Play(
                player: transform,
                onAnimTrigger: TriggerDeathAnimation,
                onComplete: () => GameFlowManager.Instance?.RequestGameOver()
            );
        }
        else
        {
            TriggerDeathAnimation();
            GameFlowManager.Instance?.RequestGameOver();
        }
    }

    // Extraemos esto a un método separado para llamarlo desde la secuencia
    private void TriggerDeathAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("isMoving", false);
            animator.SetFloat("moveX", 0f);
            animator.SetFloat("moveY", 0f);
            animator.ResetTrigger("hurt");
            animator.ResetTrigger("doRoll");
            animator.SetTrigger("isDead");   // ← Trigger en lugar de SetBool
        }
        AudioManager.Instance?.PlayPlayerDeath();
    }

    private void PlayHurtAnimation()
    {
        if (animator == null || hurtLockTimer > 0f) return;
        hurtLockTimer = hurtLockTime;
        animator.SetTrigger(HashHurt);
    }

    private void UpdateUI()
    {
        if (heartUI != null)
            heartUI.UpdateHearts(currentHP);
    }
}