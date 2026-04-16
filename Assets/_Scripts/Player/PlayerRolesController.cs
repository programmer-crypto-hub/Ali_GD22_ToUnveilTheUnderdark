using System;
using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRolesController : NetworkBehaviour
{
    [Header("Roles Stats")]
    [Tooltip("ScriptableObject с базовыми параметрами ролей игрока (PlayerRolesData).")]
    [SerializeField]
    public PlayerRoles playerRoles;

    public static PlayerRolesController Instance { get; private set; }

    public Action OnRoleGiven;

    [Header("UI Elements")]
    [Tooltip("Ссылка на UI элемент для отображения текущей роли игрока.")]
    [SerializeField]
    public GameObject rolePanel;
    [SerializeField]
    public Image roleImage;

    public int RoleId { get; private set; }
    public PlayerRoles.RoleType roleName { get; private set; }

    public override void Spawned()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("PlayerRolesController: уже существует другой экземпляр. Этот экземпляр будет отключен.", this);
            enabled = false;
            return;
        }
        Instance = this;
        if (playerRoles == null)
        {
            Debug.LogError("PlayerRolesController: PlayerRoles не назначены в инспекторе.", this);
            return;
        }
        if (Object.HasStateAuthority)
        {
            SpawnItemsForRole(roleName);
        }
        rolePanel.SetActive(false);

        ApplyRole();
    }

    public void SpawnItemsForRole(PlayerRoles.RoleType roleType)
    {
        if (roleType == PlayerRoles.RoleType.Warrior)
        {
            // Логика для спавна оружия и брони
            Debug.Log("Спавн оружия и брони для воина.");
        }
        else if (roleType == PlayerRoles.RoleType.Mage)
        {
            // Логика для спавна магических предметов
            Debug.Log("Спавн магических предметов для мага.");
        }
        else if (roleType == PlayerRoles.RoleType.Medic)
        {
            // Логика для спавна аптечек и медицинского оборудования
            Debug.Log("Спавн аптечек и медицинского оборудования для медика.");
        }
        else if (roleType == PlayerRoles.RoleType.Rich)
        {
            // Логика для спавна ценных предметов или ресурсов
            Debug.Log("Спавн ценных предметов или ресурсов для богатого игрока.");
        }
        else if (roleType == PlayerRoles.RoleType.Runner)
        {
            // Логика для спавна легкой экипировки или ускорителей
            Debug.Log("Спавн легкой экипировки или ускорителей для бегуна.");
        }
        else if (roleType == PlayerRoles.RoleType.Random)
        {
            // Логика для спавна случайных предметов
            Debug.Log("Спавн случайных предметов для игрока с рандомной ролью.");
        }
    }

    public void ApplyRole()
    {
        RoleId = UnityEngine.Random.Range(0, Enum.GetValues(typeof(PlayerRoles.RoleType)).Length + 1);
        Debug.Log($"Applying role: {RoleId}");
        roleName = (PlayerRoles.RoleType)RoleId;
        Debug.Log($"Applying role: {roleName}");
        roleImage.sprite = playerRoles.roleSprite;
    }

    public void DisplayRoleUI()
    {
        rolePanel.SetActive(true);
        new WaitForSeconds(2f); // Задержка для отображения роли (можно настроить по необходимости)
        roleImage.enabled = true;
        // Здесь можно добавить логику для отображения конкретного изображения или текста в зависимости от роли
        // Например:
        // roleImage.sprite = GetRoleSprite(roleName);
    }
    public int ReturnRoleId()
    {
        return RoleId;
    }
    public PlayerRoles.RoleType ReturnRoleName()
    {
        return roleName;
    }
}
