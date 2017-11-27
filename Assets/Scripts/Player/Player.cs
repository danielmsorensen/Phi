using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

using DanielSorensen.Utility;

[RequireComponent(typeof(PlayerMotor))]
public class Player : PhysicsObject {

    [Header("Interaction")]
    public float interactDistance;
    public LayerMask interactLayer;
    [Header("References")]
    public Transform namePlateHolder;
    public TMP_Text namePlate;
    public GameObject playerView;

    [HideInInspector]
    public Inventory.Data inventory;
    [HideInInspector]
    public Interactable interactable;

    [HideInInspector]
    public PlayerMotor motor;

    [HideInInspector]
    [SyncVar]
    public string playerName;

    protected override void Awake() {
        motor = GetComponent<PlayerMotor>();

        base.Awake();
    }

    void Start() {
        GameManager.SendToMainMenu();
        if (isLocalPlayer) {
            MultiplayerManager.localPlayer = this;
            if(MultiplayerManager.onLocalPlayerSpawned != null) {
                MultiplayerManager.onLocalPlayerSpawned.Invoke(this);
            }

            motor.enabled = true;

            CameraManager.main.SetTargetInstantly(motor.camera);
            CameraManager.main.OnTransitionFinished += ((Transform lastTarget, Transform target) => {
                if (target == motor.camera) {
                    playerView.SetActive(true);
                }
            });
            gameObject.tag = GameManager.gravityTag;
            GravitySource.AddAttractedObject(this);
            rigidbody.useGravity = false;

            foreach (MeshRenderer mr in renderer) {
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }

            CmdSetData(GameManager.GetOrCreatePlayer(AccountManager.Username));

            GameManager.main.dustSpawner.SpawnField();
        }
        else {
            motor.camera.gameObject.SetActive(false);
            motor.enabled = false;
            SetName(playerName);
            rigidbody.isKinematic = true; ;
        }
    }

    void Update() {
        #region Interacting
        if (interactable == null) {
            Crosshair.active.Normal();
            RaycastHit hit;
            Debug.DrawRay(motor.camera.transform.position, motor.camera.forward * interactDistance, Color.red, Time.deltaTime);
            if (Physics.Raycast(motor.camera.transform.position, motor.camera.forward, out hit, interactDistance, interactLayer)) {
                Interactable i = hit.transform.GetComponentInParent<Interactable>();
                if (i != null) {
                    Crosshair.active.Interact();
                    if (Input.GetMouseButtonDown(1)) {
                        i.Interact(this);
                    }
                }
            }
        }
        else {
            if(Input.GetMouseButtonDown(1)) {
                interactable.UnInteract();
            }
        }
        #endregion
    }

    public void SetName(string username) {
        gameObject.name = username;
        playerName = username;

        if (namePlate != null) {
            namePlate.text = username;
            namePlate.transform.SetParent(namePlateHolder, false);
        }

        if (!MultiplayerManager.players.ContainsKey(playerName)) {
            MultiplayerManager.players.Add(playerName.ToLower(), this);
        }
    }

    [Command]
    public void CmdSetData(Data data) {
        RpcSetData(data);
    }

    [ClientRpc]
    public void RpcSetData(Data data) {
        SetName(data.username);

        transform.position = data.position;
        transform.rotation = data.rotation;
        inventory = data.inventory;
    }

    #region Disconnecting
    [ClientRpc]
    public void RpcDisconnect(string username) {
        print("Disconnecting " + username);
        if (MultiplayerManager.players.ContainsKey(username)) {
            Player player = MultiplayerManager.players[username];
            if (MultiplayerManager.networkMode == MultiplayerManager.NetworkMode.Hosted) {
                GameManager.SavePlayer(player.Serialize());
                GameManager.SaveWorld(GameManager.currentWorld);
            }
            if (player.isLocalPlayer) {
                if (MultiplayerManager.onLocalPlayerLeave != null) {
                    MultiplayerManager.onLocalPlayerLeave.Invoke();
                }
                MultiplayerManager.localPlayer = null;
            }
            MultiplayerManager.players.Remove(username);
        }
    }
    
    [Command]
    public void CmdDisconnect() {
        print("Disconnecting from server");
        if (MultiplayerManager.networkMode == MultiplayerManager.NetworkMode.Hosted || MultiplayerManager.networkMode == MultiplayerManager.NetworkMode.Solo) {
            foreach (Player player in MultiplayerManager.players.Values) {
                GameManager.SavePlayer(player.Serialize());
            }
            GameManager.SaveWorld(GameManager.currentWorld);
            MultiplayerManager.players.Clear();
            MultiplayerManager.localPlayer = null;
        }
        else if (MultiplayerManager.networkMode == MultiplayerManager.NetworkMode.Client) {
            RpcDisconnect(MultiplayerManager.localPlayer.playerName);
        }
    }

    void OnApplicationQuit() {
        CmdDisconnect();
    }
    #endregion

    #region Dynamic RPC
    public System.Action<string, string> onRpc;

    public void RPC(string rpcName, string arg) {
        CmdRPC(rpcName, arg);
    }

    [Command]
    void CmdRPC(string rpcName, string arg) {
        RpcRPC(rpcName, arg);
    }

    [ClientRpc]
    void RpcRPC(string rpcName, string arg) {
        if(onRpc != null) {
            onRpc.Invoke(rpcName, arg);
        }
    }
    #endregion

    public Data Serialize() {
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;
        if(interactable != null && interactable is Ship) {
            Ship ship = (Ship)interactable;
            position = ship.transform.TransformPoint(ship.playerSpawnPosition);
            rotation = ship.transform.rotation;
        }
        return new Data(playerName.ToLower(), position, rotation, inventory);
    }

    void OnDrawGizmos() {
        if (motor != null) {
            if (motor.camera != null) {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(motor.camera.transform.position, interactDistance);
            }
        }
        else {
            motor = GetComponent<PlayerMotor>();
        }
    }

    [System.Serializable]
    public struct Data {
        public string username;

        public GameManager.SerializableVector3 position;
        public GameManager.SerializableQuaternion rotation;

        public Inventory.Data inventory;

        public Data(string username, Vector3 position, Quaternion rotation, Inventory.Data inventory) {
            this.username = username.ToLower();

            this.position = position;
            this.rotation = rotation;

            this.inventory = inventory;
        }
    }
}
