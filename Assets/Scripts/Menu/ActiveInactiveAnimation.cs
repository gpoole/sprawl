using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ActiveInactiveAnimation : MonoBehaviour {

    private Animator animator;

    void Awake() {
        animator = GetComponent<Animator>();
    }

    public void Activate() {
        animator.ResetTrigger("Deactivate");
        animator.SetTrigger("Activate");
    }

    public void Deactivate() {
        animator.ResetTrigger("Activate");
        animator.SetTrigger("Deactivate");
    }
}