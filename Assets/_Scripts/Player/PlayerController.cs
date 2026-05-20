using Fusion;
using UnityEngine;
using Unity.Cinemachine;

public class PlayerController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;

    public override void Spawned()
    {
        if (playerStats == null) playerStats = GetComponent<PlayerStats>();

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
}
