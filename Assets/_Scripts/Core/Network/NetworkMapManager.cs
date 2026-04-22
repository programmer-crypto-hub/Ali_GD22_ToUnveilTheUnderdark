using Edgar.Unity; // Ensure you have Edgar installed
using Fusion;
using UnityEngine;

public class NetworkMapManager : NetworkBehaviour
{
    // The seed is synced to all players. 
    // OnChangedRender triggers the generation locally when the seed is set.
    [Networked, OnChangedRender(nameof(OnSeedChanged))]
    public int MapSeed { get; set; }

    public DungeonGenerator Grid2DGenerator;

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
        Grid2DGenerator.GeneratorConfig.Seed = seed;

        // 2. Trigger the generation
        // Note: Use 'Generate()' for runtime generation
        Grid2DGenerator.Generate();
        EventBus.Instance.RaiseMapGenerated();
    }

}
