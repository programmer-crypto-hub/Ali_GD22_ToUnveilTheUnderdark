using Edgar.Unity; // Ensure you have Edgar installed
using Fusion;
using UnityEngine;

public class NetworkMapManager : NetworkBehaviour
{
    // The seed is synced to all players. 
    // OnChangedRender triggers the generation locally when the seed is set.
    [Networked, OnChangedRender(nameof(OnSeedChanged))]
    public int MapSeed { get; set; }

    public DungeonGeneratorGrid2D generator;

    public override void Spawned()
    {
        // Only the Host/Server chooses the seed
        if (HasStateAuthority)
        {
            MapSeed = Random.Range(1, 99999);
        }
    }

    // This runs on EVERY client when the seed arrives from the server
    void OnSeedChanged()
    {
        if (MapSeed != 0)
        {
            GenerateMap(MapSeed);
        }
    }

    private void GenerateMap(int seed)
    {
        Debug.Log($"Generating Edgar Map with Seed: {seed}");

        // 1. Tell Edgar to use our specific networked seed
        //Grid2DGenerator.GeneratorConfig.Seed = seed;
        if (EventBus.Instance == null)
        {
            Debug.LogError("EventBus is missing! Check if it's in the current scene or was destroyed.");
            return;
        }
        UnityEngine.Random.InitState(seed);
        // 2. Trigger the generation
        // Note: Use 'Generate()' for runtime generation
        EventBus.Instance.RaiseMapGenerated();
        generator = GetComponent<DungeonGeneratorGrid2D>();
        generator.Generate(); // Everyone builds the same Lego set!
    }

}
