using Fusion;
using Fusion.Addons.Physics;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BasicPlayerSpawner : NetworkBehaviour, INetworkRunnerCallbacks 
{
    private void OnEnable()
    {
        // If the runner is already active, register now
        var runner = FindFirstObjectByType<NetworkRunner>();
        if (runner != null) runner.AddCallbacks(this);
    }
    public override void Spawned()
    {
        Debug.Log("BasicPlayerSpawner Spawned: " + this.gameObject.name);
        if (HasStateAuthority)
        {
            // Find the GameSession (which should be on your NetworkRunner or global object)
            var session = FindFirstObjectByType<GameSession>();
            if (session != null)
            {
                // Object.InputAuthority is the PlayerRef for the person controlling this prefab
                session.RegisterPlayer(Object.InputAuthority);
            }
        }
    }

    private bool _mouseButton0;
    private bool _mouseButton1;

    private void Update()
    {
        _mouseButton0 = _mouseButton0 || Input.GetMouseButton(0);
        _mouseButton1 = _mouseButton1 || Input.GetMouseButton(1);
        if (Input.GetMouseButtonDown(0)) _mouseButton0 = true;
        if (Input.GetMouseButtonDown(1)) _mouseButton1 = true;
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        if (Input.GetKey(KeyCode.W))
            data.direction += Vector3.forward;

        if (Input.GetKey(KeyCode.S))
            data.direction += Vector3.back;

        if (Input.GetKey(KeyCode.A))
            data.direction += Vector3.left;

        if (Input.GetKey(KeyCode.D))
            data.direction += Vector3.right;

        if (Input.GetMouseButton(0)) // Left click / Touchpad tap
        {
            data.buttons.Set(NetworkInputData.MOUSEBUTTON0, _mouseButton0);
        }

        if (Input.GetMouseButton(1)) // Right click / Touchpad hold
        {
            data.buttons.Set(NetworkInputData.MOUSEBUTTON1, _mouseButton1);
        }

        input.Set(data);
    }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {
        Debug.LogWarning($"PHOTON SHUTDOWN! Reason: {shutdownReason}");
    }
 
    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) {
        Debug.Log("Connected to Fusion Server");
    }
    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {
        Debug.Log($"Disconnected: {reason}");
    }
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

    private NetworkRunner _runner;

    [SerializeField] private NetworkObject _playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();


    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            // 1. Calculate a simple spawn position
            Vector3 spawnPos = new Vector3(player.RawEncoded % 5 * 2, 0, 0);

            // 2. Spawn the PREFAB (No DontDestroyOnLoad needed here!)
            var networkPlayer = runner.Spawn(_playerPrefab, spawnPos, Quaternion.identity, player);

            if (networkPlayer != null)
            {
                // 3. Set the Player Object so Fusion knows who this is
                runner.SetPlayerObject(player, networkPlayer);

                // 4. Now that the player exists, start the game logic
                if (GameManager.Instance != null) GameManager.Instance.HandleExploration();

                Debug.Log($"Spawned and assigned player {player}");
            }
            else
            {
                Debug.LogError("Failed to spawn player prefab!");
            }
        }
    }


    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }

    async void StartGame(GameMode mode)
    {
        _runner = FindFirstObjectByType<NetworkRunner>();
        GameObject go = new GameObject("Fusion_Network_Runner");
        if (_runner == null)
            _runner = go.AddComponent<NetworkRunner>();
        _runner.AddCallbacks(this);
        var physics2D = go.AddComponent<RunnerSimulatePhysics2D>();
        DontDestroyOnLoad(go);
        DontDestroyOnLoad(this);
        _runner.ProvideInput = true;
        Debug.Log($"Starting game with mode: {mode}");

        _runner = FindFirstObjectByType<NetworkRunner>();
        _runner.AddCallbacks(this);

        // Create the NetworkSceneInfo from the current scene
        var sceneRef = SceneRef.FromIndex(3);

        var sceneInfo = new NetworkSceneInfo();
        var sceneManager = gameObject.GetComponent<NetworkSceneManagerDefault>();
        if (sceneManager == null) sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
        if (sceneRef.IsValid)
        {
            sceneInfo.AddSceneRef(sceneRef, LoadSceneMode.Additive);
        }
        var spawner = FindFirstObjectByType<BasicPlayerSpawner>();
        _runner.AddCallbacks(spawner);
        var mapManager = FindFirstObjectByType<NetworkMapManager>();
        //sceneManager.IsMultiplePeer = false;
        // Start or join (depends on gamemode) a session with a specific name
        try
        {
            Debug.Log("Attempting to run");
            var result = await _runner.StartGame(new StartGameArgs()
            {
                GameMode = mode,
                SessionName = "TestRoom",
                Scene = SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath("Assets/_Scenes/GameScene.unity")),   // This tells Photon: "Move everyone here once connected"
                SceneManager = sceneManager 
            });

            Debug.Log($"Final Result: {result.Ok}, Reason: {result.ShutdownReason}");
            if (result.Ok)
            {
                Debug.Log("Fusion Started Successfully! Loading GameScene...");
            }
            else
            {
                Debug.LogError($"Fusion Failed to Start: {result.ShutdownReason}");
                Destroy(go);
            }

        }
        catch (System.Exception e)
        {
            Debug.LogError($"Fatal Error during StartGame: {e.Message}");
        }
    }


    public void StartHostFromButton()
    {
        if(_runner == null)
        {
            StartGame(GameMode.Host);
            var Scene = SceneRef.FromIndex(3);
            //if (GUI.Button(new Rect(0, 40, 200, 40), "Join"))
            //{
            //    StartGame(GameMode.Client);
            //}
        }
    }
}