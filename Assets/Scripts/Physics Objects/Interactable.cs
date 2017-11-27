using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using DanielSorensen.Utility;

public class Interactable : PhysicsObject {
    
    Player _player;
    public Player player {
        get {
            return _player;
        }
        set {
            if(OnPlayerChange != null) {
                OnPlayerChange.Invoke(_player, value);
            }
            _player = value;
        }
    }

    public bool canPlayerMove;

    public System.Action<Player, Player> OnPlayerChange;

    public void Interact(Player player) {
        CmdInteract(player.playerName);
    }
    public void UnInteract() {
        CmdUnInteract();
    }

    [Command]
    public void CmdInteract(string username) {
        player = MultiplayerManager.players[username];

        identity.AssignClientAuthority(player.identity.connectionToClient);

        RpcInteract(username);
    }
    [ClientRpc]
    public void RpcInteract(string username) {
        player = MultiplayerManager.players[username];
        player.interactable = this;
        OnInteract(player);
    }
    [Command]
    public void CmdUnInteract() {
        RpcUnInteract();
    }
    [ClientRpc]
    public void RpcUnInteract() {
        OnUnInteract(player);
        player = null;
    }

    protected virtual void OnInteract(Player player) { }
    protected virtual void OnUnInteract(Player player) { }
}
