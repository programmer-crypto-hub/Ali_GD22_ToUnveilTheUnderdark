using Fusion;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Упрощённый спавнер врагов для базового обучения.
/// Не использует сложный пул и словари — только Instantiate и простой список.
/// </summary>
public class NewEnemySpawnerTest : NetworkBehaviour
{
    [Header("Тип врага")]
    [Tooltip("Данные врага, которого будем спавнить.")]
    public EnemyData enemyData;

    [Header("Точки спавна")]
    [Tooltip("Массив точек, где могут появляться враги.")]
    public Transform[] spawnPoints;

    [Tooltip("Начинать ли спавн автоматически при старте.")]
    public bool spawnOnStart = true;

    [Header("Отладка")]
    [Tooltip("Показывать ли логи спавна в консоли.")]
    public bool showDebugLogs = true;

    private bool isSpawning;

    [SerializeField] private NetworkObject _enemyPrefab;

    //Простой список для отслеживания созданных врагов.
    private readonly List<EnemyBase> activeEnemies = new List<EnemyBase>();

    private void Start()
    {
        if (enemyData == null || enemyData.prefab == null)
        {
            Debug.LogWarning($"{name}: SimpleEnemySpawner — не назначены EnemyData или prefab.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning($"{name}: SimpleEnemySpawner — нет точек спавна.");
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

        if (showDebugLogs)
            Debug.Log($"{name}: спавн врагов остановлен.");
    }

    public EnemyBase SpawnEnemy()
    {
        if (enemyData == null || enemyData.prefab == null)
        {
            Debug.LogWarning($"{name}: SimpleEnemySpawner — нет корректных данных врага.");
            return null;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning($"{name}: SimpleEnemySpawner — нет точек спавна.");
            return null;
        }

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 spawnPos = spawnPoint.position;
        var enemyInstance = Runner.Spawn(_enemyPrefab, spawnPos, Quaternion.identity);

        EnemyStats stats = enemyInstance.GetComponent<EnemyStats>();
        if (stats != null)
        {
            //Инициализируем статы из EnemyData, если это ещё не было сделано.
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