using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatRow : MonoBehaviour
{
    [Header("Player Stats")]
    [Tooltip("Script referrals for the player stats texts.")]
    private PlayerStats playerStats;
    private PlayerProgression playerProgression;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI xpText;
    [SerializeField] private TextMeshProUGUI xpLevelText;

    // This is the function the Manager calls to fill in the blanks
    public void SetStats(string pName, int health, int gold, float xp)
    {
        nameText.text = pName;
        healthText.text = $"{health}";
        goldText.text = $"{gold}";
        xpText.text = $"{xp}";
        xpLevelText.text = $"(Level: {playerProgression.CurrentLevel})";
        string finalXPText = xpText.text + xpLevelText.text;
    }
}