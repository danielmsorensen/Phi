using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsObject : NetworkBehaviour {
    
    Rigidbody _rigidbody;
    public new Rigidbody rigidbody {
        get {
            if (_rigidbody == null) {
                _rigidbody = GetComponent<Rigidbody>();
            }
            return _rigidbody;
        }
    }
    Collider[] _collider;
    public new Collider[] collider {
        get {
            if (_collider == null) {
                _collider = GetComponentsInChildren<Collider>();
            }
            return _collider;
        }
    }

    MeshRenderer[] _renderer;
    public new MeshRenderer[] renderer {
        get {
            if(_renderer == null) {
                _renderer = GetComponentsInChildren<MeshRenderer>();
            }
            return _renderer;
        }
    }
    
    public LayerMask groundMask;
    [Tooltip("The distance to inset the ground check ray from")]
    public float skinWidth = 0.01f;
    [Tooltip("The distance beneath the object to check for the ground in")]
    public float groundCheck = 0.01f;
    public RaycastHit ground;

    [HideInInspector]
    public List<GravitySource> gravity = new List<GravitySource>();

    NetworkIdentity _identity;
    public NetworkIdentity identity {
        get {
            if(_identity == null) {
                _identity = GetComponent<NetworkIdentity>();
            }
            return _identity;
        }
    }

    int layer;

    protected virtual void Awake() {
        rigidbody.useGravity = false;
        _collider = GetComponentsInChildren<Collider>();
        _renderer = GetComponentsInChildren<MeshRenderer>();
        _identity = GetComponent<NetworkIdentity>();

        layer = gameObject.layer;
    }

    /// <summary>
    /// Checks whether or not this object is touching the ground and stores it in the "ground" property
    /// </summary>
    /// <param name="groundCheck">The distance beneath the object to check for the ground in</param>
    /// <returns>Whether or not this object is grounded</returns>
    public bool CheckGround(float groundCheck) {
        return Physics.Raycast(transform.position + transform.up * skinWidth, -transform.up, out ground, groundCheck + skinWidth, groundMask);
    }
    /// <summary>
    /// Checks whether or not this object is touching the ground and stores it in the "ground" property
    /// </summary>
    /// <returns>Whether or not this object is groundewd</returns>
    public bool GetGround() {
        return CheckGround(groundCheck);
    }
    
    public void MakeVisible() {
        foreach(MeshRenderer mr in renderer) {
            mr.gameObject.layer = layer;
        }
    }
    public void MakeInvisible() {
        foreach(MeshRenderer mr in renderer) {
            mr.gameObject.layer = GameManager.invisibleLayer;
        }
    }

    protected virtual void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + transform.up * skinWidth, transform.position - transform.up * groundCheck);
    }
}
