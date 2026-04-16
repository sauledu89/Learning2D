using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// FSM del enemigo con disparo a distancia.
/// Estados:
///   Patrol  → patrulla entre waypoints hasta detectar al jugador.
///   Chase   → persigue al jugador hasta entrar en rango de disparo.
///   Attack  → mantiene una distancia óptima y dispara con cooldown.
/// </summary>
public class EnemyStateManager : MonoBehaviour
{
    public enum EnemyState { Wander, Chase, Attack }

    // --- REFERENCIAS ---
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private TextMeshProUGUI txtStateDebug;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    // --- RANGOS ---
    [Header("Ranges")]
    [Tooltip("Distancia a la que el enemigo detecta al jugador y empieza a perseguirlo.")]
    [SerializeField] private float detectionRange = 6f;
    [Tooltip("Distancia a la que el enemigo entra en modo disparo.")]
    [SerializeField] private float shootRange = 5f;
    [Tooltip("Distancia mínima que el enemigo intenta mantener con el jugador (retrocede si se acerca más).")]
    [SerializeField] private float keepDistance = 3f;

    // --- MOVIMIENTO ---
    [Header("Movement")]
    [SerializeField] private float wanderSpeed = 2f;
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float retreatSpeed = 2.5f;

    // --- WANDER ---
    [Header("Wander")]
    [Tooltip("Radio alrededor de la posición actual donde se elige el siguiente destino aleatorio.")]
    [SerializeField] private float wanderRadius = 5f;
    [Tooltip("Cuánto tiempo espera el enemigo al llegar a un punto antes de elegir el siguiente.")]
    [SerializeField] private float wanderWaitTime = 1f;

    // --- DISPARO ---
    [Header("Shooting")]
    [Tooltip("Prefab de la bala (Bullet.cs). Se instancia en cada disparo.")]
    [SerializeField] private GameObject bulletPrefab;
    [Tooltip("Punto de origen del disparo (un Transform hijo vacío en la punta del arma).")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireCooldown = 1.5f;
    [SerializeField] private int bulletDamage = 1;
    [SerializeField] private float bulletSpeed = 8f;

    // --- HASHES DE ANIMATOR ---
    private static readonly int HashIsMoving = Animator.StringToHash("isMoving");
    private static readonly int HashMoveX = Animator.StringToHash("moveX");
    private static readonly int HashMoveY = Animator.StringToHash("moveY");

    // --- ESTADO INTERNO ---
    private EnemyState currentState;
    private Vector2 wanderTarget;
    private float wanderWaitTimer;
    private float fireTimer;
    private Rigidbody2D rb;

    // ─────────────────────────────────────────────
    //  CICLO DE VIDA
    // ─────────────────────────────────────────────

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogWarning("[EnemyFSM] Falta Rigidbody2D.");
        if (bulletPrefab == null) Debug.LogWarning("[EnemyFSM] Asigna el prefab de bala en el Inspector.");
        if (firePoint == null) Debug.LogWarning("[EnemyFSM] Asigna el FirePoint en el Inspector.");

        // Buscar al jugador automáticamente si no está asignado
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        PickNewWanderTarget();
        ChangeState(EnemyState.Wander);
    }

    private void Update()
    {
        switch (currentState)
        {
            case EnemyState.Wander: Wander(); break;
            case EnemyState.Chase: Chase(); break;
            case EnemyState.Attack: Attack(); break;
        }

        if (txtStateDebug != null)
            txtStateDebug.text = $"Enemy: {currentState}";
    }

    // ─────────────────────────────────────────────
    //  ESTADOS
    // ─────────────────────────────────────────────

    private void Wander()
    {
        // Transición → Chase: jugador detectado
        if (player != null && DistanceToPlayer() <= detectionRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        // Si estamos esperando entre destinos, contar el timer
        if (wanderWaitTimer > 0f)
        {
            wanderWaitTimer -= Time.deltaTime;
            UpdateVisuals(Vector2.zero, isMoving: false);
            return;
        }

        // Moverse hacia el destino aleatorio actual
        MoveTo(wanderTarget, wanderSpeed);

        // Al llegar, esperar y elegir un nuevo destino
        if (Vector2.Distance(transform.position, wanderTarget) < 0.3f)
        {
            wanderWaitTimer = wanderWaitTime;
            PickNewWanderTarget();
        }
    }

    private void PickNewWanderTarget()
    {
        // Elegir un punto aleatorio dentro del wanderRadius respetando WorldBounds si existe
        Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;
        Vector2 candidate = (Vector2)transform.position + randomOffset;

        if (WorldBounds2D.Instance != null)
        {
            WorldBounds2D wb = WorldBounds2D.Instance;
            candidate.x = Mathf.Clamp(candidate.x, wb.minX + 0.5f, wb.maxX - 0.5f);
            candidate.y = Mathf.Clamp(candidate.y, wb.minY + 0.5f, wb.maxY - 0.5f);
        }

        wanderTarget = candidate;
    }

    private void Chase()
    {
        if (player == null) return;

        float dist = DistanceToPlayer();

        // Transición → Attack (entró en rango de disparo)
        if (dist <= shootRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        // Transición → Wander (jugador escapó)
        if (dist > detectionRange)
        {
            PickNewWanderTarget();
            ChangeState(EnemyState.Wander);
            return;
        }

        MoveTo(player.position, chaseSpeed);
    }

    private void Attack()
    {
        if (player == null) return;

        float dist = DistanceToPlayer();

        // Transición → Chase (jugador escapó del rango)
        if (dist > shootRange * 1.2f)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        // Mantener distancia: retroceder si el jugador está demasiado cerca
        if (dist < keepDistance)
        {
            Vector2 awayDir = ((Vector2)transform.position - (Vector2)player.position).normalized;
            MoveTo((Vector2)transform.position + awayDir, retreatSpeed);
        }
        else
        {
            // Estamos en la distancia óptima: mirar al jugador pero no movernos
            Vector2 lookDir = ((Vector2)player.position - (Vector2)transform.position).normalized;
            UpdateVisuals(lookDir, isMoving: false);
        }

        // Disparo con cooldown
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            Shoot();
            fireTimer = fireCooldown;
        }
    }

    // ─────────────────────────────────────────────
    //  DISPARO
    // ─────────────────────────────────────────────

    private void Shoot()
    {
        if (bulletPrefab == null || player == null) return;

        // Punto de origen: firePoint si existe, si no el propio transform
        Vector3 origin = firePoint != null ? firePoint.position : transform.position;
        Vector2 direction = ((Vector2)player.position - (Vector2)origin).normalized;

        GameObject bulletGO = Instantiate(bulletPrefab, origin, Quaternion.identity);
        Bullet bullet = bulletGO.GetComponent<Bullet>();

        if (bullet != null)
            bullet.SetUp(direction, targetTag: "Player", damage: bulletDamage, speed: bulletSpeed);

        // Animación de disparo: cuando tengas el sprite listo, crea tu propio trigger aquí
    }

    // ─────────────────────────────────────────────
    //  MOVIMIENTO Y VISUALS
    // ─────────────────────────────────────────────

    private void MoveTo(Vector2 targetPos, float speed)
    {
        Vector2 direction = (targetPos - (Vector2)transform.position).normalized;
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        UpdateVisuals(direction, isMoving: true);
    }

    private void UpdateVisuals(Vector2 direction, bool isMoving)
    {
        if (animator == null) return;

        animator.SetBool(HashIsMoving, isMoving);

        if (isMoving)
        {
            animator.SetFloat(HashMoveX, direction.x);
            animator.SetFloat(HashMoveY, direction.y);
        }

        if (spriteRenderer != null)
        {
            // Flip horizontal según la dirección X
            if (Mathf.Abs(direction.x) > 0.1f)
                spriteRenderer.flipX = direction.x < 0f;

            // Asegurar que nunca haya rotación accidental del sprite
            spriteRenderer.transform.localEulerAngles = Vector3.zero;
        }
    }

    // ─────────────────────────────────────────────
    //  UTILIDADES
    // ─────────────────────────────────────────────

    private void ChangeState(EnemyState nextState)
    {
        currentState = nextState;
        Debug.Log($"[Enemy FSM] → {currentState}");
    }

    private float DistanceToPlayer()
    {
        if (player == null) return Mathf.Infinity;
        return Vector2.Distance(transform.position, player.position);
    }

    // Gizmos para visualizar rangos en el Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, keepDistance);
    }
}