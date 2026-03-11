using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CollectibleItem : MonoBehaviour
{
    [Header("Reward")]
    [SerializeField] private int value = 10;

    [Header("References")]
    [SerializeField] private CollectibleGoalManager goalManager;

    [Header("Behaviour")]
    [SerializeField] private bool disableInsteadOfDestroy = true;

    private bool alreadyCollected = false;

    private void Reset()
    {
        Collider2D c = GetComponent<Collider2D>();
        c.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (alreadyCollected) return;
        if (!other.CompareTag("Player")) return;

        alreadyCollected = true;

        if (goalManager == null)
            goalManager = FindFirstObjectByType<CollectibleGoalManager>();

        if (goalManager != null)
            goalManager.RegisterCollectible(value);

        if (disableInsteadOfDestroy) gameObject.SetActive(false);
        else Destroy(gameObject);
    }
}
