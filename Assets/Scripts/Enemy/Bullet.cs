using UnityEngine;

/// <summary>
/// Proyectil genérico usado tanto por el jugador como por los enemigos.
/// Se configura al dispararse con SetUp() y viaja en línea recta.
/// Se destruye al impactar con el objetivo correcto o al expirar su vida útil.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    [Header("Configuración base")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private int damage = 1;

    // Tag del GameObject que esta bala puede dañar ("Player" o "Enemy")
    private string targetTag;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        // El collider debe ser trigger
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    /// <summary>
    /// Inicializa la bala con dirección, tag del objetivo y parámetros opcionales.
    /// Llamar inmediatamente después de Instantiate().
    /// </summary>
    public void SetUp(Vector2 direction, string targetTag, int damage = -1, float speed = -1f)
    {
        this.targetTag = targetTag;
        if (damage >= 0) this.damage = damage;
        if (speed >= 0f) this.speed = speed;

        rb.linearVelocity = direction.normalized * this.speed;

        // Rota el sprite para que apunte en la dirección de movimiento
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignoramos colisiones si el tag objetivo no está asignado
        if (string.IsNullOrEmpty(targetTag)) return;

        if (!other.CompareTag(targetTag)) return;

        // Intentamos dañar al objetivo
        if (targetTag == "Player")
        {
            // Si el jugador está en "Ignore Raycast" está en Dodge Roll → invulnerable
            if (other.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast"))
                return;

            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(damage);
        }
        else if (targetTag == "Enemy")
        {
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
                enemyHealth.TakeDamage(damage);
        }

        Destroy(gameObject);
    }

    // Gizmo para ver el tamaño real en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }
}