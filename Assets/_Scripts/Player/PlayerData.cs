using Unity.VisualScripting.Dependencies.Sqlite;
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
    public float maxHealth = 100f;

    [Header("Movement")]
    [Min(0f)]
    [Tooltip("Default Speed Value, used by PlayerController.")]
    public float moveSpeed = 5f;

    [Min(0f)]
    [Tooltip("Default Jump Force, affecting Vertical Axis.")]
    public float jumpForce = 5f;

    [Header("Secondary Movement Parameters")]
    [Min(0f)]
    [Tooltip("Ускорение при начале движения (может использоваться в более сложных контроллерах).")]
    public float acceleration = 10f;

    [Min(0f)]
    [Tooltip("Скорость поворота персонажа (градусы в секунду).")]
    public float rotationSpeed = 720f;

    [Header("Currencies")]
    [Min(0f)]
    [Tooltip("Starting amount of in-game currency (e.g., coins).")]
    public float caveCoins = 0f;

    [Min(1f)]
    [Tooltip("Maximum amount of in-game currency the player can hold.")]
    public float maxCaveCoins = 200f;
}