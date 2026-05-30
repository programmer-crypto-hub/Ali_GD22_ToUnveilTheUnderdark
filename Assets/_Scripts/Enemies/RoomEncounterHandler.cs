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
            Debug.Log("Encounter cleared!");
        }
    }


    private void OnEncounterFinished()
    {
        Debug.LogWarning($"[CRITICAL DIAGNOSTIC] OnEncounterFinished called! Time: {Time.time}s | EncounterTriggered: {EncounterTriggered} | EncounterCleared: {EncounterCleared}");

        // 1. ЗАПУСК АНИМАЦИИ ОТКРЫТИЯ СУНДУКА
        if (staticChestAnimator != null)
        {
            staticChestAnimator.SetTrigger("open_trig");
            Debug.LogWarning("[CHEST VISUAL] Called staticChestAnimator.SetTrigger('open_trig')");
        }

        enemyPanel.SetActive(false);
        // Изменение сетевых данных инвентаря Fusion считает строго Хост/Сервер
        if (Runner == null || !Runner.IsServer) return;

        // 2. ДИНАМИЧЕСКИЙ ПЕРЕХВАТ МАГАЗИНА И ИГРОКА
        var shopManager = FindFirstObjectByType<ShopUIManager>();
        var playerNetObj = FindFirstObjectByType<PlayerStats>();

        if (shopManager == null || shopManager.allItems == null || shopManager.allItems.Count == 0)
        {
            Debug.LogError("[CHEST BRIDGE ERROR] Не удалось динамически найти ShopUIManager или массив All Items на сцене пуст!");
            return;
        }

        if (playerNetObj == null || playerNetObj.InventoryItemIDs.Length == 0)
        {
            Debug.LogError("[CHEST BRIDGE ERROR] Не удалось найти PlayerStats на сцене для начисления награды!");
            return;
        }

        // Берем случайный Scriptable Object напрямую из вашего массива All Items на экране инспектора!
        ShopItem luckyDrop = shopManager.allItems[UnityEngine.Random.Range(0, shopManager.allItems.Count)];
        int itemID = luckyDrop.itemID; // Вытаскиваем его уникальный ID (например, 17 для Экскалибура или 22 для Бомбы)

        int dropQuantity = 1; // По умолчанию выпадает 1 предмет

        // Если это финальный боссфайт и выпала бомба (ID 22) — выдаем сразу 5 штук!
        if (itemID == 22)
        {
            dropQuantity = 5;
            Debug.LogWarning($"[CHEST LOOT] ДЬЯВОЛЬСКИЙ ДРОП! Выпало супер-количество: {dropQuantity} Бомб!");
        }
        // Если выпало редкое зелье (3x ID) или крутое оружие — можно настроить выдачу 2-3 штук для эпичности
        else if (itemID >= 31 && itemID <= 34)
        {
            dropQuantity = 2; // Сразу 2 зелья лечения за победу над боссом
        }

        // 5. ПООЧЕРЕДНОЕ ЗАПОЛНЕНИЕ СЕТЕВЫХ ЯЧЕЕК ХОТБАРА И ИНВЕНТАРЯ
        // Запускаем цикл, который пропишет ID предмета в свободные слоты столько раз, сколько штук выпало!
        for (int q = 0; q < dropQuantity; q++)
        {
            int targetSlotIndex = -1;

            // Сначала ищем место в нижнем хотбаре (индексы 20-29), чтобы иконки сразу вспыхнули внизу экрана на видео!
            for (int i = 20; i < 30; i++)
            {
                if (playerNetObj.InventoryItemIDs[i] == 0) { targetSlotIndex = i; break; }
            }

            // Если хотбар забит, пишем в верхний инвентарь (индексы 0-19)
            if (targetSlotIndex == -1)
            {
                for (int i = 0; i < 20; i++)
                {
                    if (playerNetObj.InventoryItemIDs[i] == 0) { targetSlotIndex = i; break; }
                }
            }

            // Если нашли свободную ячейку — записываем ID в сеть Photon Fusion!
            if (targetSlotIndex != -1)
            {
                playerNetObj.InventoryItemIDs.Set(targetSlotIndex, itemID);
                Debug.LogWarning($"[CHEST] Штука №{q + 1} предмета {luckyDrop.itemName} (ID: {itemID}) записана в сетевой слот {targetSlotIndex}!");
            }
            else
            {
                Debug.LogError($"[CHEST] Слот не найден! Инвентарь переполнен на штуке №{q + 1}.");
                break; // Останавливаем цикл, если сумки трещат по швам
            }
        }

        // 6. МГНОВЕННО ОБНОВЛЯЕМ ЭКРАН ИНТЕРФЕЙСА CANVAS
        RPC_RefreshLocalInventoryUI();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_RefreshLocalInventoryUI()
    {
        var inventoryUI = FindFirstObjectByType<InventoryUI>();
        if (inventoryUI != null)
        {
            // Вызываем двойной триггер смены визуального состояния, 
            // чтобы ваш метод RefreshUI() внутри заставил новые иконки вспыхнуть в хотбаре на записи видео!
            inventoryUI.ToggleInventoryExpansion();
            inventoryUI.ToggleInventoryExpansion();
            Debug.LogWarning("[UI SYNCHRONIZED] Интерфейс инвентаря принудительно перерисован из сетевого массива!");
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
