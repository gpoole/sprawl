using System.Collections.Generic;
using UnityEngine;

public class PersistentSingleton : MonoBehaviour {

    private static Dictionary<string, GameObject> instanceList = new Dictionary<string, GameObject>();

    void Awake() {
        if (instanceList.ContainsKey(gameObject.name)) {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(this);
        instanceList.Add(gameObject.name, gameObject);
    }

}