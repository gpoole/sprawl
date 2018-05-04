using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class HideShowAnimation : MonoBehaviour {

	private Animator animator;

	void Awake() {
		animator = GetComponent<Animator>();
	}

	public void Show() {
		animator.ResetTrigger("Hide");
		animator.SetTrigger("Show");
	}

	public void Hide() {
		animator.ResetTrigger("Show");
		animator.SetTrigger("Hide");
	}
}