using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShaderAnimation : AnimationBehaviour {

	public Material material;

	private Material originalMaterial;

	void Awake() {
		originalMaterial = GetComponent<Image>().material;
		GetComponent<Image>().material = material;
		material.SetFloat("_AnimTime", 0);
	}

	public override void Play() {
		originalMaterial = GetComponent<Image>().material;
		GetComponent<Image>().material = material;
		base.Play();
	}

	public override void Stop() {
		base.Stop();
		GetComponent<Image>().material = originalMaterial;
	}

	protected override void AnimationStep() {
		material.SetFloat("_AnimTime", time);
	}
}