using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DanielSorensen.Utility {
    public static class Utilities {

        public static Vector3 MultiplyVector(this Vector3 v1, Vector3 v2) {
            return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
        }
        public static Vector2 MultiplyVector(this Vector2 v1, Vector2 v2) {
            return ((Vector3)v1).MultiplyVector(v2);
        }

        public static Vector3 DivideVector(this Vector3 v1, Vector3 v2) {
            return new Vector3(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z);
        }
        public static Vector2 DivideVector(this Vector2 v1, Vector2 v2) {
            return ((Vector3)v1).DivideVector(v2);
        }

        public static Vector3 Abs(this Vector3 v) {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }
        public static Vector2 Abs(this Vector2 v) {
            return ((Vector3)v).Abs();
        }

        public static void SetLayerRecursively(this GameObject gameObject, int layer, bool allLayers=true) {
            gameObject.layer = layer;
            foreach(Transform child in gameObject.transform) {
                child.gameObject.layer = layer;
                child.gameObject.SetLayerRecursively(layer);
            }
        }

        public static void DrawCrosses(Vector3 position, float crossLength, Color color) {
            Gizmos.color = color;
            Gizmos.DrawLine(position + Vector3.right * crossLength / 2, position + Vector3.left * crossLength / 2);
            Gizmos.DrawLine(position + Vector3.up * crossLength / 2, position + Vector3.down * crossLength / 2);
            Gizmos.DrawLine(position + Vector3.forward * crossLength / 2, position + Vector3.back * crossLength / 2);
        }

        public static TKey GetKey<TKey, TValue>(this Dictionary<TKey, TValue> dict, TValue value) {
            return dict.Keys.ToArray()[dict.Values.ToList().IndexOf(value)];
        }
    }
}
