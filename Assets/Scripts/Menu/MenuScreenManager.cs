using System.Collections;
using System.Linq;
using UnityEngine;

public class MenuScreenManager : MonoBehaviour {

    public MenuScreen[] screens;

    private MenuScreen activeScreen;

    void Start() {
        activeScreen = screens.First();
        StartCoroutine(ShowScreen(activeScreen, true));
    }

    public void GoTo(string screenName) {
        if (activeScreen) {
            StartCoroutine(HideScreen(activeScreen));
        }

        var nextScreen = screens.FirstOrDefault(screen => screen.name == screenName);
        if (nextScreen) {
            StartCoroutine(ShowScreen(nextScreen, activeScreen == null));
            activeScreen = nextScreen;
        }
    }

    IEnumerator ShowScreen(MenuScreen screen, bool first) {
        // Wait for the old screen to go away
        if (!first) {
            yield return new WaitForSeconds(1f);
        }
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