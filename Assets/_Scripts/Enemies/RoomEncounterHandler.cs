using Fusion;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.Unicode;

public class RoomEncounterHandler : NetworkBehaviour
{
    [Header("Encounter Settings")]
    [SerializeField] private NetworkObject enemyPrefab;
    [SerializeField] private int enemyCount = 3;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private EnemyData enemyData;

    [SerializeField] private NetworkObject roomDoors;
    [SerializeField] private NetworkObject rewardChestPrefab;
    [SerializeField] private Transform chestSpawnPoint;

    [Header("State")]
    [Networked, OnChangedRender(nameof(OnEncounterStateChanged))]
    public NetworkBool EncounterTriggered { get; set; }

    [Networked, OnChangedRender(nameof(OnEncounterStateChanged))]
    public NetworkBool EncounterCleared { get; set; }

    private List<NetworkObject> spawnedEnemies = new List<NetworkObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority || EncounterTriggered || EncounterCleared) return;

        if (other.CompareTag("Player"))
        {
            StartEncounter();
        }
    }

    private void StartEncounter()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return;
        }

        EncounterTriggered = true;
        Debug.Log("Boss Encounter Started!");

        var enemy = new PlayerRef();
        for (int i = 0; i < enemyCount; i++)
        {
            Transform targetPoint = spawnPoints[i % spawnPoints.Length];

            Runner.Spawn(enemyPrefab, targetPoint.position, transform.rotation, enemy);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || !EncounterTriggered || EncounterCleared) return;

        spawnedEnemies.RemoveAll(e => e == null || !e.IsValid);

        // Если все враги мертвы — комната зачищена!
        if (spawnedEnemies.Count == 0)
        {
            EncounterCleared = true;
            OnEncounterFinished();
        }
    }

    private void OnEncounterFinished()
    {
        Debug.Log("Boss Room Cleared! Rewarding players, opening passage.");

        if (rewardChestPrefab != null && chestSpawnPoint != null)
        {
            Runner.Spawn(rewardChestPrefab, chestSpawnPoint.position, Quaternion.identity);
        }
    }

    private void OnEncounterStateChanged()
    {
        if (roomDoors == null) return;
        if (EncounterTriggered && !EncounterCleared)
        {
            roomDoors.gameObject.SetActive(true);
            Debug.Log($"Gates {roomDoors.name} closed shut!");
        }
        if (EncounterCleared)
        {
            roomDoors.gameObject.SetActive(false);
            Debug.Log($"Gate {roomDoors.name} is open!");
        }
    }
}