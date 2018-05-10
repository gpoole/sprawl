using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRaceUI : MonoBehaviour {

	public PlayerState playerState;

	public GameObject introText;

	public Text lapText;

	public Text lapTimeText;

	public Animator victoryMessage;

	public Animator loserMessage;

	public RectTransform layout;

	public Transform lapRows;

	public GameObject lapRowPrefab;

	void Start() {
		if (!playerState) {
			Debug.Log("playerState not available, disabling UI");
			enabled = false;
			return;
		}

		playerState.lap.Select(lap => String.Format("{0}/{1}", lap, 3)).SubscribeToText(lapText);

		var rankAnimations = transform.Find("Rank").GetComponentsInChildren<Animator>();
		playerState.rank
			.Where(rank => rank > 0 && rank <= rankAnimations.Length)
			.Select(rank => rankAnimations[rank - 1])
			.Subscribe(animation => animation.SetBool("Active", true))
			.AddTo(this);
		playerState.rank
			.SelectMany(rank => rankAnimations.Where((_, index) => index != (rank - 1)))
			.Subscribe(animation => animation.SetBool("Active", false))
			.AddTo(this);

		playerState.lapTimes
			.ObserveReplace()
			.Where(ev => ev.Index == playerState.lap.Value - 1)
			.Select(ev => FormatLapTime(ev.NewValue))
			.SubscribeToText(lapTimeText)
			.AddTo(this);

		playerState.lapTimes
			.ObserveAdd()
			.Where(ev => ev.Index > 0)
			.Subscribe(ev => {
				var newRow = Instantiate(lapRowPrefab, lapRows);
				newRow.GetComponentInChildren<Text>().text = FormatLapTime(playerState.lapTimes[ev.Index - 1]);
			})
			.AddTo(this);

		playerState.mode
			.Where(playerMode => playerMode == PlayerState.PlayerMode.Finished)
			.Subscribe(_ => {
				if (playerState.rank.Value == 1) {
					victoryMessage.SetTrigger("Active");
				} else {
					loserMessage.SetTrigger("Active");
				}
			})
			.AddTo(this);
	}

	private static string FormatLapTime(float time) {
		return String.Format("{0:00}:{1:00.00}", Mathf.Floor(time / 60), time % 60);
	}

	void PulseText(Text text, float fromScale, float duration) {
		StartCoroutine(PulseTextAnimation(text, fromScale, duration));
	}

	public void SetDimensions(float xOffset, float yOffset, float width, float height) {
		layout.anchorMax = new Vector2(xOffset + width, yOffset + height);
		layout.anchorMin = new Vector2(xOffset, yOffset);
	}

	IEnumerator PulseTextAnimation(Text text, float fromScale, float duration) {
		var startScale = Vector3.one * fromScale;
		var endScale = Vector3.one;
		for (float time = 0f; time < duration; time += Time.deltaTime) {
			text.transform.localScale = Vector3.Lerp(startScale, endScale, time / duration);
			yield return null;
		}
	}
}