using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : GravitySource {

    public bool keepScale;

    public Data data;
    Transform ores;

    void Awake() {
        ores = new GameObject("Ores").transform;
        ores.SetParent(transform);
    }

    protected override void Start() {
        base.Start();
        if (GameManager.main.asteroidSpawner != null) {
            GameManager.main.asteroidSpawner.OnObjectSpawned += CheckChange;
        }
        CheckChange(transform);
    }

    void CheckChange(Transform t) {
        if (t.gameObject == gameObject) {
            int hash = (transform.position.x.ToString() + "-" + transform.position.y + "-" + transform.position.z).GetHashCode();
            SetData(new Data(hash, (keepScale) ? transform.localScale.x : 0));
        }
    }

    public void SetData(Data data) {
        this.data = data;
        transform.localScale = data.scale * Vector3.one;
    }

    public struct Data {
        public int hash;
        public float scale;

        public Data(int hash, float scale=0) {
            this.hash = hash;
            Random.InitState(hash);
            this.scale = (scale == 0) ? Random.Range(GameManager.main.minAsteroidScale, GameManager.main.maxAsteroidScale) : scale;
        }
    }
}
