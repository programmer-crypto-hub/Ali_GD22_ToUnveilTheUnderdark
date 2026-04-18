using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class UIStatsController : MonoBehaviour
{
    public static UIStatsController Instance;
    public GameObject statRowPrefab;
    public Transform container;

    // A dictionary to link specific players to their UI rows
    private Dictionary<PlayerRef, PlayerStatRow> _playerRows = new Dictionary<PlayerRef, PlayerStatRow>();

    private void Awake() => Instance = this;

    public void UpdateDisplay(PlayerStats stats)
    {
        // 1. If we don't have a row for this player yet, create one
        if (!_playerRows.ContainsKey(stats.Object.InputAuthority))
        {
            var newRow = Instantiate(statRowPrefab, container).GetComponent<PlayerStatRow>();
            _playerRows.Add(stats.Object.InputAuthority, newRow);
        }

        // 2. Update the values on the row
        //_playerRows[stats.Object.InputAuthority].SetStats(stats.PlayerName, stats.Health, stats.Gold);
    }
}
