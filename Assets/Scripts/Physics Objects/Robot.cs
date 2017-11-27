using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot : PhysicsObject {

    [Space]
    public PhysicsObject target;
    [Space]
    public float moveSpeed = 5f;
    public float targetMaxDistance = 10f;
    public float targetMinDistance = 5f;

    Vector3 targetDirection;
    Vector3 moveAmount;
    bool moving;

    float sqrMax, sqrMin;

    protected override void Awake() {
        base.Awake();

        sqrMax = Mathf.Pow(targetMaxDistance, 2);
        sqrMin = Mathf.Pow(targetMinDistance, 2);
    }

    void Update() {
        targetDirection = target.transform.position - transform.position;
        float sqrTargetDistance = targetDirection.sqrMagnitude;
        targetDirection.Normalize();

        if (sqrTargetDistance > sqrMax) {
            moving = true;
        }
        else if(sqrTargetDistance <= sqrMin) {
            moving = false;
        }
        
        if (moving) {
            moveAmount = targetDirection * moveSpeed * Time.deltaTime;
        }
        else {
            moveAmount = Vector3.zero;
        }
        
        if(GetGround()) {
                        
        }
        Debug.DrawLine(transform.position, targetDirection * 2, Color.blue);
    }

    void FixedUpdate() {
        rigidbody.MovePosition(rigidbody.position + moveAmount);

        Vector3 lookDirection = transform.InverseTransformDirection(targetDirection);
        lookDirection.y = 0;
        lookDirection = transform.TransformDirection(lookDirection);
        if(lookDirection != Vector3.zero) rigidbody.MoveRotation(Quaternion.LookRotation(lookDirection, ground.normal));
    }

    protected override void OnDrawGizmosSelected() {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, targetMaxDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, targetMinDistance);
    }
}
