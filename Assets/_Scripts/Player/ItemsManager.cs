using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the player's inventory of items (weapons, tools, etc.).
/// manages the list of available items and the currently equipped item.
/// lists of items are defined in the inspector as ScriptableObjects (WeaponBase).
/// - switching between items using buttons 1 (back) and 2 (forward) via InputManager.
/// </summary>
public class ItemsManager : MonoBehaviour
{
    [Header("Player Weapons")]
    [SerializeField]
    [Tooltip("All items which can be obtained")]
    private List<WeaponBase>[] itemInstances;

    private WeaponBase currentItem;
}
