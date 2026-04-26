using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class GameSession : NetworkBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button endTurnBTN;
    [SerializeField] private GameObject movementPanel;

    public static GameSession Instance;

    // This networked variable tells everyone which PlayerRef currently has authority to act
    [Networked, OnChangedRender(nameof(OnTurnChanged))]
    public PlayerRef CurrentTurnPlayer { get; set; }
    [Networked, Capacity(4)]
    public NetworkArray<PlayerRef> PlayerOrder => default;
    [Networked] public int PlayerCount { get; set; }

    public void RegisterPlayer(PlayerRef player)
    {
        if (!HasStateAuthority) return;

        // 1. Check if the player is already in the list (prevents duplicates)
        for (int i = 0; i < PlayerCount; i++)
        {
            if (PlayerOrder[i] == player) return;
        }

        // 2. Add them if there is space
        if (PlayerCount < PlayerOrder.Length)
        {
            PlayerOrder.Set(PlayerCount, player);
            PlayerCount++;

            // 3. Auto-start the first turn if nobody is playing yet
            if (CurrentTurnPlayer == PlayerRef.None)
            {
                CurrentTurnPlayer = player;
            }

            Debug.Log($"Player {player.PlayerId} registered. Total: {PlayerCount}");
        }
    }

    public override void Spawned() => Instance = this;

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestEndTurn()
    {
        // Find current player's index in the array
        int currentIndex = -1;
        for (int i = 0; i < PlayerCount; i++)
        {
            if (PlayerOrder[i] == CurrentTurnPlayer)
            {
                currentIndex = i;
                break;
            }
        }

        // Move to next index (loop back to 0 if at the end)
        int nextIndex = (currentIndex + 1) % PlayerCount;
        CurrentTurnPlayer = PlayerOrder[nextIndex];
    }
    // This runs on everyone's machine whenever the turn changes
    public void OnTurnChanged()
    {
        if (CurrentTurnPlayer == PlayerRef.None) return;

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
