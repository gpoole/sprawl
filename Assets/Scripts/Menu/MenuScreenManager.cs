using System.Collections;
using System.Linq;
using UnityEngine;

public class MenuScreenManager : MonoBehaviour {

    public MenuScreen[] screens;

    private MenuScreen activeScreen;

    void Start() {
        StartCoroutine(ShowScreen(screens.First()));
    }

    public void GoTo(string screenName) {
        if (activeScreen) {
            StartCoroutine(HideScreen(activeScreen));
        }

        var nextScreen = screens.FirstOrDefault(screen => screen.name == screenName);
        if (nextScreen) {
            StartCoroutine(ShowScreen(nextScreen));
            activeScreen = nextScreen;
        }
    }

    IEnumerator ShowScreen(MenuScreen screen) {
        screen.gameObject.SetActive(true);
        yield return new WaitUntil(() => screen.gameObject.activeInHierarchy);
        screen.Show();
    }

    IEnumerator HideScreen(MenuScreen screen) {
        screen.Hide();
        yield return new WaitForSeconds(1f); // FIXME: aaaa
        screen.gameObject.SetActive(false);
    }

}