using UnityEngine;

public class MinimapFollow2D : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Movement")]
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Bounds (Optional)")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private Vector2 minBounds;
    [SerializeField] private Vector2 maxBounds;

    [Header("Start")]
    [SerializeField] private bool snapOnStart = true;

    private bool isFrozen = false;

    private void Start()
    {
        // Si no asignaste el target manualmente, intenta buscar al jugador por Tag
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }

        if (snapOnStart && target != null)
        {
            transform.position = GetDesiredPosition();
        }
    }

    private void LateUpdate()
    {
        // Si no hay objetivo, el script está pausado o el tiempo está detenido, no hacemos nada
        if (target == null || isFrozen || Time.timeScale == 0) return;

        Vector3 desiredPosition = GetDesiredPosition();

        // Usamos Lerp para un seguimiento suave
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );
    }

    private Vector3 GetDesiredPosition()
    {
        Vector3 desired = target.position + offset;

        if (useBounds)
        {
            desired.x = Mathf.Clamp(desired.x, minBounds.x, maxBounds.x);
            desired.y = Mathf.Clamp(desired.y, minBounds.y, maxBounds.y);
        }

        // Mantenemos la Z del offset para que la cámara no se pegue al suelo 2D
        desired.z = offset.z;

        return desired;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // Este método lo llamará el GameFlowManager al morir o pausar
    public void SetFrozen(bool frozen)
    {
        isFrozen = frozen;
    }
}