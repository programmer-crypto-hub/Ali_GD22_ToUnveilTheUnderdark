using UnityEngine;

[CreateAssetMenu(fileName = "New Player Data", menuName = "Game Data/Player Data", order = 0)]

public class PlayerData : ScriptableObject
{
    [Min(1f)]
    public int maxHealth = 100;
    [Min(0f)]
    public int currentPlayerHealth = 100;

    [Min(0f)]
    public int moveSpeed = 50;
    [Min(0f)]
    public float jumpForce = 5f;
    [Min(0f)]
    public float acceleration = 10f;
    [Min(1f)]
    public int maxDiceValue = 20;
    [Min(0f)]
    public float rotationSpeed = 720f;

    [Min(0f)]
    public int caveCoins = 0;
    [Min(1f)]
    public int maxCaveCoins = 200;
    [Min(0f)]
    public int currentPlayerCaveCoins = 0;
}