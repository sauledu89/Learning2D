using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerStateManager : MonoBehaviour
{
    public enum PlayerState { Idle, Walking, Dodging, Dead }

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

    [Header("Debug")]
    [SerializeField] private bool logWarnings = true;
    [SerializeField] private bool logMovement = false;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private PlayerState currentState;

    private static readonly int HashIsMoving = Animator.StringToHash("isMoving");
    private static readonly int HashMoveX = Animator.StringToHash("moveX");
    private static readonly int HashMoveY = Animator.StringToHash("moveY");
    private static readonly int HashDoRoll = Animator.StringToHash("doRoll"); // Trigger para la animación

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (animator == null) animator = GetComponentInChildren<Animator>(true);
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
    }

    private void Start()
    {
        currentState = PlayerState.Idle;
        gameObject.layer = LayerMask.NameToLayer("Player");
    }

    private void Update()
    {
        // 1. LEER INPUT SIEMPRE (Global)
        ReadInput();

        // 2. LÓGICA DE ENFRIAMIENTO
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;

        // 3. MÁQUINA DE ESTADOS
        switch (currentState)
        {
            case PlayerState.Idle:
                HandleIdle();
                break;
            case PlayerState.Walking:
                HandleWalking();
                break;
            case PlayerState.Dodging:
                HandleDodging();
                break;
        }
    }

    private void FixedUpdate()
    {
        // Solo aplicamos movimiento físico aquí
        if (currentState == PlayerState.Dodging)
        {
            rb.MovePosition(rb.position + rollDirection * rollSpeed * Time.fixedDeltaTime);
        }
        else
        {
            rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
        }
    }

    private void ReadInput()
    {
        if (Keyboard.current == null) return;

        // Movimiento
        float x = 0, y = 0;
        if (Keyboard.current.aKey.isPressed) x -= 1f;
        if (Keyboard.current.dKey.isPressed) x += 1f;
        if (Keyboard.current.wKey.isPressed) y += 1f;
        if (Keyboard.current.sKey.isPressed) y -= 1f;

        moveInput = new Vector2(x, y).normalized;

        // Intento de Dodge (Solo si se mueve y no está ya haciendo dodge)
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
        if (rollTimer <= 0)
        {
            ChangeState(PlayerState.Idle);
        }
    }

    private void ChangeState(PlayerState nextState)
    {
        // --- Lógica de Salida de Estado ---
        if (currentState == PlayerState.Dodging) EndDodgeInvulnerability();

        currentState = nextState;

        // --- Lógica de Entrada de Estado ---
        switch (currentState)
        {
            case PlayerState.Dodging:
                StartDodge();
                break;
        }
    }

    private void StartDodge()
    {
        rollTimer = rollDuration;
        cooldownTimer = rollCooldown;
        rollDirection = moveInput; // Fijamos la dirección al inicio del dodge
        animator.SetTrigger(HashDoRoll);

        // Simular invulnerabilidad (cambiar layer para ignorar colisiones con balas)
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    private void EndDodgeInvulnerability()
    {
        // Asegúrate de que "Player" sea el nombre exacto de tu Layer en Unity
        gameObject.layer = LayerMask.NameToLayer("Player");
    }

    private void UpdateAnimator(Vector2 input)
    {
        if (animator == null) return;

        bool isMoving = input.sqrMagnitude > 0.01f;
        animator.SetBool(HashIsMoving, isMoving);

        // Solo actualizamos dirección si nos movemos para que el Idle mantenga la última cara
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