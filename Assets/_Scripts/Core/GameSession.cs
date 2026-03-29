using UnityEngine;
using UnityEngine.UI;
using System;
//using UnityEngine.UIElements;

public class GameSession : MonoBehaviour
{
    [Header("Player References")]
    public PlayerStats playerStats;

    [Header("UI Elements")]
    [SerializeField]
    public Button endTurnBTN;
    [SerializeField]
    public GameObject movementPanel;

    [Header("Events")]
    public Action OnPlayerTurnStarted;
    public Action OnPlayerTurnEnded;
    public Action OnPosIndexChanged;

    [Header("Misc")]
    public bool isPlayerTurn { get; private set; } = false;
    public int playerPosIndex { get; set; } = 0;
    public static GameSession instance { get; private set; }

    public void Awake()
    {
        if (endTurnBTN != null)
            endTurnBTN.onClick.AddListener(EndPlayerTurn);
    }

    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        InputManager.Instance.InitializePlayerInputSystem();
        movementPanel.SetActive(true);
        // «десь можно добавить логику дл€ начала хода игрока, например, активацию UI или сброс действий.
    }

    public void EndPlayerTurn()
    {
        isPlayerTurn = false;
        InputManager.Instance.OnDestroy();
        movementPanel.SetActive(false);
        // «десь можно добавить логику дл€ окончани€ хода игрока, например, деактивацию UI или обработку действий.
    }
}
