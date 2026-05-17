using UnityEngine;
using System;
using Fusion;
using UnityEngine.UIElements;

public class EnemyEncounter : NetworkBehaviour
{
    [Header("Encounter Spawns")]
    [Tooltip("Array of Platforms, where Encounters spawn")]
    public GameObject[] spawnPlatforms;

    [Tooltip("Array of Enemy Types")]
    public EnemyData EnemyData;
    public EnemyData[] enemyTypes;
    public Sprite[] enemySprites;

    [Header("UI Settings")]
    [Tooltip("UI Element to display enemy info")]
    public GameObject encounterPanel;

    [Header("State")]
    [Networked] public NetworkBool canSpawnHere { get; set; }
    [Networked] public NetworkBool isEnemySpawned { get; set; }
    [Networked] public NetworkBool isEnemyTriggered { get; set; }
    // 0 = Basic, 1 = Medium, 2 = Boss.
    // Used to determine if an enemy can spawn at a given spawn point (via CanEnemySpawnHere())
    [Networked] public int spawnHierarchy { get; set; }

    public int spawnIndex;
    public int enemyIndex = 0; // Index to select enemy type from EnemyData array
    public int Index; // Index for EnemyHierarchy, to define if an enemy can spawn here (via CanEnemySpawnHere())

    public Action OnEnemySpawnTriggered;

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            InitEnemySpawn();
            DefEnemyIndex(EnemyHierarchy.Instance.hierarchyIndex);
            CanEnemySpawnHere();
            SpawnEnemy();
        }
    }
    public void InitEnemySpawn()
    {
        spawnIndex = UnityEngine.Random.Range(0, spawnPlatforms.Length);
        Debug.Log($"EnemyEncounter: InitEnemySpawn called. Enemy spawning at {spawnIndex}");
    }
    public int DefEnemyIndex(int enemyType)
    {
        if (EnemyData == null)
        {
            Debug.LogError("EnemyData is not assigned in EnemyEncounter.");
        }
        if (enemyType == 0)
        {
            EnemyHierarchy.Instance.hierarchyIndex = 0; // Basic enemy is at index 0 in hierarchy
            Index = 0;
            return 0;
        }
        else if (enemyType == 1)
        {
            EnemyHierarchy.Instance.hierarchyIndex = 1; // Medium enemy is at index 1 in hierarchy
            Index = 1;
            return 1;
        }
        else if (enemyType == 2)
        {
            EnemyHierarchy.Instance.hierarchyIndex = 2; // Boss enemy is at index 2 in hierarchy
            Index = 2;
            return 2;
        }
        else
        {
            Debug.LogError("Invalid enemy type provided to DefEnemyIndex.");
            return -1; // Return -1 to indicate an error
        }
    }

    public bool CanEnemySpawnHere()
    {
        if (spawnHierarchy == 0 && Index == 0) // Basic enemy can spawn at Basic spawn point
        {
            canSpawnHere = true;
            return true;
        }
        else if (spawnHierarchy == 1 && Index <= 1) // Medium enemy can spawn at Basic and Medium spawn points
        {
            canSpawnHere = true;
            return true;
        }
        else if (spawnHierarchy == 2 && Index == 2) // Boss enemy can spawn at his own spawn
        {
            canSpawnHere = true;
            return true;
        }
        else
        {
            canSpawnHere = false;
            return false;
        }
    }

    public void SpawnEnemy()
    {
        if (canSpawnHere && !isEnemySpawned)
        {
            GameObject enemyPrefab = EnemyData.prefab; 
            if (enemyPrefab != null)
            {
                Runner.Spawn(enemyPrefab, spawnPlatforms[spawnIndex].transform.position, Quaternion.identity);
                isEnemySpawned = true;
                Debug.Log($"EnemyEncounter: Spawned {EnemyData.enemyName} at {spawnPlatforms[spawnIndex].name}");
                OnEnemySpawnTriggered?.Invoke();
            }
            else
            {
                Debug.LogError("Enemy prefab is not assigned in EnemyData.");
            }
        }
    }

    // TODO: Add UI logic (separate script)
    // To display enemy info (name, health, type)
    // Akin to gizmos, above the enemy sprite
    public void DisplayEnemyData()
    {
        if (encounterPanel != null && EnemyData != null)
        {
            
        }
        else
        {
            Debug.LogError("Encounter panel or EnemyData is not assigned in EnemyEncounter.");
        }
    }
}

/* Usage: Defy enemy type from manual input
 * Used by it's parent class EnemyHierarchy
 */
public class EnemyHierarchy : EnemyEncounter
{
    public static EnemyHierarchy Instance { get; private set; }
    [SerializeField] private EnemyData.EnemyTypeByHealth hierarchyType;
    [SerializeField] public int hierarchyIndex; // 0 = Basic, 1 = Medium, 2 = Boss
}