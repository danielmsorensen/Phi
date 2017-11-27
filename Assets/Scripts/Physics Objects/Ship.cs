using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using DanielSorensen.Utility;

public class Ship : Interactable {

    [Space]
    public Movement movement;
    public Thrusters thrusters;
    [Header("Misc")]
    public new Transform camera;
    public Vector3 playerSpawnPosition;
    public LayerMask crosshairHitMask;
    public Transform namePlateHolder;

    Vector3 input;
    Vector2 mouseInput;
    float rollInput;

    Vector3 moveForce;

    Vector3 cameraPosition;
    Quaternion cameraRotation;

    protected override void Awake() {
        base.Awake();

        cameraPosition = camera.localPosition;
        cameraRotation = camera.localRotation;
    }

    void Start() {
        thrusters.ship = this;
        CameraManager.main.OnTransitionFinished += ((Transform lastTarget, Transform target) => {
            if (target == camera) {
                Crosshair.active.neutralPosition = CameraManager.main.camera.WorldToScreenPoint(transform.position + transform.forward * 10000f);
            }
        });
    }

    void Update() {
        if (player != null && player.identity.localPlayerAuthority) {
            #region Input
            input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            if (Input.GetButton("Jump")) input.y = 1;
            else if (Input.GetKey(movement.downKey)) input.y = -1;

            moveForce.x = movement.sideForce * input.x;
            moveForce.y = ((input.y > 0) ? movement.upForce : movement.downForce) * input.y;
            moveForce.z = ((input.z > 0) ? movement.frontForce : movement.downForce) * input.z;

            rollInput = 0;
            if (Input.GetKey(movement.rollCW)) rollInput = -1;
            else if (Input.GetKey(movement.rollACW)) rollInput = 1;
            #endregion

            #region Camera Pan
            if(Input.GetKey(movement.cameraUnlockKey)) {
                camera.transform.RotateAround(transform.position, camera.up, mouseInput.x);
                camera.transform.RotateAround(transform.position, camera.right, -mouseInput.y);
            }
            if(Input.GetKeyUp(movement.cameraUnlockKey)) {
                camera.transform.localPosition = cameraPosition;
                camera.transform.localRotation = cameraRotation;
            }
            #endregion

            #region Camera Unlock
            if (!Input.GetKey(movement.cameraUnlockKey)) {
                Crosshair.active.Enable();
                RaycastHit hit;
                if(Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, crosshairHitMask)) {
                    if(hit.transform != null) {
                        Crosshair.active.MoveOverWorldPoint(hit.point);
                    }
                    else {
                        Crosshair.active.ResetCrosshairPosition();
                    }
                }
                else {
                    Crosshair.active.ResetCrosshairPosition();
                }
            }
            else {
                Crosshair.active.Disable();
            }
            #endregion

            #region Thrusters
            thrusters.DisableAll();

            if (input.x > 0) thrusters.EnableThrusters("left");
            else if (input.x < 0) thrusters.EnableThrusters("right");

            if (input.y > 0) thrusters.EnableThrusters("bottom");
            else if (input.y < 0) thrusters.EnableThrusters("top");

            if (input.z > 0) thrusters.EnableThrusters("back");
            else if (input.z < 0) thrusters.EnableThrusters("front");

            if (rollInput > 0) thrusters.EnableThrusters("spinACW");
            else if (rollInput < 0) thrusters.EnableThrusters("spinCW");
            #endregion
        }
        else {
            Crosshair.active.CentreCrosshair();
        }
    }

    void FixedUpdate() {
        if (player != null && player.identity.localPlayerAuthority) {
            if (input != Vector3.zero) rigidbody.AddRelativeForce(moveForce, ForceMode.Acceleration);
            if (rollInput != 0) rigidbody.AddRelativeTorque(Vector3.forward * movement.rollForce * rollInput, ForceMode.Acceleration);

            Vector3 localRotation = movement.mouseSensitivity.MultiplyVector(new Vector2(-mouseInput.y, mouseInput.x)) * movement.mouseAcceleration;
            if(!Input.GetKey(movement.cameraUnlockKey)) rigidbody.MoveRotation(rigidbody.rotation * Quaternion.Euler(localRotation));
        }
    }

    protected override void OnInteract(Player player) {
        player.motor.enabled = false;
        player.MakeInvisible();
        player.namePlate.transform.SetParent(namePlateHolder, false);

        if (player.isLocalPlayer) {
            CameraManager.main.target = camera;
            gameObject.tag = "Untagged";

            player.playerView.SetActive(false);
        }
    }
    protected override void OnUnInteract(Player player) {
        player.motor.enabled = true;
        player.MakeVisible();
        player.namePlate.transform.SetParent(player.namePlateHolder, false);

        if (player.isLocalPlayer) {
            CameraManager.main.target = player.motor.camera;
            gameObject.tag = GameManager.gravityTag;

            player.rigidbody.position = transform.TransformPoint(playerSpawnPosition);
            player.rigidbody.rotation = rigidbody.rotation;
        }
    }

    protected override void OnDrawGizmosSelected() {
        base.OnDrawGizmosSelected();

        Utilities.DrawCrosses(transform.TransformPoint(playerSpawnPosition), 1, Color.green);
    }

    #region Local Player Tracking
    void OnLocalPlayerSpawned(Player player) {
        MultiplayerManager.localPlayer.onRpc += thrusters.OnRPC;
    }

    void OnLocalPlayerLeave() {
        MultiplayerManager.localPlayer.onRpc -= thrusters.OnRPC;
    }

    void OnEnable() {
        MultiplayerManager.onLocalPlayerSpawned += OnLocalPlayerSpawned;
        MultiplayerManager.onLocalPlayerLeave += OnLocalPlayerLeave;
    }
    void OnDisable() {
        MultiplayerManager.onLocalPlayerSpawned -= OnLocalPlayerSpawned;
        MultiplayerManager.onLocalPlayerLeave -= OnLocalPlayerLeave;
    }
    #endregion

    [System.Serializable]
    public struct Movement {
        [Header("Target Speeds")]
        public float frontForce;
        public float backForce;
        public float sideForce;
        public float upForce;
        public float downForce;
        [Space]
        public float rollForce;
        [Header("Mouse Input")]
        public Vector2 mouseSensitivity;
        public float mouseAcceleration;
        [Header("Controls")]
        public KeyCode downKey;
        public KeyCode rollCW;
        public KeyCode rollACW;
        [Space]
        public KeyCode cameraUnlockKey;
    }

    [System.Serializable]
    public struct Thrusters {
        public float enabledRate;
        public float disabledRate;
        [Space]
        public ParticleSystem[] back;
        public ParticleSystem[] front;
        public ParticleSystem[] left;
        public ParticleSystem[] right;
        public ParticleSystem[] bottom;
        public ParticleSystem[] top;
        [Space]
        public ParticleSystem[] spinCW;
        public ParticleSystem[] spinACW;

        [HideInInspector]
        public Ship ship;

        public static readonly string[] thrusterSystems = new string[] { "back", "front", "left", "right", "bottom", "top", "spinCW", "spinACW" };

        public ParticleSystem[] GetThrusterSystems(string thrusterName) {
            ParticleSystem[] system;

            switch (thrusterName) {
                default:
                case ("back"):
                    system = back;
                    break;
                case ("front"):
                    system = front;
                    break;
                case ("left"):
                    system = left;
                    break;
                case ("right"):
                    system = right;
                    break;
                case ("bottom"):
                    system = bottom;
                    break;
                case ("top"):
                    system = top;
                    break;

                case ("spinCW"):
                    system = spinCW;
                    break;
                case ("spinACW"):
                    system = spinACW;
                    break;
            }

            return system;
        }

        public void SetThrusters(float rate, string thrusters) {
            if (thrusters == "all") {
                foreach (string thruster in thrusterSystems) {
                    SetThrusters(rate, thruster);
                }
            }
            else {
                foreach (ParticleSystem thruster in GetThrusterSystems(thrusters)) {
                    ParticleSystem.EmissionModule emission = thruster.emission;
                    emission.rateOverTime = rate;
                }
            }
        }
        public void EnableThrusters(string thrusters) {
            MultiplayerManager.localPlayer.RPC("SetThrusters", thrusters + "," + enabledRate);
        }
        public void DisableThrusters(string thrusters) {
            MultiplayerManager.localPlayer.RPC("SetThrusters", thrusters + "," + disabledRate);
        }

        public void DisableAll() {
            MultiplayerManager.localPlayer.RPC("SetThrusters", "all," + disabledRate);
        }

        public void OnRPC(string rpcName, string arg) {
            if(rpcName == "SetThrusters") {
                string[] args = arg.Split(',');
                string thrusters = args[0];
                float rate = float.Parse(args[1]);

                SetThrusters(rate, thrusters);
            }
        }
    }
}
