using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DanielSorensen.Utility;

public class GravitySource : MonoBehaviour {
    
    public float gravity = 9.81f;
    public float gravityReach = 15f;
    public float smoothTime = 5f;

    static List<PhysicsObject> attractedObjects;

    protected virtual void Start() {
        if (attractedObjects == null) {
            FindAttractedObjects();
        }
    }

    public void FindAttractedObjects() {
        InitAttractedObjects();
        
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag(GameManager.gravityTag)) {
            PhysicsObject physicsObject = obj.GetComponent<PhysicsObject>();
            if (physicsObject != null) {
                AddAttractedObject(physicsObject);
            }
        }
    }
    public static void AddAttractedObject(PhysicsObject physicsObject) {
        if (attractedObjects != null && !attractedObjects.Contains(physicsObject)) {
            attractedObjects.Add(physicsObject);
        }
    }
    public static void RemoveAttractedObject(PhysicsObject physicsObject) {
        if(attractedObjects != null && attractedObjects.Contains(physicsObject)) {
            attractedObjects.Remove(physicsObject);
        }
    }
    public static void InitAttractedObjects() {
        attractedObjects = new List<PhysicsObject>();
    }
    public static void ResetAttractedObjects() {
        attractedObjects = null;
    }

    void FixedUpdate() {
        for (int i = 0; i < attractedObjects.Count; i++) {
            PhysicsObject body = attractedObjects[i];
            if(body == null) {
                attractedObjects.Remove(body);
                continue;
            }
            if(!body.identity.localPlayerAuthority) {
                continue;
            }
            RaycastHit ground;
            bool inRange = Physics.Raycast(body.transform.position + body.transform.up * body.skinWidth, transform.position - body.transform.position, out ground, gravityReach, body.groundMask);

            if (inRange && ground.transform == transform && body.gameObject.tag == GameManager.gravityTag) {
                body.rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                Vector3 bodyUp = body.transform.up;
                Vector3 normal = ground.normal;
                body.rigidbody.AddForce(-normal * gravity, ForceMode.Acceleration);
                Quaternion targetRotation = Quaternion.FromToRotation(bodyUp, normal) * body.rigidbody.rotation;
                body.rigidbody.MoveRotation(Quaternion.RotateTowards(body.rigidbody.rotation, targetRotation, Time.deltaTime / smoothTime * 360));

                if(!body.gravity.Contains(this)) {
                    body.gravity.Add(this);
                }
            }
            else {
                if(body.gravity.Contains(this)) {
                    body.gravity.Remove(this);
                    body.rigidbody.constraints = RigidbodyConstraints.None;
                }
            }
        }
    }

    MeshFilter[] meshes = new MeshFilter[0];
    void OnDrawGizmosSelected() {
        if(meshes.Length == 0) {
            meshes = GetComponentsInChildren<MeshFilter>();
        }

        foreach (MeshFilter filter in meshes) {
            if (filter.sharedMesh != null) {
                Gizmos.color = Color.blue;
                Vector3 scale = transform.lossyScale.Abs() + (2 * gravityReach * Vector3.one).DivideVector(filter.sharedMesh.bounds.size);
                Gizmos.DrawWireMesh(filter.sharedMesh, filter.transform.position, transform.rotation, scale);
            }
        }
    }
}
