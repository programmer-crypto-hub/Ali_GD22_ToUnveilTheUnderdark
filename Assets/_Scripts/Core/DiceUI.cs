using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DiceUI : MonoBehaviour
{
    public static DiceUI Instance;

    [Header("References")]
    [Tooltip("Основная картинка кубика на Canvas экрана, которая будет менять свой вид.")]
    public Image diceImage;
    public GameObject dicePanel;

    [Tooltip("Перетащите сюда ваши 20 картинок граней кубика (спрайты) от 1 до 20!")]
    public Sprite[] diceSprites;

    public void Awake()
    {
        Instance = this;

        // По умолчанию скрываем панель при старте игры
        if (dicePanel != null) dicePanel.SetActive(false);
    }

    public void HandleDiceRolled(int result)
    {
        if (result <= 0 || diceSprites == null || diceSprites.Length == 0) return;
        if (diceImage == null || dicePanel == null) return;

        // Передаем текстуру .sprite из массива
        if (result >= 1 && result <= diceSprites.Length)
        {
            diceImage.sprite = diceSprites[result - 1];
        }

        // Включаем визуал на экране
        dicePanel.SetActive(true);
        diceImage.enabled = true;
        diceImage.gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(HideUIDelay(3f));
    }

    private IEnumerator HideUIDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (diceImage != null) diceImage.enabled = false;
        if (dicePanel != null) dicePanel.SetActive(false);

        // Безопасный вызов шагов настольной игры после скрытия кубика
        if (DiceRoller.Instance != null)
        {
            int spacesToMove = DiceRoller.Instance.ConvertDiceToMovement();
            // Пошаговое движение вашего рыцаря на spacesToMove клеток...
        }
    }
}
