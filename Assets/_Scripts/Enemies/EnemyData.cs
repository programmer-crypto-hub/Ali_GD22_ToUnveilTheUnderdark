using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Game Data/Enemy Data", order = 1)]
/// <summary>
/// ScriptableObject с базовыми параметрами врагов.
/// Хранит стартовые значения 
/// </summary>
public class EnemyData : ScriptableObject
{
    [Header("Enemy Types")]
    [Tooltip("All Enemies")]
    public string enemyType = "Skeleton";
    public string enemyType2 = "Goblin";
    public string enemyType3 = "Vampire";

    [Header("Enemies' Velocity Values")]
    [Min(0f)]
    [Tooltip("Default Speed Value, used by EnemyController.")]
    public float skeletonSpeed = 5f;
    public float goblinSpeed = 7f;
    public float vampireSpeed = 10f;

    [Min(0f)]
    [Tooltip("Default Jump Force, affecting Vertical Axis.")]
    public float skeletonJumpForce = 10f;
    public float goblinJumpForce = 6f;
    public float vampireJumpForce = 4f;

    [Min(0f)]
    [Tooltip("Enemies Acceleration by Movement Start")]
    public float skeletonAccelSpeed = 10f;
    public float goblinAccelSpeed = 15f;
    public float vampireAccelSpeed = 20f;

    [Header("Enemies' Combat Stats")]
    [Min(1f)]
    [Tooltip("Default Health Values")]
    public float skeletonHealth = 20f;
    public float goblinHealth = 30f;
    public float vampireHealth = 50f;

    [Min(0f)]
    [Tooltip("Default Damage Values")]
    public float skeletonDamage = 5f;
    public float goblinDamage = 10f;
    public float vampireDamage = 15f;

}