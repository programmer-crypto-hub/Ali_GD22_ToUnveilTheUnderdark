using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class RoomEncounterHandler : NetworkBehaviour
{
    [Header("Encounter Settings")]
    [SerializeField] private NetworkObject enemyPrefab;
    [SerializeField] private int enemyCount = 3;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private EnemyData enemyData; // Scriptable Object with enemy stats]

    [Header("State")]
    [Networked] public NetworkBool EncounterTriggered { get; set; }
    [Networked] public NetworkBool EncounterCleared { get; set; }

    // Use a Ref to track enemies so the Server can check if they are dead
    private List<NetworkObject> spawnedEnemies = new List<NetworkObject>();

    // 1. Detect Player Entry (Standard Unity Trigger)
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only trigger if it's a Player and we are the Server
        if (!HasStateAuthority || EncounterTriggered || EncounterCleared) return;

        if (other.CompareTag("Player"))
        {
            StartEncounter();
        }
    }

    private void StartEncounter()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return;
        }

        EncounterTriggered = true;
        Debug.Log("Encounter Started!");

        // Part A: Spawning the enemies
        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy(enemyPrefab, spawnPoints[i].position);
        }

        // Part B: The "List Cleanup"
        spawnedEnemies.RemoveAll(e => e == null);

        if (spawnedEnemies.Count == 0)
        {
            EncounterCleared = true;
            OnEncounterFinished();
        }
    }

    // THIS IS THE SPAWNING LOGIC
    private void SpawnEnemy(NetworkObject prefab, Vector3 position)
    {
        // 1. The Server creates the object in its own memory.
        // 2. Fusion assigns a 'NetworkID' to this object.
        // 3. Fusion sends a packet to all Clients saying: "Spawn Prefab X at Position Y with ID Z."
        // 4. On the next frame, the enemy exists on EVERYONE'S screen.
        NetworkObject instance = Runner.Spawn(prefab, position, Quaternion.identity);

        // 5. Link the Scriptable Object data to this specific instance
        if (instance.TryGetComponent<EnemyStats>(out var stats))
        {
            stats.Setup(this.enemyData); // Passing the SO data to the Networked script
        }
    }


    public override void FixedUpdateNetwork()
    {
        // 5. MONITOR PROGRESS
        // Only the server checks if the room is cleared
        if (!HasStateAuthority || !EncounterTriggered || EncounterCleared) return;

        // Clean up the list if enemies were despawned (died)
        spawnedEnemies.RemoveAll(e => e == null);

        if (spawnedEnemies.Count == 0)
        {
            EncounterCleared = true;
            Debug.Log("Encounter Cleared!");
            // TODO: Unlock doors or spawn loot here
        }
    }


    private void OnEncounterFinished()
    {
        Debug.Log("Room Cleared! Reward the players.");
        // Logic for opening doors or spawning a chest goes here
    }
}
