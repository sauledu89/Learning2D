using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Pistola del jugador estilo Enter the Gungeon.
/// - El GameObject de este script (o su padre "Weapon") rota para apuntar al mouse.
/// - Dispara con Click Izquierdo con cooldown entre disparos.
/// - Se desactiva automáticamente durante el Dodge Roll (sin referencias extras).
/// 
/// SETUP en Unity:
///   1. Crea un GameObject hijo del jugador llamado "Weapon".
///   2. Ponle este script.
///   3. Crea un GameObject hijo de "Weapon" llamado "FirePoint", colócalo
///      en la punta del sprite del arma y asígnalo en el Inspector.
///   4. Asigna el mismo prefab de Bullet que usa el enemigo.
/// </summary>
public class WeaponController : MonoBehaviour
{
    [Header("Disparo")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireCooldown = 0.18f;
    [SerializeField] private int bulletDamage = 1;
    [SerializeField] private float bulletSpeed = 16f;

    [Header("Visual del arma")]
    [Tooltip("Pivot cuando el jugador apunta a la derecha (mano derecha).")]
    [SerializeField] private Transform pivotRight;
    [Tooltip("Pivot cuando el jugador apunta a la izquierda (mano izquierda).")]
    [SerializeField] private Transform pivotLeft;

    private float fireTimer;
    private Camera mainCam;
    private Collider2D[] playerColliders;
    private void Awake()
    {
        mainCam = Camera.main;
        if (bulletPrefab == null) Debug.LogWarning("[WeaponController] Asigna el prefab de bala.");
        if (firePoint == null) Debug.LogWarning("[WeaponController] Asigna el FirePoint.");

        // Cacheamos todos los colliders del jugador para ignorarlos al disparar
        playerColliders = GetComponentsInParent<Collider2D>();
    }

    private void Update()
    {
        AimAtMouse();

        if (fireTimer > 0f) fireTimer -= Time.deltaTime;

        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            TryShoot();
    }

    // ─────────────────────────────────────────────
    //  APUNTADO
    // ─────────────────────────────────────────────

    private void AimAtMouse()
    {
        if (mainCam == null) return;

        Vector2 mouseWorld = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = (mouseWorld - (Vector2)transform.position).normalized;

        bool aimingLeft = direction.x < 0f;

        // Movemos el arma al pivot de la mano correcta
        if (aimingLeft && pivotLeft != null)
            transform.localPosition = pivotLeft.localPosition;
        else if (!aimingLeft && pivotRight != null)
            transform.localPosition = pivotRight.localPosition;

        // Rotamos hacia el mouse
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Flip usando escala negativa en Y para que la rotación sea correcta en ambos lados
        transform.localScale = new Vector3(1f, aimingLeft ? -1f : 1f, 1f);
    }

    // ─────────────────────────────────────────────
    //  DISPARO
    // ─────────────────────────────────────────────

    private void TryShoot()
    {
        if (fireTimer > 0f) return;
        if (bulletPrefab == null || firePoint == null) return;

        Vector2 mouseWorld = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = (mouseWorld - (Vector2)firePoint.position).normalized;

        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        if (bullet != null)
            bullet.SetUp(direction, targetTag: "Enemy", damage: bulletDamage, speed: bulletSpeed);

        Collider2D bulletCol = bulletGO.GetComponent<Collider2D>();
        if (bulletCol != null && playerColliders != null)
            foreach (Collider2D pc in playerColliders)
                if (pc != null) Physics2D.IgnoreCollision(bulletCol, pc);

        // ── GAME FEEL ──
        AudioManager.Instance?.PlayPlayerShoot();

        fireTimer = fireCooldown;
    }
}