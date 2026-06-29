//// TODO: Find usage for script or delete it
//using System;
//using UnityEngine;

//[Serializable]
//public sealed class CheckpointSaveData
//{
//    [Header("Версия")]
//    [Tooltip("Версия формата save. Нужна, если структура данных изменится в следующих уроках.")]
//    public int schemaVersion = 1;

//    public string checkpointId;
//    public string completedSceneName;

//    [Tooltip("Имя следующей gameplay-сцены. Может быть пустым, если завершён последний уровень.")]
//    public string nextSceneName;

//    [Tooltip("Индекс завершённого уровня в LevelSequenceData. -1 означает fallback-сцену без sequence.")]
//    public int completedLevelIndex = -1;

//    [Tooltip("Индекс следующего уровня в LevelSequenceData. -1 означает, что следующего уровня нет.")]
//    public int nextLevelIndex = -1;

//    [Tooltip("Позиция точки сохранения: выход уровня или отдельная safe-point зона.")]
//    public Vector3 checkpointPosition;

//    [Tooltip("true означает сохранение через выход уровня, false — через отдельный checkpoint-trigger.")]
//    public bool savedFromLevelExit;

//    [Header("Player")]
//    [Tooltip("Здоровье игрока на момент безопасного сохранения.")]
//    public float health;

//    [Tooltip("Уровень игрока на момент безопасного сохранения.")]
//    public int playerLevel = 1;

//    [Tooltip("Опыт игрока внутри текущего уровня.")]
//    public float experience;

//    [Tooltip("Индекс активного оружия в WeaponManager.")]
//    public int weaponSlotIndex = -1;
//}