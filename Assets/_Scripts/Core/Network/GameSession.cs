using Fusion;
using UnityEngine;
using System;
using UnityEngine.UI;

public class GameSession : NetworkBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button endTurnBTN;

    public static GameSession Instance;

    [Networked, OnChangedRender(nameof(OnTurnChanged))]
    public int CurrentTurnID { get; set; } = -1;

    [Networked, Capacity(12)]
    public NetworkArray<int> TurnOrder => default;

    [Networked] public int TotalParticipants { get; set; }

    public bool isNetworkReady = false;

    public override void Spawned()
    {
        Instance = this;
        isNetworkReady = true;
    }

    public void RegisterParticipant(int id, string participantType)
    {
        for (int i = 0; i < TotalParticipants; i++)
        {
            if (TurnOrder[i] == id) return;
        }

        if (TotalParticipants < TurnOrder.Length)
        {
            TurnOrder.Set(TotalParticipants, id);
            TotalParticipants++;

            if (CurrentTurnID == -1)
            {
                CurrentTurnID = id;
            }

            Debug.Log($"Participant with ID {id} registered in turn order. Total: {TotalParticipants}");
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestEndTurn()
    {
        if (!HasStateAuthority || TotalParticipants == 0) return;

        int currentIndex = -1;
        for (int i = 0; i < TotalParticipants; i++)
        {
            if (TurnOrder[i] == CurrentTurnID)
            {
                currentIndex = i;
                break;
            }
        }

        if (currentIndex == -1) currentIndex = 0;

        int nextIndex = (currentIndex + 1) % TotalParticipants;
        CurrentTurnID = TurnOrder[nextIndex];

        Debug.Log($"Turn switched to ID: {CurrentTurnID}");

        OnTurnChanged();
    }

    // Внутри вашего GameSession.cs

    private int _lastProcessedTurnID = -2; // Кэш для отслеживания предыдущего хода

    public void OnTurnChanged()
    {
        if (CurrentTurnID == -1) return;

        // СЕТЕВОЙ ЗАМОК: Если этот ход мы УЖЕ обработали в прошлых кадрах, 
        // мгновенно выходим! Это полностью остановит ежекадровое мигание панели!
        if (CurrentTurnID == _lastProcessedTurnID) return;

        // Запоминаем текущий ход, чтобы запереть замок на следующие тики
        _lastProcessedTurnID = CurrentTurnID;

        if (endTurnBTN != null) endTurnBTN.gameObject.SetActive(true);

        Debug.LogWarning($"[TURN SYSTEM UI] Ход официально переключен на ID {CurrentTurnID}. Панели стабилизированы.");

        GameManager.Instance.RaiseEvent(GameManager.Events.OnTurnChanged, CurrentTurnID);
    }

}