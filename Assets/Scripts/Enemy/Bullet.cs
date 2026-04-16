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

    [Header("Colisión con entorno")]
    [Tooltip("Layers con las que la bala debe destruirse al impactar (ej: Walls).")]
    [SerializeField] private LayerMask destroyOnLayers;

    private string targetTag;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        GetComponent<Collider2D>().isTrigger = true;
    }

    public void SetUp(Vector2 direction, string targetTag, int damage = -1, float speed = -1f)
    {
        this.targetTag = targetTag;
        if (damage >= 0) this.damage = damage;
        if (speed >= 0f) this.speed = speed;

        rb.linearVelocity = direction.normalized * this.speed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Destruirse al impactar con muros u otras layers del entorno
        if ((destroyOnLayers.value & (1 << other.gameObject.layer)) != 0)
        {
            Destroy(gameObject);
            return;
        }

        if (string.IsNullOrEmpty(targetTag)) return;
        if (!other.CompareTag(targetTag)) return;

        if (targetTag == "Player")
        {
            // Invulnerable durante Dodge Roll (layer Ignore Raycast)
            if (other.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast"))
                return;

            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null) playerHealth.TakeDamage(damage);
        }
        else if (targetTag == "Enemy")
        {
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null) enemyHealth.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}