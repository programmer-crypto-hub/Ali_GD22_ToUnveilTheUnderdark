// TODO: Fix this script, find usage or delete.
using System;
using Fusion;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerRolesController : NetworkBehaviour
{
    [SerializeField]
    public PlayerRoles playerRoles;

    public static PlayerRolesController Instance { get; private set; }

    public GameObject rolePanel;
    public Image roleImage;
    [Networked]
    public int RoleId { get; set; }
    [Networked]
    public PlayerRoles.RoleType roleName { get; set; }

    public override void Spawned()
    {
        if (Instance != null && Instance != this)
        {
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
        StartCoroutine(DisplayRoleUICoroutine());
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

    public IEnumerator DisplayRoleUICoroutine()
    {
        rolePanel.SetActive(true);
        yield return new WaitForSeconds(2f); // Задержка для отображения роли (можно настроить по необходимости)
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
