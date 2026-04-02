using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private Ball _prefabBall;
    [SerializeField] private PhysxBall _prefabPhysxBall;

    [Networked] private TickTimer delay { get; set; }

    private NetworkCharacterController _cc;
    private Vector3 _forward;

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterController>();
        _forward = transform.forward;
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();
            _cc.Move(5 * data.direction * Runner.DeltaTime);

            if (data.direction.sqrMagnitude > 0)
                _forward = data.direction;

            if (HasStateAuthority && delay.ExpiredOrNotRunning(Runner))
            {
                if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON0))
                {
                    Debug.Log("Spawn Ball");
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    Vector3 spawnPosition = transform.position + (transform.right * 4f);
                    Runner.Spawn(_prefabBall, spawnPosition, Quaternion.identity, Object.InputAuthority,
                  (runner, o) =>
                  {
                      o.GetComponent<Ball>().Spawned();
                  });
                }
            }

            else if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON1))
            {
                Debug.Log("Spawn PhysxBall");
                Vector3 spawnPosition = transform.position + (transform.right * 4f);
                delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                // Fusion version of Instantiate for networked objects. It will be spawned on all clients and the server
                Runner.Spawn(_prefabPhysxBall, spawnPosition, Quaternion.identity, Object.InputAuthority,
                  (runner, o) =>
                  {
                      o.GetComponent<PhysxBall>().Init(10 * _forward);
                  });
            }
        }
    }
}