using Fusion;
using Fusion.Addons.Physics;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BasicPlayerSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkObject _playerPrefab;

    private NetworkRunner _runner;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    private bool _mouseButton0;
    private bool _mouseButton1;

    private void Update()
    {
        // Gathers quick click states inside standard Unity frames
        if (Input.GetMouseButtonDown(0)) _mouseButton0 = true;
        if (Input.GetMouseButtonDown(1)) _mouseButton1 = true;
    }

    // Kicked off by your UI Button
    public void StartHostFromButton()
    {
        if (_runner != null) return; // Already running, ignore extra clicks

        StartGame(GameMode.Host);
    }

    private async void StartGame(GameMode mode)
    {
        // 1. Setup the Network Runner GameObject cleanly
        GameObject go = new GameObject("Fusion_Network_Runner");
        _runner = go.AddComponent<NetworkRunner>();
        go.AddComponent<RunnerSimulatePhysics3D>();
        DontDestroyOnLoad(go);
        DontDestroyOnLoad(this.gameObject);


        // 2. Register callbacks ONCE right here
        _runner.AddCallbacks(this);
        _runner.ProvideInput = true;

        // 3. Setup standard scene management
        var sceneManager = go.AddComponent<NetworkSceneManagerDefault>();

        try
        {
            int targetSceneIndex = 3; // Your GameScene Index
            await _runner.StartGame(new StartGameArgs()
            {
                GameMode = mode,
                SessionName = "GameRoom",
                Scene = SceneRef.FromIndex(targetSceneIndex),
                SceneManager = sceneManager
            });
        }
        catch (Exception e)
        {
            Debug.LogError($"Fatal Error during StartGame: {e.Message}");
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        // Standard frame-by-frame queries (No subscriptions needed)
        if (Input.GetKey(KeyCode.W)) data.direction += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) data.direction += Vector3.back;
        if (Input.GetKey(KeyCode.A)) data.direction += Vector3.left;
        if (Input.GetKey(KeyCode.D)) data.direction += Vector3.right;

        data.buttons.Set(NetworkInputData.MOUSEBUTTON0, _mouseButton0 || Input.GetMouseButton(0));
        data.buttons.Set(NetworkInputData.MOUSEBUTTON1, _mouseButton1 || Input.GetMouseButton(1));

        // Flush tracking variables immediately after processing
        _mouseButton0 = false;
        _mouseButton1 = false;

        input.Set(data);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene(); // Load loading screen
        if (!runner.IsServer) return;
        if (_spawnedCharacters.ContainsKey(player)) return; // Prevent duplicate spawns

        Vector3 spawnPos = new Vector3((player.RawEncoded % 5) * 2f, 0f, 0f);
        Quaternion spawnRotation = Quaternion.Euler(-90f, 0f, 0f);
        var networkPlayer = runner.Spawn(_playerPrefab, spawnPos, spawnRotation, player);

        if (networkPlayer != null)
        {
            _spawnedCharacters.Add(player, networkPlayer);
            runner.SetPlayerObject(player, networkPlayer);

            if (GameManager.Instance != null) GameManager.Instance.HandleExploration();
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            if (networkObject != null) runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }

    // Unregister callbacks if this manager component is destroyed to prevent leaks
    private void OnDestroy()
    {
        if (_runner != null)
        {
            _runner.RemoveCallbacks(this);
        }
    }

    // Boilerplates required by the INetworkRunnerCallbacks Interface
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}
