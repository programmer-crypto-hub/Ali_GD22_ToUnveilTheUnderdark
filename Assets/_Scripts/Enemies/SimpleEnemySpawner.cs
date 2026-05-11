using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SimpleEnemySpawner : NetworkBehaviour
{
    [Header("“ип врага")]
    [Tooltip("ƒанные врага, которого будем спавнить.")]
    public EnemyData enemyData;

    [Header("“очки спавна")]
    [Tooltip("ћассив точек, где могут по€вл€тьс€ враги.")]
    public Transform[] spawnPoints;

    [Header("Ќастройки спавна")]
    [Min(0.1f)]
    [Tooltip("»нтервал между спавнами (в секундах).")]
    public float spawnInterval = 5f;

    [Min(0)]
    [Tooltip("ћаксимальное количество врагов одновременно на сцене.")]
    public int maxEnemies = 10;

    [Tooltip("Ќачинать ли спавн автоматически при старте.")]
    public bool spawnOnStart = true;

    [Header("ќтладка")]
    [Tooltip("ѕоказывать ли логи спавна в консоли.")]
    public bool showDebugLogs = true;

    private bool isSpawning;
    private Coroutine spawnCoroutine;

    [SerializeField] private NetworkObject _enemyPrefab;

    //ѕростой список дл€ отслеживани€ созданных врагов.
    private readonly List<EnemyBase> activeEnemies = new List<EnemyBase>();

    private void Start()
    {
        if (enemyData == null || enemyData.prefab == null)
        {
            Debug.LogWarning($"{name}: SimpleEnemySpawner Ч не назначены EnemyData или prefab.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning($"{name}: SimpleEnemySpawner Ч нет точек спавна.");
            return;
        }

        if (spawnOnStart)
        {
            StartSpawning();
        }
    }

    public void StartSpawning()
    {
        if (isSpawning)
        {
            if (showDebugLogs)
                Debug.LogWarning($"{name}: спавн уже запущен.");
            return;
        }

        isSpawning = true;
        spawnCoroutine = StartCoroutine(SpawnCoroutine());

        if (showDebugLogs)
            Debug.Log($"{name}: спавн врагов запущен.");
    }

    public void StopSpawning()
    {
        if (!isSpawning)
        {
            if (showDebugLogs)
                Debug.LogWarning($"{name}: спавн не был запущен.");
            return;
        }

        isSpawning = false;
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        if (showDebugLogs)
            Debug.Log($"{name}: спавн врагов остановлен.");
    }

    private IEnumerator SpawnCoroutine()
    {
        while (isSpawning)
        {
            yield return new WaitForSeconds(spawnInterval);

            CleanupInactiveEnemies();

            if (activeEnemies.Count >= maxEnemies)
            {
                if (showDebugLogs)
                    Debug.Log($"{name}: достигнут лимит врагов. ѕропускаем спавн.");
                continue;
            }

            SpawnEnemy(); 
        }
    }

    public EnemyBase SpawnEnemy()
    {
        if (Runner == null || !Runner.IsRunning) return null;
        if (enemyData == null || enemyData.prefab == null)
        {
            Debug.LogWarning($"{name}: SimpleEnemySpawner Ч нет корректных данных врага.");
            return null;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning($"{name}: SimpleEnemySpawner Ч нет точек спавна.");
            return null;
        }

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 spawnPos = spawnPoint.position;
        var enemyInstance = Runner.Spawn(_enemyPrefab, spawnPos, Quaternion.identity);

        EnemyStats stats = enemyInstance.GetComponent<EnemyStats>();
              if (stats != null)
              {
                    //»нициализируем статы из EnemyData, если это ещЄ не было сделано.
                  stats.Setup(enemyData);
              }

              EnemyBase enemy = enemyInstance.GetComponent<EnemyBase>();
              if (enemy != null)
              {
                  activeEnemies.Add(enemy);
              }

              if (showDebugLogs)
              {
                  Debug.Log($"{name}: создан враг {enemyData.enemyName} в точке {spawnPoint.name}");
              }

              return enemy;
          }

         private void CleanupInactiveEnemies()
         {
            activeEnemies.RemoveAll(e => e == null || !e.gameObject.activeInHierarchy);
         }

    private void OnDestroy()
    {
        StopSpawning();
    }
}