using UnityEngine;

[CreateAssetMenu(fileName = "New Player Roles", menuName = "Game Data/Player Roles", order = 0)]
public class PlayerRoles : ScriptableObject
{
    public enum RoleType
    {
        [Tooltip("Warrior - Player Given Better Combat Tools and Higher Damage")]
        Warrior = 0,
        [Tooltip("Mage - Player Given Special Spells and Magic")]
        Mage = 1,
        [Tooltip("Medic - Player Given Ability to Heal and Revive")]
        Medic = 2,
        [Tooltip("Rich - Player Given lots of Money at Start")]
        Rich = 3,
        [Tooltip("Runner - Player Given Faster Speed")]
        Runner = 4,
        [Tooltip("Random - Player Given Random Abilities and Inventory")]
        Random = 5
    }

    [Header("Role Settings")]
    public RoleType roleType;

    [Header("Role Sprites")]
    [Tooltip("Sprites")]
    public Sprite roleSprite;
}
