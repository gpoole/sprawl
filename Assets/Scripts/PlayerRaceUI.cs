using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRaceUI : MonoBehaviour {

	public PlayerState playerState;

	public Text lapText;

	public Text lapTimes;

	public Text rankText;

	public Text rankOrdinalText;

	public Text countdownText;

	public Text winnerText;

	public Text loserText;

	void Start() {
		playerState.lap.SubscribeToText(lapText);
		playerState.lap.Subscribe(_ => PulseText(lapText, 2f, 0.5f));

		playerState.rank.SubscribeToText(rankText);
		playerState.rank.Select(rank => OrdinalSuffix(rank)).SubscribeToText(rankOrdinalText);
		playerState.rank.Subscribe(_ => PulseText(rankText, 2f, 0.5f));

		playerState.lapTimes
			.ObserveAdd()
			.Select(ev => String.Format("Lap {0}: {1:00}:{2:00.00}", ev.Index + 1, Mathf.Floor(ev.Value / 60), ev.Value % 60))
			.Scan(((acc, lapTime) => acc + "\n" + lapTime))
			.SubscribeToText(lapTimes);

		playerState.mode
			.Where(mode => mode == PlayerState.PlayerMode.Starting)
			.Subscribe(_ => StartCoroutine(PlayIntro()));

		playerState.mode
			.Where(mode => mode == PlayerState.PlayerMode.Finished)
			.Subscribe(_ => StartCoroutine(PlayOutro()));

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
		countdownText.gameObject.SetActive(true);
		for (var i = 3; i > 0; i--) {
			yield return StartCoroutine(FlashCountdown(i));
		}
		yield return new WaitForSeconds(2f);
		countdownText.gameObject.SetActive(false);
	}

	IEnumerator FlashCountdown(int seconds) {
		const float animationTime = 1f;
		var scaleAnimation = AnimationCurve.EaseInOut(0, 4f, 1f, 1f);
		countdownText.text = seconds.ToString();
		for (float time = 0f; time < animationTime; time += Time.deltaTime) {
			countdownText.transform.localScale = Vector3.one * scaleAnimation.Evaluate(time / animationTime);
			countdownText.color = Color.Lerp(new Color(217, 42, 49, 0), new Color(217, 42, 49, 255), time / animationTime);
			yield return null;
		}
		yield return new WaitForSeconds(0.5f);
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