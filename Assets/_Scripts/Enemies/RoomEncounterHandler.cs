using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class RoomEncounterHandler : NetworkBehaviour
{
    [Header("Encounter Settings")]
    [SerializeField] private NetworkObject enemyPrefab;
    [SerializeField] private int enemyCount = 1; 
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private EnemyData enemyData;

    [Header("Presentation Visuals")]
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
        //if (!HasStateAuthority) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("[SERVER] Player entered the boss room trigger. Starting encounter...");
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
        Debug.Log("[SERVER] Boss Encounter Started! Монстры призываются на поле.");

        spawnedEnemies.Clear();

        for (int i = 0; i < enemyCount; i++)
        {
            Transform targetPoint = spawnPoints[i % spawnPoints.Length];

            Vector3 transRot = new Vector3(-90, 0, 0);
            NetworkObject instance = Runner.Spawn(enemyPrefab, targetPoint.position, Quaternion.Euler(transRot), Runner.LocalPlayer);

            if (instance != null)
            {
                spawnedEnemies.Add(instance);

                if (instance.TryGetComponent<EnemyBase>(out var baseEnemy))
                {
                    // Если у вашего EnemyBase есть метод инициализации, раскомментируйте:
                    baseEnemy.CurrentHP = enemyData != null ? enemyData.maxHealth : 100f;
                }
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Проверку живых врагов делает только Сервер (Хост)
        if (!HasStateAuthority || !EncounterTriggered || EncounterCleared) return;

        // Удаляем из списка тех, кто умер и деспавнился (стал null или потерял валидность)
        spawnedEnemies.RemoveAll(e => e == null || !e.IsValid);

        // Комната зачищена ТОЛЬКО тогда, когда реально все заспавненные враги исчезли!
        if (spawnedEnemies.Count == 0)
        {
            EncounterCleared = true;
            OnEncounterFinished();
        }
    }

    private void OnEncounterFinished()
    {
        Debug.Log("[SERVER] Boss Room Cleared! Награда создана.");

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
