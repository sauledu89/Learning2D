using UnityEngine;
using TMPro;
using System.Collections; // Necesario para la Corrutina

public class PlayerHealth : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHP = 10;
    [SerializeField] private int currentHP;
    public int CurrentHP => currentHP;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI txtHP;

    [Header("Player Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float hurtLockTime = 0.6f;

    private bool isDead;
    private float hurtLockTimer;

    // Hash optimizados
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
        currentHP = maxHP;
        UpdateUI();
        isDead = false;
    }

    private void Update()
    {
        if (hurtLockTimer > 0f)
            hurtLockTimer -= Time.unscaledDeltaTime;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHP -= amount;
        UpdateUI();

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
            return;
        }

        PlayHurtAnimation();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // 1. Notificamos al StateManager para detener el movimiento del jugador
        var stateManager = GetComponent<PlayerStateManager>();
        if (stateManager != null) stateManager.OnPlayerDeath();

        // 2. Animación de muerte
        if (animator != null)
        {
            animator.ResetTrigger(HashHurt);
            animator.ResetTrigger(HashDoRoll);
            animator.SetBool(HashIsDead, true);
        }

        // 3. [IMPORTANTE] Notificamos al GameFlowManager del evento.
        // Él se encargará de mostrar el panel y gestionar el tiempo.
        GameFlowManager.Instance.RequestGameOver();
    }

    private void PlayHurtAnimation()
    {
        if (animator == null || hurtLockTimer > 0f) return;
        hurtLockTimer = hurtLockTime;
        animator.SetTrigger(HashHurt);
    }

    private void UpdateUI()
    {
        if (txtHP != null)
            txtHP.text = $"HP: {currentHP}";
    }
}