using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ”правл€ет оружием игрока (вариант B: все оружи€ Ч дочерние объекты, смена через Enable/Disable):
/// - хранит экземпл€ры оружи€ из префаба игрока;
/// - листает только Ђдоступныеї оружи€ (список availableWeapons);
/// - переключение по кнопкам 1 (назад) и 2 (вперЄд) через InputManager.
/// </summary>
public class ItemsManager : MonoBehaviour
{
    [Header("ќружи€ на игроке")]
    [SerializeField]
    [Tooltip("All items which can be obtained")]
    private WeaponBase[] itemInstances;

    private int currentAvailableIndex;
    private WeaponBase currentItem;

    
}
