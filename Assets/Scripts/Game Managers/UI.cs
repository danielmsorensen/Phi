using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UI : MonoBehaviour {

    public List<Page> pages = new List<Page>();
    public int pageIndex { get; protected set; }
    
    public List<TextReference> textReferences = new List<TextReference>();
    Dictionary<string, Text> texts = new Dictionary<string, Text>();

    void Awake() {
        UpdateTextReferences();

        for (int i = 0; i < pages.Count; i++) {
            if (i == 0) OpenPage(0, true);
            else ClosePage(i, true);
        }

        pageIndex = 0;
    }

    public void UpdateTextReferences() {
        texts.Clear();
        foreach(TextReference tr in textReferences) {
            texts.Add(tr.name, tr.text);
        }
    }

    public void SetText(string text, string value) {
        texts[text].text = value;
    }

    public void SetPage(string name) {
        SetPage(name, false);
    }
    public void SetPage(string name, bool instantly) {
        SetPage(pages.Select((Page p) => p.name).ToList().IndexOf(name), instantly);
        Debug.Log("Set page " + name);
    }
    public void SetPage(int index) {
        SetPage(index, false);
    }
    public void SetPage(int index, bool instantly) {
        ClosePage(pageIndex, instantly);
        OpenPage(index, instantly);
        Debug.Log("Set page " + index);

        pageIndex = index;
    }

    public void OpenPage(int index) {
        OpenPage(index, false);
    }
    public void OpenPage(int index, bool instantly) {
        if (pages[index].enableGameObject) pages[index].gameObect.SetActive(true);
        if (pages[index].animate) {
            pages[index].animator.gameObject.SetActive(true);

            if (instantly) {
                pages[index].animator.Play(pages[index].openTrigger);
            }
            else {
                pages[index].animator.ResetTrigger(pages[index].closeTrigger);
                pages[index].animator.ResetTrigger(pages[index].openTrigger);
                pages[index].animator.SetTrigger(pages[index].openTrigger);
            }
        }
    }

    public void ClosePage(int index) {
        ClosePage(index, false);
    }
    public void ClosePage(int index, bool instantly) {
        if (pages[index].disableGameObject) pages[index].gameObect.SetActive(false);
        if (pages[index].animate) {
            pages[index].animator.gameObject.SetActive(true);

            if (instantly) {
                pages[index].animator.Play(pages[index].closeTrigger);
            }
            else {
                pages[index].animator.ResetTrigger(pages[index].openTrigger);
                pages[index].animator.ResetTrigger(pages[index].closeTrigger);
                pages[index].animator.SetTrigger(pages[index].closeTrigger);
            }
        }
    }

    [System.Serializable]
    public struct Page {
        public string name;
        [Space]
        public GameObject gameObect;
        public Animator animator;
        [Space]
        public bool enableGameObject;
        public bool disableGameObject;
        [Space]
        public bool animate;
        public string openTrigger;
        public string closeTrigger;
    }

    [System.Serializable]
    public struct TextReference {
        public string name;
        public Text text;

        public TextReference(string name, Text text) {
            this.name = name;
            this.text = text;
        }
    }
}
