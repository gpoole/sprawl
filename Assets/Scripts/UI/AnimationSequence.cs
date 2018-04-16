using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimationSequence : AnimationBehaviour {

	[Serializable]
	public class Entry {
		public AnimationBehaviour animation;
		public bool played;
		public float offset = 0;
	}

	public List<Entry> animations;

	void Awake() {
		duration = animations.Max(entry => entry.offset + entry.animation.duration);
	}

	protected override void AnimationStep() {
		foreach (var playable in animations.Where(entry => !entry.played && !entry.animation.playing && time >= entry.offset)) {
			playable.animation.Play();
			playable.played = true;
		}
	}
}