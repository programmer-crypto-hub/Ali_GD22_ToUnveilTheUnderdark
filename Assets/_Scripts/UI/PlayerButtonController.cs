using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // ОБЯЗАТЕЛЬНО: Подключаем систему ивентов Unity
using Fusion;

/*
 * Emergency Multi-Bypass Script for MVP Presentation.
 * Uses direct Pointer Clicks to ignore completely any empty OnClick() or RemoveAllListeners() bugs.
 */
public class PlayerButtonController : MonoBehaviour, IPointerClickHandler
{
    [Header("Player Tracking")]
    public NetworkObject playerNetworkObject;

    [Header("UI Buttons")]
    [SerializeField] public Button rollDiceButton;
    [SerializeField] public Button shopButton;
    [SerializeField] public Button endTurnButton;
    [SerializeField] public Button nextWeapon;
    [SerializeField] public Button prevButton;

    private bool _isInitialized = false;

    private void Update()
    {
        // ==================== ФАЗА 1: СЕТЕВОЕ ОЖИДАНИЕ ====================
        if (!_isInitialized)
        {
            // 1. Ленивый поиск игрока
            if (playerNetworkObject == null)
            {
                foreach (var player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
                {
                    if (player.Object != null && player.Object.HasInputAuthority)
                    {
                        playerNetworkObject = player.GetComponent<NetworkObject>();
                        break;
                    }
                }
                if (playerNetworkObject == null) return;
            }

            // 2. Сетевой барьер
            if (DiceRoller.Instance == null || DiceUI.Instance == null || GameSession.Instance == null) return;

            // 3. Точка активации (срабатывает 1 раз на 201-м кадре)
            _isInitialized = true;

            var weaponManager = playerNetworkObject.GetComponentInChildren<WeaponManager>();
            if (weaponManager != null)
            {
                if (nextWeapon != null) { nextWeapon.onClick.RemoveAllListeners(); nextWeapon.onClick.AddListener(() => weaponManager.SwitchToNextWeapon()); }
                if (prevButton != null) { prevButton.onClick.RemoveAllListeners(); prevButton.onClick.AddListener(() => weaponManager.SwitchToPrevWeapon()); }
            }

            Debug.LogWarning("[UI SYSTEM] Сетевой барьер успешно пройден на 201-м кадре!");
        }

        // 1. ПРИНУДИТЕЛЬНЫЙ БРОСОК КУБИКА (Кнопка R)
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (DiceRoller.Instance != null && DiceUI.Instance != null && _isInitialized == true)
            {
                DiceRoller.Instance.RequestRollDice();
                DiceUI.Instance.HandleDiceRolled(DiceRoller.Instance.DiceRollResult);
            }
        }

        // 2. ПРИНУДИТЕЛЬНЫЙ КОНЕЦ ХОДА (Кнопка E)
        if (Input.GetKeyDown(KeyCode.E) && _isInitialized == true)
        {
            if (GameSession.Instance != null)
            {
                GameSession.Instance.RPC_RequestEndTurn();
                _isInitialized = false;
            }
        }
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        // Проверяем, на какой конкретно объект нажал игрок пальцем/тачпадом
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
