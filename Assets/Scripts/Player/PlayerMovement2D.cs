using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement2D : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    // Called by the Input System when movement input is performed
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;     // Disable gravity for 2D movement
        rb.freezeRotation = true; // Prevent rotation
    }

    //  Update is for decisions 
    private void Update()
    {
        if (Keyboard.current == null)
        {
            moveInput = Vector2.zero;
            return;
        }

        float x = 0f;
        float y = 0f;

        if (Keyboard.current.wKey.isPressed) y += 1f;
       
        if (Keyboard.current.sKey.isPressed) y -= 1f;

        if (Keyboard.current.aKey.isPressed) x -= 1f;

        if (Keyboard.current.dKey.isPressed) x += 1f;
    
        moveInput = new Vector2(x, y).normalized;
    }

    // fixedUpdate is for physics
    private void FixedUpdate()
    {

        Vector2 nextPos = rb.position + moveInput * moveSpeed * Time.deltaTime;
        rb.MovePosition(nextPos);

        /*
        Vector2 movement = moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);
        */
        }

}
