using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerMotor : MonoBehaviour {

    [Header("Mouse")]
    public Vector2 mouseSensitivity = Vector2.one * 2f;
    public float mouseMax = 60;
    public float mouseMin = -60;
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float moveSmoothTime = 0.15f;
    [Space]
    public float jumpSpeed = 5f;
    [Space]
    public KeyCode jetpackDownKey = KeyCode.LeftShift;
    [Header("References")]
    public new Transform camera;

    float cameraRotation;
    Vector3 moveAmount;
    bool grounded;

    Vector3 smoothMoveVelocity;

    [HideInInspector]
    public Player player;
    new Rigidbody rigidbody;

    void Awake() {
        player = GetComponent<Player>();
        rigidbody = player.rigidbody;
    }

    void Update() {
        if (!GameManager.inventoryOpen && !GameManager.paused) {
            transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity.x);
            cameraRotation += Input.GetAxisRaw("Mouse Y") * mouseSensitivity.y;
            cameraRotation = Mathf.Clamp(cameraRotation, mouseMin, mouseMax);
            camera.localEulerAngles = Vector3.left * cameraRotation;

            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            Vector3 moveDir = new Vector3(input.x, 0, input.y);

            if (!grounded && player.gravity.Count == 0) {
                if (Input.GetButton("Jump")) {
                    moveDir.y = 1;
                }
                else if (Input.GetKey(jetpackDownKey)) {
                    moveDir.y = -1;
                }
            }

            moveDir.Normalize();
            Vector3 targetMoveAmount = moveDir * moveSpeed;
            moveAmount = Vector3.SmoothDamp(moveAmount, targetMoveAmount, ref smoothMoveVelocity, moveSmoothTime);

            if (grounded && Input.GetButtonDown("Jump")) {
                rigidbody.AddForce(transform.up * jumpSpeed, ForceMode.VelocityChange);
            }
        }
    }

    void FixedUpdate() {
        Vector3 move = transform.TransformDirection(moveAmount) * Time.fixedDeltaTime;
        rigidbody.MovePosition(rigidbody.position + move);

        grounded = player.GetGround();
    }
}
