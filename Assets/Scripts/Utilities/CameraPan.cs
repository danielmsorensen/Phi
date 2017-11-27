using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPan : MonoBehaviour {

    public Vector3 minAngles, maxAngles;
    public float frequency;

    Vector3 targetAngles;
    Vector3 velocity;

    void Awake() {
        StartCoroutine(SetTargetAngles());
    }

    void Update() {
        transform.eulerAngles = Vector3.SmoothDamp(transform.eulerAngles, targetAngles, ref velocity, 1 / frequency, float.MaxValue, Time.deltaTime);
    }

    IEnumerator SetTargetAngles() {
        while (Application.isPlaying) {
            targetAngles = new Vector3(Random.Range(minAngles.x, maxAngles.x), Random.Range(minAngles.y, maxAngles.y), Random.Range(minAngles.z, maxAngles.z));
            yield return new WaitForSeconds(1 / frequency);
        }
    }
}
