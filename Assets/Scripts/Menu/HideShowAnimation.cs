using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class HideShowAnimation : MonoBehaviour {

	private Animator animator;

	void Start() {
		animator = GetComponent<Animator>();
	}

	public void Show() {
		animator.SetTrigger("Show");
	}

	public void Hide() {
		animator.SetTrigger("Hide");
	}
}