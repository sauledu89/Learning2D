using UnityEngine;

public class WorldBounds2D : MonoBehaviour
{
    public static WorldBounds2D Instance { get; private set; }

    [Header("World Bounds (Units)")]
    public float minX = -10f;
    public float maxX = 10f;
    public float minY = -5f;
    public float maxY = 5f;

    [Header("Optional: Auto From SpriteRenderer")]
    public bool autoFromSpriteRenderer = false;
    public SpriteRenderer referenceSprite;

    [Tooltip("Shrinks the bounds inward (safety margin).")]
    public float inset = 0f;

    public float Width => maxX - minX;
    public float Height => maxY - minY;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (autoFromSpriteRenderer && referenceSprite != null)
            AutoSetupFromSprite();
    }

    [ContextMenu("Auto Setup From SpriteRenderer")]
    public void AutoSetupFromSprite()
    {
        Bounds b = referenceSprite.bounds;
        minX = b.min.x + inset;
        maxX = b.max.x - inset;
        minY = b.min.y + inset;
        maxY = b.max.y - inset;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 center = new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, 0f);
        Vector3 size = new Vector3((maxX - minX), (maxY - minY), 0f);

        Gizmos.DrawWireCube(center, size);
    }
}
