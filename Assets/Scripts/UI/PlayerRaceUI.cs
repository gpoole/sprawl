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

	public Text rankText;

	public Text rankOrdinalText;

	public Text winnerText;

	public Text loserText;

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
			.Subscribe(animation => animation.SetBool("Active", true));
		playerState.rank
			.SelectMany(rank => rankAnimations.Where((_, index) => index != (rank - 1)))
			.Subscribe(animation => animation.SetBool("Active", false));

		playerState.lapTimes
			.ObserveReplace()
			.Where(ev => ev.Index == playerState.lap.Value - 1)
			.Select(ev => String.Format("{0:00}:{1:00.00}", Mathf.Floor(ev.NewValue / 60), ev.NewValue % 60))
			.SubscribeToText(lapTimeText);

		playerState.mode
			.Where(mode => mode == PlayerState.PlayerMode.Starting)
			.Subscribe(_ => StartCoroutine(PlayIntro()));
	}

	void PulseText(Text text, float fromScale, float duration) {
		StartCoroutine(PulseTextAnimation(text, fromScale, duration));
	}

	IEnumerator PulseTextAnimation(Text text, float fromScale, float duration) {
		var startScale = Vector3.one * fromScale;
		var endScale = Vector3.one;
		for (float time = 0f; time < duration; time += Time.deltaTime) {
			text.transform.localScale = Vector3.Lerp(startScale, endScale, time / duration);
			yield return null;
		}
	}

	IEnumerator PlayIntro() {
		introText.gameObject.SetActive(true);
		yield return new WaitForSeconds(5);
		introText.gameObject.SetActive(false);
	}

	IEnumerator PlayOutro() {
		yield return null; // FIXME: noop
		if (playerState.rank.Value == 1) {
			winnerText.gameObject.SetActive(true);
		} else {
			loserText.gameObject.SetActive(true);
		}
	}

	string OrdinalSuffix(int rank) {
		if (rank <= 0) return "";

		switch (rank % 100) {
			case 11:
			case 12:
			case 13:
				return "th";
		}

		switch (rank % 10) {
			case 1:
				return "st";
			case 2:
				return "nd";
			case 3:
				return "rd";
			default:
				return "th";
		}

	}
}