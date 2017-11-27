using UnityEngine;
using System.Collections;

public class FieldSpawner : MonoBehaviour {

    public int seed;
    public bool autoSpawnField = true;
    [Space]
    public int count = 100;
    public float radius = 25;
    [Space]
    public GameObject prefab;
    public Transform target;
    [Space]
    public bool useFalloffShader = false;

    public System.Action<Transform> OnObjectSpawned;

    float sqrMaxDistance;

    void Start() {
        sqrMaxDistance = radius * radius;

        if (useFalloffShader) {
            MeshRenderer renderer = prefab.transform.GetComponentInChildren<MeshRenderer>();
            Material material = renderer.sharedMaterial;
            material.SetFloat("_FalloffDistance", radius);
        }

        Random.InitState(seed);
        if (autoSpawnField) {
            SpawnField();
        }
    }
    
    void Update() {
        for (int i = 0; i < transform.childCount; i++) {
            Transform child = transform.GetChild(i);
            Vector3 distance = child.position - target.position;

            if (distance.sqrMagnitude > sqrMaxDistance) {
                distance = Vector3.ClampMagnitude(distance, radius);
                Vector3 position = target.position - distance;
                child.position = position;

                if(OnObjectSpawned != null) {
                    OnObjectSpawned.Invoke(child);
                }
            }
        }
    }

    public void SpawnField() {
        for (int i = 0; i < count; i++) {
            GameObject spawned = Instantiate(prefab, target.position + (Random.insideUnitSphere * radius), Random.rotation, transform);
            if (OnObjectSpawned != null) {
                OnObjectSpawned.Invoke(spawned.transform);
            }
        }
    }

    public void DestroyField() {
        for (int i = 0; i < transform.childCount; i++) {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
