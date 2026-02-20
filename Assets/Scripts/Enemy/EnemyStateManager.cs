using System.Collections;
using TMPro;
using UnityEngine;

public class EnemyStateManager : MonoBehaviour
{
    public enum EnemyState { Patrol, Chase, Attack }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private TextMeshProUGUI txtStateDebug;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Ranges")]
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float attackRange = 1.5f;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 3.5f;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 1.2f;

    [Header("Attack Settings")]
    [SerializeField] private float dashDistance = 3f; // Qu� tanto se desplaza
    [SerializeField] private float dashDuration = 0.5f; // Cu�nto dura el desplazamiento
    private bool isDashing = false; // Bloqueo para no repetir el dash

    // HASH de la animaci�n de ataque
    private static readonly int HashAttackTrigger = Animator.StringToHash("attackTrigger");

    [Header("Damage")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private int damagePerHit = 1;

    [Header("Detection Settings")]
    [SerializeField] private LayerMask playerLayer; 
    [SerializeField] private float attackCheckRadius = 0.8f; // Tama�o del "radar" de da�o


    // --- HASHES DE ANIMATOR ---
    private static readonly int HashIsMoving = Animator.StringToHash("isMoving");
    private static readonly int HashMoveX = Animator.StringToHash("moveX");
    private static readonly int HashMoveY = Animator.StringToHash("moveY");

    private EnemyState currentState;
    private int currentWaypointIndex;
    private float attackTimer;
    private Rigidbody2D rb; // <<< a�adir

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // <<< obtener Rigidbody2D
        if (rb == null) Debug.LogWarning("Enemy necesita un Rigidbody2D.");

        ChangeState(EnemyState.Patrol);
        if (playerHealth == null && player != null)
            playerHealth = player.GetComponent<PlayerHealth>();

    }

    private void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Chase:
                Chase();
                break;
            case EnemyState.Attack:
                Attack();
                break;
        }

        if (txtStateDebug != null)
            txtStateDebug.text = $"Enemy State: {currentState}";
    }

    private void ChangeState(EnemyState nextState)
    {
        currentState = nextState;
        Debug.Log($"[Enemy FSM] State changed to {currentState}");
    }

    private void Patrol()
    {
        MoveTo(waypoints[currentWaypointIndex], patrolSpeed);

        if (Vector2.Distance(transform.position, player.position) <= detectionRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        if (Vector2.Distance(transform.position, waypoints[currentWaypointIndex].position) < 0.2f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    private void Chase()
    {
        MoveTo(player, chaseSpeed);

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= attackRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        if (distance > detectionRange)
        {
            ChangeState(EnemyState.Patrol);
        }
    }

    private void Attack()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        // Solo iniciamos el Dash si el cooldown termin� y no estamos ya en uno
        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f && !isDashing)
        {
            // 1. Calculamos la direcci�n hacia el jugador
            Vector2 attackDirection = (player.position - transform.position).normalized;

            // 2. Iniciamos la l�gica de desplazamiento y animaci�n
            StartCoroutine(PerformDashAttack(attackDirection));

            attackTimer = attackCooldown;
        }

        // Si no est� haciendo el dash y el jugador se alej� mucho, volver a Chase
        if (!isDashing && distance > attackRange * 1.5f)
        {
            ChangeState(EnemyState.Chase);
        }
    }

    private void MoveTo(Transform target, float speed)
    {
        Vector2 direction = (target.position - transform.position).normalized;
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        UpdateEnemyVisuals(direction);
    }

    private void UpdateEnemyVisuals(Vector2 direction)
    {
        if (animator == null || isDashing) return; // Si est� en pleno Dash, no actualizamos caminar/idle

        bool moving = direction.sqrMagnitude > 0.01f;
        animator.SetBool(HashIsMoving, moving);

        if (moving)
        {
            animator.SetFloat(HashMoveX, direction.x);
            animator.SetFloat(HashMoveY, direction.y);

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = direction.x < 0f; // Ajusta seg�n tu sprite
            }
        }
    

    //  (spriteRenderer != null)
    // {
    spriteRenderer.transform.localEulerAngles = Vector3.zero;

            // Solo hacemos Flip si hay movimiento lateral significativo
            if (moving && Mathf.Abs(direction.x) < 0.1f)
            {
                spriteRenderer.flipX = direction.x > 0f;
            }
    }

    private IEnumerator PerformDashAttack(Vector2 direction)
    {
        isDashing = true;
        if (animator != null) animator.SetTrigger(HashAttackTrigger);

        // Calculamos la fuerza del impacto
        // Usamos velocidad f�sica en lugar de Lerp para que el motor de f�sica trabaje
        float dashForce = dashDistance / dashDuration;
        if (rb != null)
            rb.linearVelocity = direction * dashForce;
        else
            transform.position += (Vector3)(direction * dashForce * Time.deltaTime); // fallback

        // Esperamos a que pase el tiempo del dash
        yield return new WaitForSeconds(dashDuration);

        if (rb != null) rb.linearVelocity = Vector2.zero; // usar velocity, no linearVelocity
        isDashing = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Verificamos si estamos en Dash y si chocamos con el Player
        if (isDashing && ((playerLayer.value & (1 << collision.gameObject.layer)) != 0))
        {
            PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damagePerHit);

                // IMPORTANTE: Detenemos el dash al impactar para que no lo atraviese
                isDashing = false;
                    if (rb != null) rb.linearVelocity = Vector2.zero; // corregido
                Debug.Log("�Impacto f�sico detectado!");
            }
        }
    }

    // Para que puedas ver el radio de ataque en el Editor (color rojo)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackCheckRadius);
    }
}
