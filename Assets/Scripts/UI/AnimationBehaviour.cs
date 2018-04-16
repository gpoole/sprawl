using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AnimationBehaviour : MonoBehaviour {

	public bool autoplay;

	public bool playing {
		get;
		protected set;
	}

	public float duration;

	public float time {
		get;
		protected set;
	}

	protected Coroutine animationCoroutine;

	protected virtual void Start() {
		if (autoplay) {
			Play();
		}
	}

	public virtual void Play() {
		playing = true;
		time = 0;
		animationCoroutine = StartCoroutine(PlaySteps());
	}

	public virtual void Stop() {
		if (playing) {
			playing = false;
			time = duration;
			if (animationCoroutine != null) {
				StopCoroutine(animationCoroutine);
				animationCoroutine = null;
			}
		}
	}

	private IEnumerator PlaySteps() {
		while (time < duration) {
			time += Time.deltaTime;
			AnimationStep();
			yield return null;
		}
		time = 1;
		AnimationStep();
		Stop();
	}

	protected abstract void AnimationStep();

}