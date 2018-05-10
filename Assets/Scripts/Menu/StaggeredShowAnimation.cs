using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaggeredShowAnimation : MonoBehaviour {

	public float delay = 0.2f;

	public HideShowAnimation[] animations;

	public void Show() {
		StartCoroutine(DoShowAnimation());
	}

	public void Hide() {
		StartCoroutine(DoHideAnimation());
	}

	IEnumerator DoShowAnimation() {
		foreach (var animation in animations) {
			Debug.Log("Show");
			yield return new WaitForSeconds(delay);
			animation.Show();
		}
	}

	IEnumerator DoHideAnimation() {
		foreach (var animation in animations) {
			yield return new WaitForSeconds(delay);
			animation.Hide();
		}
	}
}