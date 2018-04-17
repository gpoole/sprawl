using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShaderAnimation : MonoBehaviour {

	public float time;

	private Image image;

	void Awake() {
		image = GetComponent<Image>();
	}

	void Update() {
		image.material.SetFloat("_AnimTime", time);
	}
}