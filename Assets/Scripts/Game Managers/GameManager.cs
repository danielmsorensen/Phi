using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class GameManager : MonoBehaviour {

    public static GameManager main;
    
    #region Constants
    public const string gravityTag = "Use Gravity";

    public const int groundLayer = 8;
    public const int playerLayer = 9;
    public const int playerViewLayer = 11;
    public const int interactableLayer = 12;
    public const int invisibleLayer = 13;

    public const float asteroidSize = 11.54f;

    public const string modelName = "Model";

    public static readonly Vector3 spawnPosition = new Vector3(0, 1230, 0);
    #endregion

    public FieldSpawner dustSpawner;
    public FieldSpawner asteroidSpawner;
    public float minAsteroidScale;
    public float maxAsteroidScale;
    [Space]
    public GameObject dimmer;
    public Inventory inventory;
    public GameObject pauseMenu;
    public UnityEngine.UI.Button hostingButton;
    public TMPro.TMP_Text hostingButtonText;

    public static bool inventoryOpen;
    public static bool paused;

    public static Dictionary<string, World> worlds;
    public static World currentWorld = new World("Testing World", 0, World.Area.Planet);
    public static WorldDiscovery.World discoveredWorld;
    
    void Awake() {
        main = this;
        if (SceneSwitcher.scene == SceneSwitcher.Scene.Space) {
            switch (MultiplayerManager.networkMode) {
                case (MultiplayerManager.NetworkMode.Solo):
                    SetSeed(currentWorld.seed);
                    LoadSavables();
                    GravitySource.ResetAttractedObjects();
                    break;
                case (MultiplayerManager.NetworkMode.Client):
                    SetSeed(discoveredWorld.world.seed);
                    break;
            }
        }
    }
    void Start() {
        if(SceneSwitcher.scene == SceneSwitcher.Scene.Space) {
            if(SendToMainMenu()) return;
            switch (MultiplayerManager.networkMode) {
                case (MultiplayerManager.NetworkMode.Solo):
                    MultiplayerManager.singleton.worldDiscovery.Shutdown();
                    MultiplayerManager.singleton.StartAsHost();
                    break;
                case (MultiplayerManager.NetworkMode.Client):
                    MultiplayerManager.singleton.worldDiscovery.Shutdown();
                    NetworkManager.singleton.networkAddress = discoveredWorld.address;
                    MultiplayerManager.singleton.StartAsClient();
                    hostingButton.interactable = false;
                    break;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Crosshair.active.Enable();
        }
    }

    void Update() {
        if (SceneSwitcher.scene == SceneSwitcher.Scene.Space) {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                Pause();
            }
        }
    }

    public void SetSeed(int seed) {
        if (asteroidSpawner != null) {
            asteroidSpawner.seed = currentWorld.seed;
        }
        else {
            Debug.LogError("The Asteroid Spawner is null");
        }
        if (dustSpawner != null) {
            dustSpawner.seed = currentWorld.seed;
        }
    }

    public void LoadSavables() {
        foreach(string s in currentWorld.savables) {
            Savable.Deserialize(s);
        }
    }

    #region Inventory
    public void OpenInvetory(Inventory.Data data) {
        inventory.gameObject.SetActive(true);
        inventory.OpenInventory(data);
        inventoryOpen = true;

        Cursor.lockState = CursorLockMode.None;
        Crosshair.active.Disable();

        dimmer.SetActive(true);
    }

    public void CloseInventory() {
        inventory.gameObject.SetActive(false);
        inventoryOpen = false;

        Cursor.lockState = CursorLockMode.Locked;
        Crosshair.active.Enable();

        dimmer.SetActive(false);
    }
    #endregion

    #region Menu Functions
    public void Pause() {
        pauseMenu.gameObject.SetActive(true);
        paused = true;
        if (MultiplayerManager.networkMode == MultiplayerManager.NetworkMode.Solo) {
            Time.timeScale = 0;
        }

        Cursor.lockState = CursorLockMode.None;
        Crosshair.active.Disable();

        dimmer.SetActive(true);
    }

    public void Resume() {
        pauseMenu.gameObject.SetActive(false);
        paused = false;
        Time.timeScale = 1;

        Cursor.lockState = CursorLockMode.Locked;
        Crosshair.active.Enable();

        dimmer.SetActive(false);
    }

    public void ToggleHosting() {
        if (MultiplayerManager.networkMode == MultiplayerManager.NetworkMode.Hosted) {
            MultiplayerManager.singleton.worldDiscovery.Shutdown();
            hostingButtonText.text = "Start Hosting";
            MultiplayerManager.networkMode = MultiplayerManager.NetworkMode.Solo;
        }
        else if (MultiplayerManager.networkMode == MultiplayerManager.NetworkMode.Solo) {
            MultiplayerManager.singleton.worldDiscovery.StartHosting();
            hostingButtonText.text = "Stop Hosting";
            MultiplayerManager.networkMode = MultiplayerManager.NetworkMode.Hosted;
        }
    }

    public void MainMenu() {
        MultiplayerManager.localPlayer.CmdDisconnect();

        paused = false;
        Time.timeScale = 1;

        MultiplayerManager.networkMode = MultiplayerManager.NetworkMode.Searching;
        MultiplayerManager.singleton.StopHost();
        // We do not need to change scenes manually because when the host is stopped, the uNet HLAPI will automatically change scenes for us
    }

    public void Quit() {
        foreach (Player player in MultiplayerManager.players.Values) {
            SavePlayer(player.Serialize());
        }
        SaveWorld(currentWorld);
        SceneSwitcher.Active.Quit();
    }
    #endregion

    #region Saving
    public static void LoadData() {
        CreateDirectory("Worlds");
        worlds = new Dictionary<string, World>();
        foreach (object o in LoadObjects("Worlds")) {
            if (o is World) {
                World world = (World)o;
                worlds.Add(world.name, world);
            }
        }
    }

    public static bool PlayerSaved(string username) {
        if (currentWorld != null && currentWorld.playerData != null) {
            return currentWorld.playerData.ContainsKey(username);
        }
        return false;
    }

    public static Player.Data CreatePlayerData(string username) {
        Player.Data data = new Player.Data(username, spawnPosition, Quaternion.identity, Inventory.Data.Empty);
        currentWorld.playerData.Add(username, data);
        SaveWorld(currentWorld);
        return data;
    }

    public static Player.Data GetPlayer(string username) {
        return currentWorld.playerData[username];
    }

    public static Player.Data GetOrCreatePlayer(string username) {
        if (PlayerSaved(username)) {
            return GetPlayer(username);
        }
        else {
            Debug.Log("Creating Player Data for: " + username);
            return CreatePlayerData(username);
        }
    }

    public static void SavePlayer(Player.Data data) {
        currentWorld.playerData[data.username.ToLower()] = data;
        print("Saving Player: " + data.username);
    }

    public static World CreateWorld(string name, int seed) {
        World world = new World(name.ToLower(), seed, World.Area.Planet);
        worlds.Add(world.name, world);
        SaveWorld(world);
        return world;
    }

    public static void SaveWorld(World world) {
        Savable[] savables = FindObjectsOfType<Savable>();
        foreach(Savable s in savables) {
            world.savables.Add(s.Serialize());
        }
        SaveObject(world.name.ToLower(), world, "Worlds");
        print(savables.Length + " savables found");
        print("Saved world: " + world.name);
    }

    public static void DeleteWorld(World world) {
        if(worlds.ContainsKey(world.name.ToLower())) {
            worlds.Remove(world.name);
        }
        DeleteObjectData(world.name, "Worlds");
    }
    #endregion

    public static bool SendToMainMenu() {
        if (SceneSwitcher.scene == SceneSwitcher.Scene.Space) {
            if (!AccountManager.LoggedIn) {
                Debug.LogWarning("Player not logged in, sending to Main Menu");
                SceneSwitcher.Active.MainMenu();
            }
            return !AccountManager.LoggedIn;
        }
        return false;
    }

    #region Serialization
    public static void SaveObject(string name, object o, string subPath) {
        name = name.Trim(Path.GetInvalidFileNameChars());
        subPath = subPath.Trim(Path.GetInvalidPathChars());
        string path = Application.persistentDataPath + "/" + subPath + "/" + name + ".dat";
        
        CreateDirectory(subPath);
        
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = File.Open(path, FileMode.OpenOrCreate);

        formatter.Serialize(stream, o);
        stream.Close();
    }
    public static object LoadObject(string name, string subPath) {
        name = name.Trim(Path.GetInvalidFileNameChars());
        subPath = subPath.Trim(Path.GetInvalidPathChars());
        string path = Application.persistentDataPath + "/" + subPath + "/" + name + ".dat";

        if (Exists(name, subPath)) {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = File.Open(path, FileMode.Open);

            object o = formatter.Deserialize(stream);
            stream.Close();
            return o;
        }
        else {
            Debug.LogError("The object: " + name + " in the subPath: " + subPath + " does not exist");
            return null;
        }
    }
    public static void DeleteObjectData(string name, string subPath) {
        name = name.Trim(Path.GetInvalidFileNameChars());
        subPath = subPath.Trim(Path.GetInvalidPathChars());
        string path = Application.persistentDataPath + "/" + subPath + "/" + name + ".dat";

        if (Exists(name, subPath)) {
            File.Delete(path);
        }
        else {
            Debug.LogError("The object: " + name + " in the subPath: " + subPath + " does not exist");
        }
    }
    public static object[] LoadObjects(string subPath) {
        subPath = subPath.Trim(Path.GetInvalidPathChars());
        string path = Application.persistentDataPath + "/" + subPath;

        DirectoryInfo directory = new DirectoryInfo(path);
        List<object> objects = new List<object>();

        foreach (FileInfo file in directory.GetFiles()) {
            if(file.Extension == ".dat") {
                objects.Add(LoadObject(Path.GetFileNameWithoutExtension(file.Name), subPath));
            }
        }
        return objects.ToArray();
    }
    public static bool Exists(string name, string subPath) {
        name = name.Trim(Path.GetInvalidFileNameChars());
        subPath = subPath.Trim(Path.GetInvalidPathChars());
        string path = Application.persistentDataPath + "/" + subPath + "/" + name + ".dat";

        return File.Exists(path);
    }
    public static void CreateDirectory(string subPath) {
        subPath = subPath.Trim(Path.GetInvalidPathChars());
        string path = Application.persistentDataPath + "/" + subPath;

        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }
    }
    #endregion

    [System.Serializable]
    public class World {
        public string name;
        public int seed;

        public Dictionary<string, Player.Data> playerData;
        public List<string> savables;

        public enum Area { Planet = 1, Space = 2 }
        public Area area;

        public World (string name, int seed, Area area) {
            this.name = name;
            this.seed = seed;

            playerData = new Dictionary<string, Player.Data>();
            savables = new List<string>();

            this.area = area;
        }
    }

    #region Serializable Classes
    /// <summary>
    /// Since unity doesn't flag the Vector3 as serializable, we
    /// need to create our own version. This one will automatically convert
    /// between Vector3 and SerializableVector3
    /// </summary>
    [System.Serializable]
    public struct SerializableVector3 {
        /// <summary>
        /// x component
        /// </summary>
        public float x;

        /// <summary>
        /// y component
        /// </summary>
        public float y;

        /// <summary>
        /// z component
        /// </summary>
        public float z;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rX"></param>
        /// <param name="rY"></param>
        /// <param name="rZ"></param>
        public SerializableVector3(float rX, float rY, float rZ) {
            x = rX;
            y = rY;
            z = rZ;
        }

        /// <summary>
        /// Returns a string representation of the object
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return string.Format("[{0}, {1}, {2}]", x, y, z);
        }

        /// <summary>
        /// Automatic conversion from SerializableVector3 to Vector3
        /// </summary>
        /// <param name="rValue"></param>
        /// <returns></returns>
        public static implicit operator Vector3(SerializableVector3 rValue) {
            return new Vector3(rValue.x, rValue.y, rValue.z);
        }

        /// <summary>
        /// Automatic conversion from Vector3 to SerializableVector3
        /// </summary>
        /// <param name="rValue"></param>
        /// <returns></returns>
        public static implicit operator SerializableVector3(Vector3 rValue) {
            return new SerializableVector3(rValue.x, rValue.y, rValue.z);
        }
    }

    /// <summary>
    /// Since unity doesn't flag the Quaternion as serializable, we
    /// need to create our own version. This one will automatically convert
    /// between Quaternion and SerializableQuaternion
    /// </summary>
    [System.Serializable]
    public struct SerializableQuaternion {
        /// <summary>
        /// x component
        /// </summary>
        public float x;

        /// <summary>
        /// y component
        /// </summary>
        public float y;

        /// <summary>
        /// z component
        /// </summary>
        public float z;

        /// <summary>
        /// w component
        /// </summary>
        public float w;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rX"></param>
        /// <param name="rY"></param>
        /// <param name="rZ"></param>
        /// <param name="rW"></param>
        public SerializableQuaternion(float rX, float rY, float rZ, float rW) {
            x = rX;
            y = rY;
            z = rZ;
            w = rW;
        }

        /// <summary>
        /// Returns a string representation of the object
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return string.Format("[{0}, {1}, {2}, {3}]", x, y, z, w);
        }

        /// <summary>
        /// Automatic conversion from SerializableQuaternion to Quaternion
        /// </summary>
        /// <param name="rValue"></param>
        /// <returns></returns>
        public static implicit operator Quaternion(SerializableQuaternion rValue) {
            return new Quaternion(rValue.x, rValue.y, rValue.z, rValue.w);
        }

        /// <summary>
        /// Automatic conversion from Quaternion to SerializableQuaternion
        /// </summary>
        /// <param name="rValue"></param>
        /// <returns></returns>
        public static implicit operator SerializableQuaternion(Quaternion rValue) {
            return new SerializableQuaternion(rValue.x, rValue.y, rValue.z, rValue.w);
        }
    }
    #endregion
}
