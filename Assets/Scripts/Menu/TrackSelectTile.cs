using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TrackSelectTile : MonoBehaviour {

    public Track track;

    public Image image;

    public Text trackName;

    private ActiveInactiveAnimation activeInactiveAnimation;

    void Start() {
        activeInactiveAnimation = GetComponent<ActiveInactiveAnimation>();
        UpdateUI();

        var trackSelectController = GetComponentInParent<TrackSelectController>();
        trackSelectController.selectedTrack
            .Subscribe(selected => {
                if (selected == track) {
                    activeInactiveAnimation.Activate();
                } else {
                    activeInactiveAnimation.Deactivate();
                }
            }).AddTo(this);

        trackSelectController.selectionConfirmed
            .Where(_ => trackSelectController.selectedTrack.Value == track)
            .Subscribe(confirmed => {
                // ???
                // if (confirmed) {
                //     activeInactiveAnimation.Activate();
                // } else {
                //     activeInactiveAnimation.Deactivate();
                // }
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