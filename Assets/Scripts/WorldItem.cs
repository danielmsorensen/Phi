using UnityEngine;
using System.Collections;
using TMPro;

public class WorldItem : MonoBehaviour {

    [SerializeField]
    TMP_Text worldName;

    GameManager.World _world;
    public GameManager.World world {
        get {
            return _world;
        }
        set {
            _world = value;
            worldName.text = _world.name;
        }
    }

    WorldDiscovery.World _discoveredWorld;
    public WorldDiscovery.World discoveredWorld {
        get {
            return _discoveredWorld;
        }
        set {
            _discoveredWorld = value;
            worldName.text = _discoveredWorld.world.name;
        }
    }
}
