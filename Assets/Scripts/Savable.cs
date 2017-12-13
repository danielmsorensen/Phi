using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Savable : MonoBehaviour{

    public string resourcePath;
    public Dictionary<string, string> data;
    public System.Action OnDeserialized;
    
    public string Serialize() {
        return JsonUtility.ToJson(new Data(resourcePath, transform.position, transform.rotation, GameManager.currentWorld.realm, data), false);        
    }

    public static Savable Deserialize(string json) {
        Data data = JsonUtility.FromJson<Data>(json);
        Savable s = Instantiate(Resources.Load<Savable>(data.resourcePath), data.position, data.rotation);

        s.resourcePath = data.resourcePath;
        s.data = data.data;

        if (s.OnDeserialized != null) {
            s.OnDeserialized.Invoke();
        }

        return s;
    }

    public static GameManager.World.Realm GetRealm(string json) {
        Data data = JsonUtility.FromJson<Data>(json);
        return data.realm;
    }

    [System.Serializable]
    struct Data {
        public string resourcePath;

        public GameManager.SerializableVector3 position;
        public GameManager.SerializableQuaternion rotation;

        public GameManager.World.Realm realm;

        public Dictionary<string, string> data;

        public Data (string resourcePath, GameManager.SerializableVector3 position, GameManager.SerializableQuaternion rotation, GameManager.World.Realm realm, Dictionary<string, string> data) {
            this.resourcePath = resourcePath;

            this.position = position;
            this.rotation = rotation;

            this.realm = realm;

            this.data = data;
        }

        public Data(string resourcePath, GameManager.SerializableVector3 position, GameManager.SerializableQuaternion rotation, GameManager.World.Realm realm, params KeyValuePair<string, string>[] data) {
            this.resourcePath = resourcePath;

            this.position = position;
            this.rotation = rotation;

            this.realm = realm;

            this.data = new Dictionary<string, string>();
            foreach(KeyValuePair<string, string> pair in data) {
                this.data.Add(pair.Key, pair.Value);
            }
        }
    }
}
