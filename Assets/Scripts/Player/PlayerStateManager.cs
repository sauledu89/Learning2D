using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerStateManager : MonoBehaviour
{
    public enum PlayerState
    {
        Idle,
        Walking,
        Dodging,
        Dead
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Dodge Roll")]
    [SerializeField] private float rollSpeed = 12f;
    [SerializeField] private float rollDuration = 0.35f;
    [SerializeField] private float rollCooldown = 0.6f;
    private float rollTimer;
    private float cooldownTimer;
    private Vector2 rollDirection;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TextMeshProUGUI txtStateDebug;

    [Header("World Wrap")]
    [SerializeField] private WorldBounds2D world;
    [SerializeField] private bool autoFindWorld = true;

    [Header("World Wrap Helper")]
    [SerializeField] private WrapMover2D wrapMover;
    [SerializeField] private CameraFollow cameraScript;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private PlayerState currentState;

    // Los hash optimizan el acceso: comparar enteros es mucho más eficiente que comparar strings
    private static readonly int HashIsMoving = Animator.StringToHash("isMoving");
    private static readonly int HashMoveX = Animator.StringToHash("moveX");
    private static readonly int HashMoveY = Animator.StringToHash("moveY");
    private static readonly int HashDoRoll = Animator.StringToHash("doRoll");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (animator == null) animator = GetComponentInChildren<Animator>(true);
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        if (autoFindWorld && world == null)
            world = WorldBounds2D.Instance;
    }

    private void Start()
    {
        currentState = PlayerState.Idle;
        // [RECOMENDACIÓN] Asegurar que el jugador esté en su Layer correcta al iniciar
        gameObject.layer = LayerMask.NameToLayer("Player");
    }

    private void Update()
    {
        // 1. LEER INPUT
        ReadInput();

        // 2. LÓGICA DE ENFRIAMIENTO (Dodge)
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;

        // 3. MÁQUINA DE ESTADOS
        switch (currentState)
        {
            case PlayerState.Idle: HandleIdle(); break;
            case PlayerState.Walking: HandleWalking(); break;
            case PlayerState.Dodging: HandleDodging(); break;
            case PlayerState.Dead: break;
        }

        if (txtStateDebug != null)
            txtStateDebug.text = $"Player State: {currentState}";
    }

    private void FixedUpdate()
    {
        Vector2 nextPos;
        if (currentState == PlayerState.Dodging)
            nextPos = rb.position + rollDirection * rollSpeed * Time.fixedDeltaTime;
        else
            nextPos = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;

        // Implementación del WrapMover2D para el efecto "Pac-Man"
        if (wrapMover != null)
        {
            if (wrapMover.TryWrap(ref nextPos, out Vector2 wrapOffset))
            {
                // Si hubo un salto de posición, avisamos a la cámara para que no haga el recorrido largo
                if (wrapOffset != Vector2.zero && cameraScript != null)
                {
                    cameraScript.InstantSnap();
                }
            }
        }

        rb.MovePosition(nextPos);
    }

    private void ReadInput()
    {
        if (Keyboard.current == null) return;

        float x = 0, y = 0;
        if (Keyboard.current.aKey.isPressed) x -= 1f;
        if (Keyboard.current.dKey.isPressed) x += 1f;
        if (Keyboard.current.wKey.isPressed) y += 1f;
        if (Keyboard.current.sKey.isPressed) y -= 1f;

        moveInput = new Vector2(x, y).normalized;

        if (Keyboard.current.spaceKey.wasPressedThisFrame && currentState != PlayerState.Dodging && cooldownTimer <= 0 && moveInput.sqrMagnitude > 0.1f)
        {
            ChangeState(PlayerState.Dodging);
        }
    }

    private void HandleIdle()
    {
        UpdateAnimator(Vector2.zero);
        if (moveInput.sqrMagnitude > 0.01f) ChangeState(PlayerState.Walking);
    }

    private void HandleWalking()
    {
        UpdateAnimator(moveInput);
        if (moveInput.sqrMagnitude <= 0.01f) ChangeState(PlayerState.Idle);
    }

    private void HandleDodging()
    {
        rollTimer -= Time.deltaTime;
        if (rollTimer <= 0) ChangeState(PlayerState.Idle);
    }

    private void ChangeState(PlayerState nextState)
    {
        // Salida de estado: Si dejamos de rodar, recuperamos colisiones normales
        if (currentState == PlayerState.Dodging) EndDodgeInvulnerability();

        currentState = nextState;

        // Entrada de estado
        if (currentState == PlayerState.Dodging) StartDodge();
    }

    private void StartDodge()
    {
        rollTimer = rollDuration;
        cooldownTimer = rollCooldown;
        rollDirection = moveInput;
        animator.SetTrigger(HashDoRoll);

        // [NUEVO] Optimización sugerida: Usar una Layer específica para invulnerabilidad
        // en lugar de Ignore Raycast, podrías usar una llamada "PlayerInvulnerable"
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    private void EndDodgeInvulnerability()
    {
        gameObject.layer = LayerMask.NameToLayer("Player");
    }

    // [IMPORTANTE] Este método ahora es llamado por PlayerHealth antes de avisar al GameFlowManager
    public void OnPlayerDeath()
    {
        ChangeState(PlayerState.Dead);
        rb.linearVelocity = Vector2.zero;
        // Al morir, este script se apaga para no procesar más movimientos
        this.enabled = false;
    }

    private void UpdateAnimator(Vector2 input)
    {
        if (animator == null) return;

        bool isMoving = input.sqrMagnitude > 0.01f;
        animator.SetBool(HashIsMoving, isMoving);

        if (isMoving)
        {
            animator.SetFloat(HashMoveX, input.x);
            animator.SetFloat(HashMoveY, input.y);

            if (spriteRenderer != null && Mathf.Abs(input.x) > 0.01f)
                spriteRenderer.flipX = input.x < 0f;
        }

        if (spriteRenderer != null)
            spriteRenderer.transform.localEulerAngles = Vector3.zero;
    }
}