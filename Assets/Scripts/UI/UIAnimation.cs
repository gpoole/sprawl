using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAnimation : AnimationBehaviour {

	public enum Property {
		Alpha,
		Position,
		Dimensions
	}

	public Property property;

	public AnimationCurve curve;

	private CanvasRenderer canvas;

	private CanvasGroup canvasGroup;

	void Awake() {
		canvas = GetComponent<CanvasRenderer>();
		canvasGroup = GetComponent<CanvasGroup>();
	}

	protected override void Start() {
		AnimationStep();
		base.Start();
	}

	protected override void AnimationStep() {
		switch (property) {
			case Property.Alpha:
				var alpha = curve.Evaluate(time / duration);
				if (canvasGroup) {
					canvasGroup.alpha = alpha;
				} else {
					canvas.SetAlpha(alpha);
				}
				return;
		}
	}

}