using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class GameSession : NetworkBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button endTurnBTN;
    [SerializeField] private GameObject movementPanel;

    public static GameSession Instance { get; private set; }

    // This networked variable tells everyone which PlayerRef currently has authority to act
    [Networked, OnChangedRender(nameof(OnTurnChanged))]
    public PlayerRef CurrentTurnPlayer { get; set; }

    public override void Spawned() => Instance = this;

    // This runs on everyone's machine whenever the turn changes
    void OnTurnChanged()
    {
        bool isMyTurn = (Runner.LocalPlayer == CurrentTurnPlayer);
        
        // Toggle UI based on turn authority
        if (endTurnBTN != null) endTurnBTN.gameObject.SetActive(isMyTurn);
        movementPanel.SetActive(isMyTurn);
        
        if (isMyTurn) InputManager.Instance.InitializePlayerInputSystem();
        else InputManager.Instance.OnDestroy();
    }

    // Only the Host should call this to move to the next player
    public void CycleTurn(PlayerRef nextPlayer)
    {
        if (HasStateAuthority) CurrentTurnPlayer = nextPlayer;
    }
}
