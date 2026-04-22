using Fusion;
using UnityEngine;

public class BootstrapManager : NetworkBehaviour
{
    [SerializeField] private GameObject networkRunnerPrefab;

    private void Start()
    {
        if (FindFirstObjectByType<NetworkRunner>() == null)
        {
            // Spawning this prefab will bring the GameManager, 
            // InputManager, and EventBus along for the ride!
            Instantiate(networkRunnerPrefab);
        }
    }
}