using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System.IO;

public class Options : MonoBehaviour {

    [Header("Create World")]
    public TMP_InputField worldNameField;
    public TMP_InputField seedField;
    public TMP_Text infomation;
    [Header("Load World")]
    public Transform worldsHolder;
    public WorldItem worldItemTemplate;
    [Header("Join World")]
    public Transform discoveredWorldsHolder;
    public WorldItem discoveredWorldItemTemplate;

    public static Dictionary<string, string> options;

    List<WorldDiscovery.World> onlineWorlds = new List<WorldDiscovery.World>();

    void Awake() {
        options = new Dictionary<string, string>();
        if (!File.Exists(Application.persistentDataPath + "/settings.txt")) {
            File.CreateText(Application.persistentDataPath + "/settings.txt").Close();
        }
        string[] lines = File.ReadAllLines(Application.persistentDataPath + "/settings.txt");
        for (int i = 0; i < lines.Length; i++) {
            string line = lines[i];
            if (line.Length > 0) {
                if (line[0] != '#') {
                    string[] parts = line.Split(':');
                    if (parts.Length > 1) {
                        if (!options.ContainsKey(parts[0])) {
                            options.Add(parts[0], parts[1]);
                        }
                    }
                }
            }
        }
        Dictionary<string, string> baseOptions = new Dictionary<string, string>();
        baseOptions.Add("Network Port", "7777");
        baseOptions.Add("Discovery Port", "47777");
        foreach (KeyValuePair<string, string> pair in baseOptions) {
            if (!options.ContainsKey(pair.Key)) {
                AddOption(pair.Key, pair.Value);
            }
        }
    }

    void Start() {
        LoadWorlds();
        MultiplayerManager.singleton.worldDiscovery.StartSearching();
    }

    public static void AddOption(string key, string value) {
        if (!options.ContainsKey(key)) {
            options.Add(key, value);
            File.AppendAllText(Application.persistentDataPath + "/settings.txt", System.Environment.NewLine + key + ":" + value);
        }
    }

    public void CreateWorld() {
        if (!GameManager.worlds.ContainsKey(worldNameField.text.ToLower())) {
            GameManager.World world = GameManager.CreateWorld(worldNameField.text.ToLower(), int.Parse(seedField.text));
            GameManager.currentWorld = world;
            MultiplayerManager.networkMode = MultiplayerManager.NetworkMode.Solo;
            SceneSwitcher.Active.Play();
        }
        else {
            infomation.color = Color.red;
            infomation.text = "That world already exists";
        }
    }

    public void LoadWorld(WorldItem item) {
        GameManager.currentWorld = item.world;
        MultiplayerManager.networkMode = MultiplayerManager.NetworkMode.Solo;
        SceneSwitcher.Active.Play();
    }

    public void DeleteWorld(WorldItem item) {
        GameManager.DeleteWorld(item.world);
        Destroy(item.gameObject);
    }

    public void LoadWorlds() {
        GameManager.LoadData();
        ClearActiveChildren(worldsHolder);
        foreach (GameManager.World world in GameManager.worlds.Values) {
            WorldItem item = Instantiate(worldItemTemplate, worldsHolder);
            item.gameObject.SetActive(true);
            item.world = world;
        }
        LayoutRebuilder.MarkLayoutForRebuild(worldsHolder.parent as RectTransform);
    }

    public void LoadOnlineWorld(WorldItem item) {
        MultiplayerManager.networkMode = MultiplayerManager.NetworkMode.Client;
        GameManager.discoveredWorld = item.discoveredWorld;
        SceneSwitcher.Active.Play();
    }

    public void LoadOnlineWorlds() {
        ClearActiveChildren(discoveredWorldsHolder);
        if (!MultiplayerManager.singleton.worldDiscovery.isClient) {
            MultiplayerManager.singleton.worldDiscovery.StartSearching();
        }
        onlineWorlds.Clear();
    }

    public void AddOnlineWorld(WorldDiscovery.World world) {
        if (!onlineWorlds.Contains(world)) {
            onlineWorlds.Add(world);
            WorldItem item = Instantiate(discoveredWorldItemTemplate, discoveredWorldsHolder);
            item.gameObject.SetActive(true);
            item.discoveredWorld = world;
            LayoutRebuilder.MarkLayoutForRebuild(discoveredWorldsHolder.parent as RectTransform);
        }
    }

    public void Quit() {
        SceneSwitcher.Active.Quit();
    }

    void OnEnable() {
        WorldDiscovery.OnWorldDiscovered += AddOnlineWorld;
    }
    void OnDisable() {
        WorldDiscovery.OnWorldDiscovered -= AddOnlineWorld;
    }

    public void ClearActiveChildren(Transform t) {
        for (int i = 0; i < t.childCount; i++) {
            GameObject child = t.GetChild(i).gameObject;
            if (child.activeSelf) {
                Destroy(child);
            }
        }
    }
}
