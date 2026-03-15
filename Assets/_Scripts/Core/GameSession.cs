using UnityEngine;
using UnityEngine.UI;

public class GameSession : MonoBehaviour
{
    [Header("Player References")]
    public PlayerStats playerStats;

    [Header("UI Elements")]
    public Button endTurnBTN;

    public bool isPlayerTurn { get; private set; } = false;

    public void Awake()
    {
        if (endTurnBTN != null)
            endTurnBTN.onClick.AddListener(EndPlayerTurn);
    }

    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        InputManager.Instance.InitializePlayerInputSystem();
        // «десь можно добавить логику дл€ начала хода игрока, например, активацию UI или сброс действий.
    }

    public void EndPlayerTurn()
    {
        isPlayerTurn = false;
        InputManager.Instance.OnDestroy();
        // «десь можно добавить логику дл€ окончани€ хода игрока, например, деактивацию UI или обработку действий.
    }
}
