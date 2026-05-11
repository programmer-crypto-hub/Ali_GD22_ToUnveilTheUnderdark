using System.Collections.Generic;
using UnityEngine;

public class ItemsManager : MonoBehaviour
{
    [Header("Player Weapons")]
    [SerializeField]
    [Tooltip("All items which can be obtained")]
    private List<WeaponBase>[] itemInstances;

    private WeaponBase currentItem;
}
