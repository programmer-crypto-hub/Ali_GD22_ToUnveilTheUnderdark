using System;
using UnityEngine;

public class PlayerRolesController : MonoBehaviour
{
    [Header("Roles Stats")]
    [Tooltip("ScriptableObject с базовыми параметрами ролей игрока (PlayerRolesData).")]
    public PlayerRoles playerRoles;

    public static PlayerRolesController Instance { get; private set; }

    public Action OnRoleGiven;

    private void Awake()
    {
        if (playerRoles == null)
        {
            Debug.LogError("PlayerRolesController: PlayerRoles не назначены в инспекторе.", this);
        }
    }

    private void OnEnable()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("PlayerRolesController: уже существует другой экземпл€р. Ётот экземпл€р будет отключен.", this);
            enabled = false;
            return;
        }
        Instance = this;
    }

    private void ApplyRole(int role)
    {
        role = UnityEngine.Random.Range(0, Enum.GetValues(typeof(PlayerRoles.RoleType)).Length - 1);
        Debug.Log($"Applying role: {role}");
    }
}
