using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour {

    public static Crosshair active;
    public Image crosshair;
    [Header("Sprites")]
    public Sprite normal;
    public Sprite interacting;
    [Header("Movement")]
    public float smoothTime = 0.5f;
    public bool lockX = true;
    public bool lockY = false;

    public Vector2 neutralPosition;
    Vector2 velocity;

    void Awake() {
        active = this;
    }
    
    #region Visual
    public void Normal() {
        crosshair.sprite = normal;
    }
    public void Interact() {
        crosshair.sprite = interacting;
    }

    public void Enable() {
        crosshair.color = Color.white;
    }
    public void Disable() {
        crosshair.color = Color.clear;
    }
    #endregion

    #region Positioning
    public void MoveCrosshair(Vector2 screenPoint) {
        if (lockX) screenPoint.x = transform.position.x;
        if (lockY) screenPoint.y = transform.position.y;
        transform.position = screenPoint;
    }
    public void MoveOverWorldPoint(Vector3 worldPoint) {
        MoveCrosshair(CameraManager.main.camera.WorldToScreenPoint(worldPoint));
    }
    public void ResetCrosshairPosition() {
        MoveCrosshair(neutralPosition);
    }
    public void CentreCrosshair() {
        MoveCrosshair(CameraManager.main.camera.ViewportToScreenPoint(0.5f * Vector3.one));
    }
    #endregion
}