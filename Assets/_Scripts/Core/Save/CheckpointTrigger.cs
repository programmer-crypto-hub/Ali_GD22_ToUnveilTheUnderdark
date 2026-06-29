// TODO: Find usage for this script (for example, save system).
//using UnityEngine;

//[RequireComponent(typeof(Collider))]
//public class CheckpointTrigger : MonoBehaviour
//{
//    [Header("Слот сохранения")]
//    [Tooltip("Индекс слота сохранения: 0, 1 или 2.")]
//    [SerializeField] private int slotIndex;

//    [Header("Фильтр игрока")]
//    [Tooltip("Tag, который считается игроком для активации checkpoint.")]
//    [SerializeField] private string requiredTag = "Player";

//    [Header("Правило safe-point")]
//    [Tooltip("Если включено, сохранение разрешено только после завершения указанного encounter.")]
//    [SerializeField] private bool requireEncounterCompleted;

//    [Tooltip("Encounter, завершение которого делает checkpoint безопасным. Если поле пустое, проверяется отсутствие активных encounter.")]
//    [SerializeField] private EncounterTrigger requiredEncounter;

//    [Header("Отладка")]
//    [SerializeField] private bool showDebugLogs = true;

//    private void Reset()
//    {
//        Collider checkpointCollider = GetComponent<Collider>();
//        checkpointCollider.isTrigger = true;
//    }


//    private void OnTriggerEnter(Collider other)
//    {
//        if (!IsValidSlotIndex())
//            return;

//        if (!IsPlayerCollider(other))
//            return;

//        if (!CanSaveByEncounterRule())
//            return;

//        if (GameManager.Instance == null)
//        {
//            Debug.LogWarning($"{name}: GameManager.Instance не найден. Checkpoint не может сохранить прогресс.", this);
//            return;
//        }

//        bool saved = GameManager.Instance.TrySaveCheckpointProgress(slotIndex, transform.position);
//        if (showDebugLogs && saved)
//            Debug.Log($"{name}: отдельный checkpoint сохранён в слот {slotIndex}.", this);
//    }


//    private bool IsValidSlotIndex()
//    {
//        if (slotIndex >= 0 && slotIndex < CheckpointSaveSystem.SlotCount)
//            return true;

//        Debug.LogError(
//            $"{name}: неверный slotIndex {slotIndex}. " +
//            $"Укажите значение 0..{CheckpointSaveSystem.SlotCount - 1} в Inspector.",
//            this);
//        return false;
//    }

//    private bool CanSaveByEncounterRule()
//    {
//        if (!requireEncounterCompleted)
//            return true;

//        if (requiredEncounter != null)
//        {
//            if (requiredEncounter.IsEncounterCompleted)
//                return true;

//            if (showDebugLogs)
//                Debug.Log($"{name}: checkpoint ожидает завершения encounter '{requiredEncounter.name}'.", this);

//            return false;
//        }

//        EncounterTrigger[] encounters = FindObjectsByType<EncounterTrigger>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
//        for (int i = 0; i < encounters.Length; i++)
//        {
//            if (encounters[i] != null && encounters[i].IsEncounterRunning)
//            {
//                if (showDebugLogs)
//                    Debug.Log($"{name}: checkpoint пропущен, потому что encounter ещё активен.", this);

//                return false;
//            }
//        }

//        return true;
//    }

//    private bool IsPlayerCollider(Collider other)
//    {
//        if (other == null)
//            return false;

//        if (string.IsNullOrWhiteSpace(requiredTag))
//            return true;

//        if (other.CompareTag(requiredTag))
//            return true;

//        if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag(requiredTag))
//            return true;

//        Transform root = other.transform.root;
//        return root != null && root.CompareTag(requiredTag);
//    }
//}