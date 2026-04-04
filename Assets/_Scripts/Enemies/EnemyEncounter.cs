using UnityEngine;
using System;
using UnityEngine.UIElements;

public class EnemyEncounter : MonoBehaviour
{
    [Header("Encounter Spawns")]
    [Tooltip("Array of Platforms, where Encounters spawn")]
    public GameObject[] spawnPlatforms;

    [Tooltip("Array of Enemy Types")]
    public EnemyData EnemyData;
    public EnemyData[] enemyTypes;
    public Sprite[] enemySprites;

    [Header("UI Settings")]
    [Tooltip("UI Element to display encounter info")]
    public GameObject encounterPanel;

    [Header("Encounter Settings")]
    [Tooltip("Multiple configurations of Enemy Spawning")]
    public bool canSpawnHere = false;
    public bool isEnemySpawned = false;

    public int spawnIndex;
    public int enemyIndex = 0; // Index to select enemy type from EnemyData array
    public int Index; // Index for EnemyHierarchy, to define if an enemy can spawn here (via CanEnemySpawnHere())

    public Action OnEnemySpawnTriggered;

    public void InitEnemySpawn()
    {
        spawnIndex = UnityEngine.Random.Range(0, spawnPlatforms.Length);
        Debug.Log($"EnemyEncounter: InitEnemySpawn called. Enemy spawning at {spawnIndex}");
    }
    public void DefEnemyIndex(EnemyData.EnemyTypeByHealth enemyType)
    {
        if (EnemyData == null)
        {
            Debug.LogError("EnemyData is not assigned in EnemyEncounter.");
            return;
        }
        if (enemyType == EnemyData.EnemyTypeByHealth.Basic)
            Index = 0;
        else if (enemyType == EnemyData.EnemyTypeByHealth.Medium)
            Index = 1;
        else if (enemyType == EnemyData.EnemyTypeByHealth.Boss)
            Index = 2;
    }

    public bool CanEnemySpawnHere()
    {
        if (spawnIndex == spawnPlatforms.Length && Index < 2)
            return false;
        else if (spawnIndex == spawnPlatforms.Length - 1 && Index == 2)
            return false;
        else
            return true;
    }
}
