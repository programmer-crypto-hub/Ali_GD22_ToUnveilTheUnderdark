using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class RoomEncounterHandler : NetworkBehaviour
{
    public static RoomEncounterHandler Instance { get; private set; }
    [Header("Encounter Settings")]
    [SerializeField] private NetworkObject enemyPrefab;
    [SerializeField] private int enemyCount = 1; 
    [SerializeField] public Transform[] spawnPoints;
    [SerializeField] private EnemyData enemyData;

    [Header("Presentation Visuals")]
    [SerializeField] private NetworkObject roomDoors;
    [SerializeField] private NetworkObject rewardChestPrefab;
    [SerializeField] private Transform chestSpawnPoint;

    [Header("Cinematic Presentation")]
    [SerializeField] private Animator staticChestAnimator;
    [SerializeField] private GameObject enemyPanel;

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
        Debug.Log("Boss Encounter Started!");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RequestStateChange(GameManager.GameState.Combat);
        }
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
                    baseEnemy.CurrentHP = enemyData != null ? enemyData.maxHealth : 100f;
                }
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || !EncounterTriggered || EncounterCleared) return;

        spawnedEnemies.RemoveAll(e => e == null || !e.IsValid);

        if (spawnedEnemies.Count == 0)
        {
            EncounterCleared = true;
            OnEncounterFinished();
            Debug.Log("Encounter cleared!");
        }
    }


    private void OnEncounterFinished()
    {
        Debug.LogWarning($"OnEncounterFinished called! Time: {Time.time}s | EncounterTriggered: {EncounterTriggered} | EncounterCleared: {EncounterCleared}");

        if (staticChestAnimator != null)
        {
            staticChestAnimator.SetTrigger("open_trig");
            Debug.LogWarning("[CHEST VISUAL] Called staticChestAnimator.SetTrigger('open_trig')");
        }

        enemyPanel.SetActive(false);
        if (Runner == null || !Runner.IsServer) return;

        var shopManager = FindFirstObjectByType<ShopUIManager>();
        var playerNetObj = FindFirstObjectByType<PlayerStats>();

        if (shopManager == null || shopManager.allItems == null || shopManager.allItems.Count == 0)
        {
            Debug.LogError("Couldn't find any shop items!");
            return;
        }

        if (playerNetObj == null || playerNetObj.InventoryItemIDs.Length == 0)
        {
            Debug.LogError("Couldn't find PlayerStats on the scene for award distribution!");
            return;
        }

        ShopItem luckyDrop = shopManager.allItems[UnityEngine.Random.Range(0, shopManager.allItems.Count)];
        int itemID = luckyDrop.itemID; 

        int dropQuantity = 1; 

        if (itemID == 22)
        {
            dropQuantity = 5;
        }
        else if (itemID >= 31 && itemID <= 34)
        {
            dropQuantity = 2;
        }

        for (int q = 0; q < dropQuantity; q++)
        {
            int targetSlotIndex = -1;

            for (int i = 20; i < 30; i++)
            {
                if (playerNetObj.InventoryItemIDs[i] == 0) { targetSlotIndex = i; break; }
            }

            if (targetSlotIndex == -1)
            {
                for (int i = 0; i < 20; i++)
                {
                    if (playerNetObj.InventoryItemIDs[i] == 0) { targetSlotIndex = i; break; }
                }
            }

            if (targetSlotIndex != -1)
            {
                playerNetObj.InventoryItemIDs.Set(targetSlotIndex, itemID);
                Debug.Log($"Item {q + 1} of name {luckyDrop.itemName} with ID: {itemID} was assigned to slot {targetSlotIndex}!");
            }
            else
            {
                Debug.LogError($"Slot wasn't found for item {q + 1}.");
                break; 
            }
        }

        RPC_RefreshLocalInventoryUI();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_RefreshLocalInventoryUI()
    {
        var inventoryUI = FindFirstObjectByType<InventoryUI>();
        if (inventoryUI != null)
        {
            inventoryUI.ToggleInventoryExpansion();
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
