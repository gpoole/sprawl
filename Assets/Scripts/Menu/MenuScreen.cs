using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class MenuScreen : MonoBehaviour {

    public UnityEvent OnShow = new UnityEvent();

    public UnityEvent OnHide = new UnityEvent();

    private Animator animator;

    void Start() {
        animator = GetComponent<Animator>();
    }

    public void Show() {
        animator.SetTrigger("Show");
        OnShow.Invoke();
    }

    public void Hide() {
        animator.SetTrigger("Hide");
        OnHide.Invoke();
    }

}