using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Inventory : MonoBehaviour {

    public GridLayoutGroup items;
    public InventoryItem itemTemplate;
    [Space]
    public int countX;
    public int countY;
    [Space]
    public Data data;
    [Space]
    public List<InventoryItem.Data> allItems;

    RectTransform rectTransform;
    List<InventoryItem> children;

    void Awake() {
        rectTransform = transform as RectTransform;
        InitInventory();
    }

    [ContextMenu("Init Inventory")]
    public void InitInventory() {
        children = new List<InventoryItem>();
        if(rectTransform == null) {
            rectTransform = transform as RectTransform;
        }
        Vector2 size = new Vector2(countX * items.cellSize.x + items.padding.left + items.padding.right + items.spacing.x * (countX - 1),
                                   countY * items.cellSize.y + items.padding.top + items.padding.bottom + items.spacing.y * (countY - 1));
        rectTransform.sizeDelta = size;
            for (int i = 0; i < items.transform.childCount; i++) {
                GameObject child = items.transform.GetChild(i).gameObject;
                if (child != itemTemplate.gameObject) {
                    DestroyImmediate(child);
                    i--;
                }
            }
        for (int x = 0; x < countX; x++) {
            for (int y = 0; y < countY; y++) {
                InventoryItem item = Instantiate(itemTemplate, items.transform);
                item.gameObject.SetActive(true);
                item.gameObject.name = "Item (empty)";
                children.Add(item);
            }
        }
    }

    public void OpenInventory(Data data) {
        this.data = data;
        for (int i = 0; i < children.Count; i++) {
            InventoryItem child = children[i];
            if (i < data.items.Length) {
                InventoryItem.Data item = data.items[i];
                if (item.type != InventoryItem.Type.Empty) {
                    child.SetItem(item);
                    continue;
                }
            }
            child.SetItem(InventoryItem.Data.Empty);
        }
    }

    [System.Serializable]
    public struct Data {
        public InventoryItem.Data[] items;

        public Data(List<InventoryItem.Data> items) {
            this.items = items.ToArray();
        }

        public static Data Empty = new Data(new List<InventoryItem.Data>());
    }
}
