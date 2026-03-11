using UnityEngine;

/// <summary>
/// Screen/world wrap helper.
/// - Can AUTO-apply wrapping (for simple objects)
/// - Or can be used as a PURE helper: call TryWrap(ref pos, out offset) from another script.
///
/// Important:
/// If a character moves with Rigidbody2D.MovePosition in another script (like PlayerStateManager),
/// DO NOT also auto-apply wrap here. Use helper mode (autoApply = false).
/// </summary>
public class WrapMover2D : MonoBehaviour
{
    [Header("World Reference")]
    [SerializeField] private WorldBounds2D world;
    [SerializeField] private bool autoFindWorld = true;

    [Header("Wrap Settings")]
    [Tooltip("After wrapping, we push slightly inside bounds to avoid edge jitter.")]
    [SerializeField] private float padding = 0.05f;

    [Header("Apply Mode")]
    [Tooltip("If true, this component will apply wrapping automatically each step.")]
    [SerializeField] private bool autoApply = false;

    [Tooltip("If true, wraps using Rigidbody2D.position in FixedUpdate. If false, wraps Transform in LateUpdate.")]
    [SerializeField] private bool useRigidbody2D = true;

    [SerializeField] private Rigidbody2D rb;

    [Header("Debug")]
    [SerializeField] private bool logWrap = false;

    public bool DidWrapThisStep { get; private set; }
    public Vector2 LastWrapOffset { get; private set; }

    private void Awake()
    {
        if (autoFindWorld && world == null)
            world = WorldBounds2D.Instance;

        if (useRigidbody2D && rb == null)
            rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (!autoApply) return;
        if (!useRigidbody2D) return;
        if (world == null || rb == null) return;

        Vector2 p = rb.position;
        if (TryWrap(ref p, out Vector2 offset))
        {
            rb.position = p;
            if (logWrap) Debug.Log($"[WrapMover2D] {name} wrapped (RB) offset={offset}");
        }
    }

    private void LateUpdate()
    {
        if (!autoApply) return;
        if (useRigidbody2D) return;
        if (world == null) return;

        Vector2 p = transform.position;
        if (TryWrap(ref p, out Vector2 offset))
        {
            transform.position = new Vector3(p.x, p.y, transform.position.z);
            if (logWrap) Debug.Log($"[WrapMover2D] {name} wrapped (Transform) offset={offset}");
        }
    }

    /// <summary>
    /// Helper function: modifies 'pos' if it is outside bounds.
    /// Returns true if wrapping occurred. Also outputs the offset applied.
    /// </summary>
    // ... (Código del PDF WrapMover2D)
    public bool TryWrap(ref Vector2 pos, out Vector2 offset)
    {
        offset = Vector2.zero;
        if (world == null) return false;

        Vector2 newPos = pos;
        // Lógica de teletransporte
        if (pos.x > world.maxX) newPos.x = world.minX + padding;
        else if (pos.x < world.minX) newPos.x = world.maxX - padding;

        if (pos.y > world.maxY) newPos.y = world.minY + padding;
        else if (pos.y < world.minY) newPos.y = world.maxY - padding;

        offset = newPos - pos;
        if (offset != Vector2.zero)
        {
            pos = newPos;
            return true;
        }
        return false;
    }

    // Optional getters (useful for other scripts)
    public WorldBounds2D World => world;
    public float Padding => padding;
}
