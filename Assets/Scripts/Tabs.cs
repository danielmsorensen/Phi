using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tabs : MonoBehaviour {

    public float selectedAlpha;
    public float normalAlpha;

    public List<Image> tabs;

    void Awake() {
        SetTab(0);
    }

    public void SetTab(Transform tab) {
        SetTab(tab.GetSiblingIndex());
    }
    public void SetTab(int index) {
        for (int i = 0; i < tabs.Count; i++) {
            Image child = tabs[i];
            Color colour = child.color;
            if (i == index) {
                colour.a = selectedAlpha;
            }
            else {
                colour.a = normalAlpha;
            }
            child.color = colour;
        }
    }
}
