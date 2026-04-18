using Fusion;
using Fusion.Addons.Physics;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BasicPlayerSpawner : MonoBehaviour, INetworkRunnerCallbacks 
{
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        Debug.Log("BasicPlayerSpawner Awake: " + this.gameObject.name);
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
        Debug.Log("OnPlayerJoined method active");
        if (runner.IsServer && runner.IsRunning)
        {
            Vector3 spawnPos = new Vector3(player.RawEncoded % 5, 10, 0); 
            Quaternion spawnRotation = new Quaternion(-90, 0, 0, 0);
            // 1. Spawn the player prefab
            DontDestroyOnLoad(_playerPrefab);
            if (_playerPrefab == null)
            {
                Debug.LogError("Player Prefab is not assigned in the Inspector!");
                return; 
            }
            var networkPlayer = runner.Spawn(_playerPrefab, spawnPos, spawnRotation, player, (runner, obj) => {
                // This happens BEFORE the object is fully placed in the world
                obj.gameObject.SetActive(true);
            });
            networkPlayer.gameObject.SetActive(true);
            Debug.Log($"OnPlayerJoined: Spawning Player: {networkPlayer}");
            if (networkPlayer == null)
            {
                Debug.LogError("FATAL: runner.Spawn returned NULL! The Prefab Table is likely out of sync.");
                return;
            }
            // 2. ASSIGN IT: This is what fills that "Local Player Object" field in the Inspector
            runner.SetPlayerObject(player, networkPlayer);

            Debug.Log($"Player {player} joined and assigned to {networkPlayer.name}");
            // Create a unique position for the player
            Vector3 spawnPosition = new Vector3((player.RawEncoded % runner.Config.Simulation.PlayerCount) * 3, 1, 0);
            
            runner.SetPlayerObject(player, networkPlayer); 
            // Keep track of the player avatars for easy access
            _spawnedCharacters.Add(player, networkPlayer);
        }
        else
        {
            Debug.LogWarning($"OnPlayerJoined called but runner is not server or not running. IsServer: {runner.IsServer}, IsRunning: {runner.IsRunning}");
        }

        if (_playerPrefab == null)
        {
            _playerPrefab = Resources.Load<NetworkObject>("Player");
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
        GameObject go = new GameObject("Fusion_Network_Runner");
        var physics2D = go.AddComponent<RunnerSimulatePhysics2D>(); 
        var _runner = go.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        Debug.Log($"Starting game with mode: {mode}");

        _runner = FindFirstObjectByType<NetworkRunner>();
        _runner.AddCallbacks(this);

        // Create the NetworkSceneInfo from the current scene
        var sceneRef = SceneRef.FromIndex(2);

        var sceneInfo = new NetworkSceneInfo();
        var sceneManager = gameObject.GetComponent<NetworkSceneManagerDefault>();
        if (sceneManager == null) sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
        if (sceneRef.IsValid)
        {
            sceneInfo.AddSceneRef(sceneRef, LoadSceneMode.Additive);
        }
        var spawner = FindFirstObjectByType<BasicPlayerSpawner>();
        _runner.AddCallbacks(spawner);
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
            var Scene = SceneRef.FromIndex(2);
            //if (GUI.Button(new Rect(0, 40, 200, 40), "Join"))
            //{
            //    StartGame(GameMode.Client);
            //}
        }
    }
}