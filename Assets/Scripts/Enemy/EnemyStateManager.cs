using System.Collections;
using UnityEngine;

public class EnemyStateManager : MonoBehaviour
{
    public enum EnemyState { Wander, Chase, Attack }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Ranges")]
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float shootRange = 5f;
    [SerializeField] private float keepDistance = 3f;

    [Header("Movement")]
    [SerializeField] private float wanderSpeed = 2f;
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float retreatSpeed = 2.5f;

    [Header("Wander")]
    [SerializeField] private float wanderRadius = 5f;
    [SerializeField] private float wanderWaitTime = 1f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireCooldown = 1.5f;
    [SerializeField] private int bulletDamage = 1;
    [SerializeField] private float bulletSpeed = 8f;

    private static readonly int HashIsMoving = Animator.StringToHash("isMoving");
    private static readonly int HashMoveX = Animator.StringToHash("moveX");
    private static readonly int HashMoveY = Animator.StringToHash("moveY");

    private EnemyState currentState;
    private Vector2 wanderTarget;
    private float wanderWaitTimer;
    private float fireTimer;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogWarning("[EnemyFSM] Falta Rigidbody2D.");
        if (bulletPrefab == null) Debug.LogWarning("[EnemyFSM] Asigna el prefab de bala en el Inspector.");
        if (firePoint == null) Debug.LogWarning("[EnemyFSM] Asigna el FirePoint en el Inspector.");

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
        // Dejar de procesar si el jugador está muerto
        if (player != null)
        {
            var ph = player.GetComponent<PlayerHealth>();
            if (ph != null && ph.IsDead)
            {
                UpdateVisuals(Vector2.zero, isMoving: false);
                return;
            }
        }

        switch (currentState)
        {
            case EnemyState.Wander: Wander(); break;
            case EnemyState.Chase: Chase(); break;
            case EnemyState.Attack: Attack(); break;
        }
    }

    private void Wander()
    {
        if (player != null && DistanceToPlayer() <= detectionRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        if (wanderWaitTimer > 0f)
        {
            wanderWaitTimer -= Time.deltaTime;
            UpdateVisuals(Vector2.zero, isMoving: false);
            return;
        }

        MoveTo(wanderTarget, wanderSpeed);

        if (Vector2.Distance(transform.position, wanderTarget) < 0.3f)
        {
            wanderWaitTimer = wanderWaitTime;
            PickNewWanderTarget();
        }
    }

    private void PickNewWanderTarget()
    {
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

        if (dist <= shootRange) { ChangeState(EnemyState.Attack); return; }
        if (dist > detectionRange) { PickNewWanderTarget(); ChangeState(EnemyState.Wander); return; }

        MoveTo(player.position, chaseSpeed);
    }

    private void Attack()
    {
        if (player == null) return;
        float dist = DistanceToPlayer();

        if (dist > shootRange * 1.2f) { ChangeState(EnemyState.Chase); return; }

        if (dist < keepDistance)
        {
            Vector2 awayDir = ((Vector2)transform.position - (Vector2)player.position).normalized;
            MoveTo((Vector2)transform.position + awayDir, retreatSpeed);
        }
        else
        {
            Vector2 lookDir = ((Vector2)player.position - (Vector2)transform.position).normalized;
            UpdateVisuals(lookDir, isMoving: false);
        }

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f) { Shoot(); fireTimer = fireCooldown; }
    }

    private void Shoot()
    {
        if (bulletPrefab == null || player == null) return;

        Vector3 origin = firePoint != null ? firePoint.position : transform.position;
        Vector2 direction = ((Vector2)player.position - (Vector2)origin).normalized;

        GameObject bulletGO = Instantiate(bulletPrefab, origin, Quaternion.identity);
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        if (bullet != null)
            bullet.SetUp(direction, targetTag: "Player", damage: bulletDamage, speed: bulletSpeed);

        // ── GAME FEEL ──
        AudioManager.Instance?.PlayEnemyShoot();
    }

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
            if (Mathf.Abs(direction.x) > 0.1f)
                spriteRenderer.flipX = direction.x < 0f;
            spriteRenderer.transform.localEulerAngles = Vector3.zero;
        }
    }

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