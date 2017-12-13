using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTiler : MonoBehaviour {

    public float viewRadius;
    public float groundTileSize;
    [Space]
    public GameObject groundPrefab;
    public Transform target;
    public Transform map;

    int tilesInView;
    Dictionary<Vector3, Transform> tiles = new Dictionary<Vector3, Transform>();
    Vector3 lastPlayerTile;

    public System.Action<Transform> onTileSpawned;

    void Awake() {
        tilesInView = Mathf.CeilToInt(viewRadius / groundTileSize);
        SpawnMap();
    }

    void SpawnMap() {
        for (int i = 0; i < map.childCount; i++) {
            Destroy(map.GetChild(i).gameObject);
        }
        for (int x = -tilesInView; x <= tilesInView; x++) {
            for (int z = -tilesInView; z <= tilesInView; z++) {
                Vector3 position = new Vector3(x, 0, z) * groundTileSize;
                GameObject tile = Instantiate(groundPrefab, position, Quaternion.identity, map);
                tile.name = "Ground (" + position.x.ToString("000") + ", " + position.z.ToString("000") + ")";
                tiles.Add(position, tile.transform);
                if(onTileSpawned != null) {
                    onTileSpawned.Invoke(tile.transform);
                }
            }
        }
    }

    void Update() {
        if (target != null) {
            Vector3 playerTile = GetTile(target.position);
            if (playerTile != lastPlayerTile) {
                Dictionary<Vector3, Transform> coppiedTiles = new Dictionary<Vector3, Transform>(tiles);
                foreach (KeyValuePair<Vector3, Transform> pair in coppiedTiles) {
                    Vector3 difference = (pair.Key - playerTile) / groundTileSize;
                    bool x = Mathf.Abs(difference.x) > tilesInView;
                    bool z = Mathf.Abs(difference.z) > tilesInView;
                    Vector3 position = pair.Key;
                    if (x || z) {
                        if (x) position.x = playerTile.x - Mathf.Sign(difference.x) * tilesInView * groundTileSize;
                        if (z) position.z = playerTile.z - Mathf.Sign(difference.z) * tilesInView * groundTileSize;
                        pair.Value.position = position;
                        tiles.Remove(pair.Key);
                        tiles.Add(position, pair.Value);
                        pair.Value.gameObject.name = "Ground (" + position.x.ToString("000") + ", " + position.z.ToString("000") + ")";
                        if(onTileSpawned != null) {
                            onTileSpawned.Invoke(pair.Value);
                        }
                    }
                }
            }
            lastPlayerTile = playerTile;
        }
    }

    public Vector3 GetTile(Vector3 position) {
        return new Vector3(Mathf.Floor(position.x / groundTileSize + 0.5f) * groundTileSize, position.y, Mathf.Floor(position.z / groundTileSize + 0.5f) * groundTileSize);
    }

    void OnDrawGizmos() {
        if (map != null) {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere((map == null) ? Vector3.zero : map.position, viewRadius);
            if (!Application.isPlaying) {
                float tilesInView = Mathf.CeilToInt(viewRadius / groundTileSize) * groundTileSize + groundTileSize / 2f;
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(map.position, new Vector3(tilesInView * 2, 0.1f, tilesInView * 2));
            }
        }
    }
}
