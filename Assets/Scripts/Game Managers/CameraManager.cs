using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraManager : MonoBehaviour {

    public new Camera camera { get; private set; }
    public static CameraManager main;

    Transform _target;
    public Transform target {
        get {
            return _target;
        }
        set {
            lastTarget = _target;
            _target = value;
            smooth = true;
        }
    }
    Transform lastTarget;
    bool smooth = false;

    public Transform defaultTarget;
    [Space]
    public bool trackPosition = true;
    public bool trackRotation = true;
    [Space]
    public float positionSpeed;
    public float rotationSpeed;
    [Space]
    public LayerMask sceneViewLayers = ~0;
    [Space]
    public Vector2 cameraFactor = new Vector2(80f, 45f);
    
    public System.Action<Transform, Transform> OnTransitionFinished;

    void Awake() {
        camera = GetComponent<Camera>();
        main = this;
        if (defaultTarget != null) {
            target = defaultTarget;
        }
    }

    void Start() {
        if(SceneSwitcher.scene == SceneSwitcher.Scene.MainMenu) {
            UpdateCameraFOV();
        }
    }

    void LateUpdate() {
        if (target != null) {
            if (smooth) {
                if (trackPosition) transform.position = Vector3.MoveTowards(transform.position, target.position, positionSpeed * Time.deltaTime);
                if (trackRotation) transform.rotation = Quaternion.RotateTowards(transform.rotation, target.rotation, rotationSpeed * Time.deltaTime);
            }
            else {
                if (trackPosition) transform.position = target.position;
                if (trackRotation) transform.rotation = target.rotation;
            }

            if (transform.position == target.position && transform.rotation == target.rotation && smooth == true) {
                smooth = false;
                if (OnTransitionFinished != null) {
                    OnTransitionFinished.Invoke(lastTarget, target);
                }
            }
        }
    }

    public void UpdateCameraFOV() {
        if (camera.aspect < 1.778f) {
            camera.fieldOfView = cameraFactor.x / Mathf.Sqrt(camera.aspect);
        }
        else {
            camera.fieldOfView = cameraFactor.y * Mathf.Sqrt(camera.aspect);
        }
    }

    public void SetTargetInstantly(Transform target) {
        this.target = target;
        transform.position = target.position;
        transform.rotation = target.rotation;
        smooth = false;
    }

    [ContextMenu("Set Visible Scene Layers")]
    public void SetVisibleSceneLayers() {
        #if UNITY_EDITOR
        UnityEditor.Tools.visibleLayers = sceneViewLayers;
        #endif
    }

    void OnValidate() {
        main = this;
        if (camera == null) {
            camera = GetComponent<Camera>();
        }
    }
}
