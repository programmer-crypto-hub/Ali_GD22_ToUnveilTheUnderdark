using Fusion;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerButtonController : NetworkBehaviour, IPointerClickHandler
{
    [Header("Player Tracking")]
    public NetworkObject playerNetworkObject;

    [Header("UI Buttons")]
    [SerializeField] public Button rollDiceButton;
    [SerializeField] public Button shopButton;
    [SerializeField] public Button endTurnButton;
    [SerializeField] public Button nextWeapon;
    [SerializeField] public Button prevButton;

    private NetworkObject _localPlayerNetworkObject;
    private bool _isInitialized = false;

    public override void Spawned()
    {
        base.Spawned();

        // Asynchronously wait for the local player to exist before binding UI elements
        StartCoroutine(InitializeUIWhenPlayerSpawns());
    }

    private System.Collections.IEnumerator InitializeUIWhenPlayerSpawns()
    {
        // Wait frame-by-frame until the runner spawns the local player prefab
        while (_localPlayerNetworkObject == null)
        {
            if (Runner != null)
            {
                var localPlayerRef = Runner.GetPlayerObject(Runner.LocalPlayer);
                if (localPlayerRef != null)
                {
                    _localPlayerNetworkObject = localPlayerRef.GetComponent<NetworkObject>();
                }
            }
            yield return null;
        }

        // Runs exactly once on boot
        if (_localPlayerNetworkObject != null)
        {
            var weaponManager = _localPlayerNetworkObject.GetComponentInChildren<WeaponManager>();
            if (weaponManager != null)
            {
                if (nextWeapon != null)
                {
                    nextWeapon.onClick.RemoveAllListeners();
                    nextWeapon.onClick.AddListener(() => weaponManager.SwitchToNextWeapon());
                }
                if (prevButton != null)
                {
                    prevButton.onClick.RemoveAllListeners();
                    prevButton.onClick.AddListener(() => weaponManager.SwitchToPrevWeapon());
                }
                Debug.Log("Successfully bound weapon navigation buttons to local player data slots.");
            }
        }

        _isInitialized = true;
    }

    // Executes cleanly on network ticks with zero performance overhead!
    public override void FixedUpdateNetwork()
    {
        // Exit early if the player or dependencies aren't fully awake yet
        if (!_isInitialized) return;
        if (DiceRoller.Instance == null || GameSession.Instance == null) return;

        // Extract the authoritative, safe inputs passed through the runner
        if (GetInput(out NetworkInputData data))
        {
            if (data.diceRollPressed)
            {
                // Trigger the synchronized server-authoritative roll execution
                DiceRoller.Instance.RequestRollDice();
                Debug.Log("[NETWORK COMBAT] Authoritative server request to roll dice received.");
            }
            if (data.endTurnPressed)
            {
                // Safely fire the turn engine cleanup RPC
                GameSession.Instance.RPC_RequestEndTurn();
                Debug.Log("[NETWORK COMBAT] Authoritative server request to close active turn state.");
            }
        }
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        GameObject clickedObject = eventData.pointerPressRaycast.gameObject;
        if (clickedObject == null) return;

        if (clickedObject == rollDiceButton.gameObject || clickedObject.transform.IsChildOf(rollDiceButton.transform))
        {
            if (DiceRoller.Instance != null && DiceUI.Instance != null)
            {
                Debug.LogWarning("[HARDCORE BYPASS] Физический клик по КУБИКУ пойман напрямую через Pointer!");
                DiceRoller.Instance.RequestRollDice();
                DiceUI.Instance.HandleDiceRolled(DiceRoller.Instance.DiceRollResult);
            }
        }

        if (clickedObject == endTurnButton.gameObject || clickedObject.transform.IsChildOf(endTurnButton.transform))
        {
            if (GameSession.Instance != null)
            {
                Debug.LogWarning("[HARDCORE BYPASS] Физический клик по КОНЦУ ХОДА пойман напрямую через Pointer!");
                GameSession.Instance.RPC_RequestEndTurn();
            }
        }

        if (clickedObject == shopButton.gameObject || clickedObject.transform.IsChildOf(shopButton.transform))
        {
            if (ShopUIManager.Instance != null)
            {
                ShopUIManager.Instance.ToggleShop(true);
            }
        }
    }
}
