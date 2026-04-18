using UnityEngine;
using UnityEngine.UI;

public class PlayerStatRow : MonoBehaviour
{
    [Header("Player Stats")]
    [Tooltip("Script referrals for the player stats texts.")]
    private PlayerStats playerStats;
    private PlayerProgression playerProgression;

    [SerializeField] private Text nameText;
    [SerializeField] private Text healthText;
    [SerializeField] private Text goldText;
    [SerializeField] private Text xpText;
    [SerializeField] private Text xpLevelText;

    // This is the function the Manager calls to fill in the blanks
    public void SetStats(string pName, int health, int gold, float xp)
    {
        nameText.text = pName;
        healthText.text = $"HP: {health}";
        goldText.text = $"Cave Coins: {gold}";
        xpText.text = $"XP: {xp}";
        xpLevelText.text = $"Level: {playerProgression.CurrentLevel}";
        string finalXPText = xpText.text + xpLevelText.text;
    }
}