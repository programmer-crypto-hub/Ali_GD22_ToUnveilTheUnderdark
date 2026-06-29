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

        UnityEngine.Random.InitState(seed);
        // Trigger the generation
        GameManager.Instance.RaiseMapGenerated();
        generator = GetComponent<DungeonGeneratorGrid2D>();
        generator.Generate(); 
    }

}
