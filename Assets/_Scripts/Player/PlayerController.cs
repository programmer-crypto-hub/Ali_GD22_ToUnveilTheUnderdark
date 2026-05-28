using Fusion;
using UnityEngine;
using Unity.Cinemachine;
using Unity.VisualScripting;

public class PlayerController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;

    private bool _canActThisTurn = false;

    public override void Spawned()
    {
        if (playerStats == null) playerStats = GetComponent<PlayerStats>();
        if (!HasInputAuthority || PlayerMovement.Instance == null) return;
        DiceRoller.Instance.OnDiceRollCompleted += MovePlayer; // Subscribe to dice rolls
        // Привязываем камеру ТОЛЬКО на том компьютере, который управляет этим персонажем
        if (HasInputAuthority)
        {
            // Находим новую Cinemachine Camera v3 на сцене
            CinemachineCamera vCam = FindFirstObjectByType<CinemachineCamera>();

            if (vCam != null)
            {
                // В Cinemachine v3 поле называется TrackingTarget (вместо старых Follow/LookAt)
                vCam.Follow = this.transform;

                Debug.Log("[CINEMACHINE v3] Камера Unity 6 успешно захватила вашего сетевого игрока!");
            }
            else
            {
                Debug.LogError("[CINEMACHINE v3] Не удалось найти CinemachineCamera на сцене!");
            }
        }
    }

    public bool CanMove()
    {
        if (!HasInputAuthority) return false;
        if (playerStats == null || playerStats.playerData.moveSpeed <= 0 || playerStats.CurrentHealth <= 0) return false;
        return true;
    }

    public override void FixedUpdateNetwork()
    {
        MovePlayer(0);
        if (!_canActThisTurn) return;
    }

    public void MovePlayer(int diceValue)
    {
        if (playerStats == null || playerStats.playerData == null || diceValue <= 0) return;
        
        transform.position += Vector3.right * diceValue * playerStats.playerData.moveSpeed * Runner.DeltaTime;
        transform.rotation = Quaternion.identity;
        if (playerStats.playerAnim != null)
            playerStats.playerAnim.SetTrigger("walk_trig");
    }
}
