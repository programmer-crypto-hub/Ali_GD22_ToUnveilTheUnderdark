using Fusion;
using TMPro;
using UnityEngine;

public class PhysxBall : NetworkBehaviour
{
    [Networked] private TickTimer life { get; set; }

    public void Init(Vector3 velocity)
    {
        transform.position += transform.forward * velocity.magnitude * Runner.DeltaTime;
    }

    public override void Spawned()
    {
        //var forward = transform.forward;
        life = TickTimer.CreateFromSeconds(Runner, 5.0f);
        //transform.position += forward * velocity.magnitude * Runner.DeltaTime;
    }

    public override void FixedUpdateNetwork()
    {
        if (life.Expired(Runner))
            Runner.Despawn(Object);
    }
}