using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour {

    void LateUpdate() {
        transform.rotation = Quaternion.LookRotation(transform.position - CameraManager.main.camera.transform.position, CameraManager.main.camera.transform.up);
    }
}
