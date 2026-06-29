using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameEventRegistry", menuName = "Architecture/Game Event Registry")]
public class GameEventRegistry : ScriptableObject
{
    [Header("Global System Phases")]
    public Action<GameManager.GameState, GameManager.GameState> OnGameStateTransition;
    public Action OnMapGenerated;
    public Action OnSettingsClosed;
    public Action OnRoleGiven;

    [Header("Player Lifecycle & Turns")]
    public Action<int> OnTurnChanged;
    public Action OnDeath;

    [Header("Player Vitals & Progression")]
    public Action<float, float> OnHealthChanged;   // current, max
    public Action<int, int> OnGoldChanged;         // current, changedAmount
    public Action<string, int> OnRoleApplied;       // roleName, roleId
    public Action<int> OnLevelUp;
    public Action<float, float> OnXPChanged;       // current, max

    [Header("Enemy State Triggers")]
    public Action OnEnemyDied;
    public Action OnEnemySpawnTriggered;

    [Header("Combat Assets & RNG")]
    public Action<WeaponBase> OnWeaponChanged;
    public Action<int> OnDiceRolled;      
    public Action<int> OnDiceRollCompleted;        // finalInt
}