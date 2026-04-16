using System.Collections;
using UnityEngine;

/// <summary>
/// Genera enemigos en los puntos de spawn (los waypoints que antes usaba la patrulla).
/// Respeta el límite máximo de enemigos vivos y el GameFlowManager.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Prefab")]
    [Tooltip("Prefab del enemigo a generar (debe tener EnemyStateManager y EnemyHealth).")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Puntos de Spawn")]
    [Tooltip("Arrastra aquí los Transforms de los waypoints que tenías en la escena.")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Configuración de Spawn")]
    [Tooltip("Cuántos segundos entre cada intento de spawn.")]
    [SerializeField] private float spawnInterval = 4f;
    [Tooltip("Máximo de enemigos vivos al mismo tiempo.")]
    [SerializeField] private int maxEnemiesAlive = 6;
    [Tooltip("Distancia mínima al jugador para que un punto de spawn sea válido.")]
    [SerializeField] private float minSpawnDistanceFromPlayer = 5f;

    private Transform player;
    private int currentEnemyCount = 0;

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        if (enemyPrefab == null)
            Debug.LogError("[EnemySpawner] Asigna el prefab del enemigo en el Inspector.");

        if (spawnPoints == null || spawnPoints.Length == 0)
            Debug.LogWarning("[EnemySpawner] No hay puntos de spawn asignados.");

        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // Solo spawneamos si el juego está en curso
            if (GameFlowManager.Instance != null && !GameFlowManager.Instance.IsGameplay)
                continue;

            if (currentEnemyCount < maxEnemiesAlive)
                TrySpawn();
        }
    }

    private void TrySpawn()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        // Buscar un punto válido (lejos del jugador)
        // Mezclamos el orden para no siempre intentar el mismo primero
        Transform chosenPoint = GetValidSpawnPoint();

        if (chosenPoint == null)
        {
            Debug.Log("[EnemySpawner] No se encontró punto de spawn válido (todos demasiado cerca del jugador).");
            return;
        }

        GameObject enemy = Instantiate(enemyPrefab, chosenPoint.position, Quaternion.identity);
        currentEnemyCount++;

        // Nos suscribimos a la muerte del enemigo para actualizar el contador
        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        if (health != null)
            health.OnDeath += OnEnemyDied;
    }

    private Transform GetValidSpawnPoint()
    {
        // Crear una copia mezclada de los índices para no sesgar al primero
        int[] indices = ShuffledIndices(spawnPoints.Length);

        foreach (int i in indices)
        {
            Transform point = spawnPoints[i];
            if (point == null) continue;

            if (player == null) return point; // Sin jugador, cualquier punto sirve

            float dist = Vector2.Distance(point.position, player.position);
            if (dist >= minSpawnDistanceFromPlayer)
                return point;
        }

        return null;
    }

    private void OnEnemyDied()
    {
        currentEnemyCount = Mathf.Max(0, currentEnemyCount - 1);
    }

    private int[] ShuffledIndices(int count)
    {
        int[] indices = new int[count];
        for (int i = 0; i < count; i++) indices[i] = i;

        // Fisher-Yates shuffle
        for (int i = count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }
        return indices;
    }

    // Gizmo para ver los puntos de spawn en el Editor
    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;
        Gizmos.color = Color.green;
        foreach (Transform t in spawnPoints)
        {
            if (t != null)
            {
                Gizmos.DrawWireSphere(t.position, 0.4f);
                Gizmos.DrawLine(transform.position, t.position);
            }
        }
    }
}