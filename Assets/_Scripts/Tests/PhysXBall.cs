using UnityEngine;
using Fusion;

public class PhysxBall : NetworkBehaviour
{
    [Networked] private TickTimer life { get; set; }

    public void Init(Vector3 velocity)
    {
        GetComponent<Rigidbody>().linearVelocity = velocity;  
    }

    public override void Spawned()
    {
        var forward = transform.forward;
        life = TickTimer.CreateFromSeconds(Runner, 5.0f);
        GetComponent<Rigidbody>().linearVelocity = forward;
    }

    public override void FixedUpdateNetwork()
    {
        if (life.Expired(Runner))
            Runner.Despawn(Object);
    }
}