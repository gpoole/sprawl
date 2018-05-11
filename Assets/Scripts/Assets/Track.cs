using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu]
public class Track : ScriptableObject {

    [Serializable]
    public class TrackSetting {
        public string name;
        public string value;
    }

    public string trackName;

    public string sceneName;

    public Sprite image;

    public TrackSetting[] settings = new TrackSetting[0];

}