using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRolesController : MonoBehaviour
{
    [Header("Roles Stats")]
    [Tooltip("ScriptableObject с базовыми параметрами ролей игрока (PlayerRolesData).")]
    public PlayerRoles playerRoles;

    public static PlayerRolesController Instance { get; private set; }

    public Action OnRoleGiven;

    [Header("UI Elements")]
    [Tooltip("—сылка на UI элемент дл€ отображени€ текущей роли игрока.")]
    public GameObject rolePanel;
    [SerializeField] public Image roleImage;

    public int RoleId { get; private set; }
    public PlayerRoles.RoleType roleName { get; private set; }

    private void Awake()
    {
        if (playerRoles == null)
        {
            Debug.LogError("PlayerRolesController: PlayerRoles не назначены в инспекторе.", this);
        }

        rolePanel.SetActive(false);

        ApplyRole();
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

    public void ApplyRole()
    {
        RoleId = UnityEngine.Random.Range(0, Enum.GetValues(typeof(PlayerRoles.RoleType)).Length + 1);
        Debug.Log($"Applying role: {RoleId}");
        roleName = (PlayerRoles.RoleType)RoleId;
        roleImage.sprite = playerRoles.roleSprites[RoleId];
    }

    public void DisplayRoleUI()
    {
        rolePanel.SetActive(true);
        roleImage.enabled = true;
        // «десь можно добавить логику дл€ отображени€ конкретного изображени€ или текста в зависимости от роли
        // Ќапример:
        // roleImage.sprite = GetRoleSprite(roleName);
    }
}
