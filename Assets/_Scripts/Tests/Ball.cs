using Fusion;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    [Networked] private TickTimer life { get; set; }

    public override void Spawned()
    {
        life = TickTimer.CreateFromSeconds(Runner, 5.0f);
        Debug.Log(life);
    }

    public override void FixedUpdateNetwork()
    {
        transform.position += transform.forward * 50 * Runner.DeltaTime;

        if (Object.HasStateAuthority && life.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
    }
}