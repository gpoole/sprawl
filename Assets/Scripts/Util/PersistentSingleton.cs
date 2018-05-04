using System.Collections.Generic;
using UnityEngine;

public class PersistentSingleton<T> : MonoBehaviour {

    public static T Instance {
        get;
        private set;
    }

    public void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(this);
        Instance = GetComponent<T>();
    }

}