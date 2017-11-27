using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MultiplayerManager : NetworkManager {

    public static Dictionary<string, Player> players = new Dictionary<string, Player>();
    public static Player localPlayer;

    public static System.Action<Player> onLocalPlayerSpawned;
    public static System.Action onLocalPlayerLeave;

    public enum NetworkMode { Solo, Hosted, Client, Searching };
    static NetworkMode _networkMode;
    public static NetworkMode networkMode {
        get {
            return _networkMode;
        }
        set {
            _networkMode = value;
        }
    }

    public WorldDiscovery worldDiscovery;

    public static int port;

    public static new MultiplayerManager singleton {
        get {
            return NetworkManager.singleton as MultiplayerManager;
        }
    }

    void Awake() {
        #region Network Manger Awake Method
        LogFilter.currentLogLevel = (int)logLevel;

        if (singleton != null && singleton != this) {
            Destroy(gameObject);
            return;
        }

        if (dontDestroyOnLoad) {
            DontDestroyOnLoad(gameObject);
        }

        NetworkManager.singleton = this;
        #endregion
    }

    void OnEnable() {
        SceneSwitcher.OnSceneLoaded += OnSceneLoaded;
    }
    void OnDisable() {
        SceneSwitcher.OnSceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded() {
        if (SceneSwitcher.scene == SceneSwitcher.Scene.MainMenu) {
            networkMode = NetworkMode.Searching;
            port = int.Parse(Options.options["Network Port"]);
        }
        players.Clear();
    }

    public void StartAsHost() {
        networkPort = port;
        StartHost();
    }

    public void StartAsClient() {
        networkPort = port;
        StartClient();
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId) {
        if (SceneSwitcher.scene == SceneSwitcher.Scene.Space) {
            Player player = Instantiate(playerPrefab, GameManager.spawnPosition, Quaternion.identity).GetComponent<Player>();
            NetworkServer.AddPlayerForConnection(conn, player.gameObject, playerControllerId);
        }
    }
}
