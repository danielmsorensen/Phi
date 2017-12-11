using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WorldDiscovery : NetworkDiscovery {

    public static List<World> availableWorlds;
    public static System.Action<World> OnWorldDiscovered;

    public static int port;

    void Awake() {
        useNetworkManager = false;
        Initialize();
    }

    void OnEnable() {
        SceneSwitcher.OnSceneLoaded += OnSceneLoaded;
    }
    void OnDisable() {
        SceneSwitcher.OnSceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded() {
        if (SceneSwitcher.scene == SceneSwitcher.Scene.MainMenu) {
            port = int.Parse(Options.options["Discovery Port"]);
        }
    }

    public void StartHosting() {
        Shutdown();
        broadcastPort = port;
        Initialize();
        string[] versions = Application.version.Split('.');
        broadcastVersion = int.Parse(versions[0]);
        broadcastSubVersion = int.Parse(versions[1]);
        broadcastData = JsonUtility.ToJson(GameManager.currentWorld);
        if (!isServer) {
            if(!StartAsServer()) {
                Debug.LogError("Unable to host world");
            }
        }
    }

    public void StartSearching() {
        Shutdown();
        broadcastPort = port;
        Initialize();
        availableWorlds = new List<World>();
        if (!isClient) {
            if(!StartAsClient()) {
                Debug.LogError("Unable to search for worlds");
            }
        }
    }

    public void Shutdown() {
        if (running) {
            StopBroadcast();
        }
    }

    public override void OnReceivedBroadcast(string fromAddress, string data) {
        World world = new World(fromAddress, JsonUtility.FromJson<GameManager.World>(data));
        if (!availableWorlds.Contains(world)) {
            availableWorlds.Add(world);
        }
        if(OnWorldDiscovered != null) {
            OnWorldDiscovered.Invoke(world);
        }
        print("Discovered world: " + data + " on: " + fromAddress);
    }

    public struct World {
        public string address;
        public GameManager.World world;

        public World(string address, GameManager.World world) {
            this.address = address;
            this.world = world;
        }

        public override bool Equals(object obj) {
            if (obj is World) {
                World other = (World)obj;
                return address == other.address;
            }
            return false;
        }
        public override int GetHashCode() {
            return address.GetHashCode();
        }
        public override string ToString() {
            return "DiscoveredWorld: " + world.name + " on " + address;
        }

        public static bool operator ==(World w1, World w2) {
            return w1.Equals(w2);
        }
        public static bool operator !=(World w1, World w2) {
            return !(w1 == w2);
        }
    }
}
