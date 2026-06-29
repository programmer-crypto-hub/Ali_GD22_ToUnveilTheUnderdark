using UnityEngine;

[CreateAssetMenu(
    fileName = "WeaponData",
    menuName = "Game Data/Weapon Data",
    order = 0)]
public class WeaponData : ScriptableObject
{
    public enum WeaponType
    {
        Melee = 0,   
        Ranged = 1, 
        Magic = 2
    }
    public string weaponName = "New Weapon";

    public WeaponType weaponType = WeaponType.Melee;

    public Sprite icon;

    [Min(0f)]
    public float damage = 10f;
    [Min(0.1f)]
    public float attackSpeed = 1f;
    [Min(0f)]
    public float range = 2f;
    [Min(0f)]
    public float knockbackForce = 0f;

    public string attackAnimationName;
    public AudioClip attackSound;

    public GameObject projectilePrefab;
}
