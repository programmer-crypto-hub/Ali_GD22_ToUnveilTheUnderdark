using UnityEngine;

[CreateAssetMenu(fileName = "New Player Data", menuName = "Game Data/Player Data", order = 0)]
/// <summary>
/// ScriptableObject с базовыми параметрами игрока.
/// Хранит стартовые значения здоровья, маны и настроек движения,
/// которые затем читают PlayerStats и PlayerController.
/// </summary>
public class PlayerData : ScriptableObject
{
    [Header("Main Characteristics")]
    [Min(1f)]
    [Tooltip("Maximum Health Value by Default.")]
    public int maxHealth = 100;

    [Header("Movement")]
    [Min(0f)]
    [Tooltip("Default Speed Value, used by PlayerController.")]
    public int moveSpeed = 5;

    [Min(0f)]
    [Tooltip("Default Jump Force, affecting Vertical Axis.")]
    public float jumpForce = 5f;

    [Header("Secondary Movement Parameters")]
    [Min(0f)]
    [Tooltip("Ускорение при начале движения (может использоваться в более сложных контроллерах).")]
    public float acceleration = 10f;

    [Min(1f)]
    [Tooltip("Maximal Dice Value")]
    public int maxDiceValue = 20;

    [Min(0f)]
    [Tooltip("Скорость поворота персонажа (градусы в секунду).")]
    public float rotationSpeed = 720f;

    [Header("Currencies")]
    [Min(0f)]
    [Tooltip("Starting amount of in-game currency (e.g., coins).")]
    public int caveCoins = 0;

    [Min(1f)]
    [Tooltip("Maximum amount of in-game currency the player can hold.")]
    public int maxCaveCoins = 200;

    [Header("In-Game Stats (Visibile for other players")]
    [Min(0f)]
    [Tooltip("Current amount of in-game currency (e.g., coins).")]
    public int currentPlayerCaveCoins = 0;

    [Min(0f)]
    [Tooltip("Current Health Value.")]
    public int currentPlayerHealth = 100;
}