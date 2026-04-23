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

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestEndTurn()
    {
        // 1. Logic runs on Server: Cycle to the next player
        // (You likely have a list of PlayerRefs)
        PlayerRef nextPlayer = CurrentTurnPlayer(Object.InputAuthority);

        // 2. Update the [Networked] Turn property
        CurrentTurnPlayer = nextPlayer;
    }
    // This runs on everyone's machine whenever the turn changes
    void OnTurnChanged()
    {
        bool isMyTurn = (Runner.LocalPlayer == CurrentTurnPlayer);
        
        // Toggle UI based on turn authority
        if (endTurnBTN != null) endTurnBTN.gameObject.SetActive(isMyTurn);
        movementPanel.SetActive(isMyTurn);

        if (isMyTurn)
        {
            InputManager.Instance.playerActionMap.Enable(); // Enable the Action Map
            movementPanel.SetActive(true);
        }
        else
        {
            InputManager.Instance.playerActionMap.Disable(); // Disable the Action Map
            movementPanel.SetActive(false);
            ShopUIManager.Instance.ToggleShop(false); // Close shop if left open
        }
    }

    // Only the Host should call this to move to the next player
    public void CycleTurn(PlayerRef nextPlayer)
    {
        if (HasStateAuthority) CurrentTurnPlayer = nextPlayer;
    }
}
