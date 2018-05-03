using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TrackSelectTile : MonoBehaviour {

    public Track track;

    public Image image;

    public Text trackName;

    void Start() {
        UpdateUI();

        var trackSelectController = GetComponentInParent<TrackSelectController>();
        trackSelectController.selectedTrack
            .Subscribe(selected => {
                if (selected == track) {
                    transform.localScale = Vector3.one * 1.1f;
                } else {
                    transform.localScale = Vector3.one;
                }
            }).AddTo(this);

        trackSelectController.selectionConfirmed
            .Where(_ => trackSelectController.selectedTrack.Value == track)
            .Subscribe(confirmed => {
                if (confirmed) {
                    transform.localScale = Vector3.one * 1.2f;
                } else {
                    transform.localScale = Vector3.one * 1.1f;
                }
            })
            .AddTo(this);
    }

    void OnValidate() {
        UpdateUI();
    }

    void UpdateUI() {
        if (track != null) {
            if (image != null) {
                image.sprite = track.image;
            }

            if (trackName != null) {
                trackName.text = track.trackName;
            }
        }
    }

}