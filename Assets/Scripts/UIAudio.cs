using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIAudio : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler {

    bool active = true;
    Selectable selectable;
    CanvasGroup group;

    void Start() {
        selectable = GetComponent<Selectable>();
        group = GetComponentInParent<CanvasGroup>();

    }
    void Update() {
        if (selectable != null) active = selectable.interactable;
        if (group != null && !group.interactable) active = false;
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (active) SoundManager.instance.ClickDown();
        else SoundManager.instance.PlayDisabledTone();
    }
    public void OnPointerUp(PointerEventData eventData) {
        if (active) SoundManager.instance.ClickUp();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (active) SoundManager.instance.Rollover();
    }
}
