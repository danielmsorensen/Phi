using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour {

    public Image icon;

    public enum Type { Empty, Placeable, Consumable, Weapon, Tool };

    public Data data;

	public void SetItem(Data data) {
        if(data.type != Type.Empty) {
            icon.color = Color.white;
            icon.sprite = Resources.Load<Sprite>("Sprites/" + data.icon);
        }
        else {
            icon.color = Color.clear;
        }
    }

    [System.Serializable]
    public struct Data {
        public string name;
        public Type type;
        public string icon;
        [Header("Properties")]
        public Block block;
        public Consumable consumable;
        public Weapon weapon;
        public Tool tool;

        public Data(string name, Type type, Sprite icon, Block block, Consumable consumable, Weapon weapon, Tool tool) {
            this.name = name;
            this.type = type;
            this.icon = icon.name;

            this.block = block;
            this.consumable = consumable;
            this.weapon = weapon;
            this.tool = tool;
        }

        public override bool Equals(object obj) {
            if (obj is Data) {
                return this == (Data)obj;
            }
            else {
                return false;
            }
        }

        public static bool operator ==(Data d1, Data d2) {
            if(d1.type == d2.type) {
                switch(d1.type) {
                    case (Type.Empty):
                        return true;
                    case (Type.Placeable):
                        return d1.block == d2.block;
                    case (Type.Consumable):
                        return d1.consumable == d2.consumable;
                    case (Type.Weapon):
                        return d1.weapon == d2.weapon;
                    case (Type.Tool):
                        return d1.tool == d2.tool;
                }
            }
            return false;
        }
        public static bool operator !=(Data d1, Data d2) {
            return !(d1 == d2);
        }

        public override int GetHashCode() {
            return ToString().GetHashCode();
        }
        public override string ToString() {
            return "InventoryItem.Data: " + name + ", " + type + ", " + icon + ", " + block.name + ", " + consumable + ", " + weapon + ", " + tool;
        }

        public static Data Empty = new Data("", Type.Empty, null, null, null, null, null);
    }
}
