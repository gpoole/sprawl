using System.Collections;
using System.Linq;
using UnityEngine;

public class MenuScreenManager : MonoBehaviour {

    public MenuScreen[] screens;

    private MenuScreen activeScreen;

    public void GoTo(string screenName) {
        if (activeScreen) {
            StartCoroutine(HideScreen(activeScreen));
        }

        activeScreen = screens.FirstOrDefault(screen => screen.name == screenName);
        if (activeScreen) {
            StartCoroutine(ShowScreen(activeScreen));
        }
    }

    IEnumerator ShowScreen(MenuScreen screen) {
        screen.gameObject.SetActive(true);
        yield return new WaitUntil(() => screen.gameObject.activeInHierarchy);
        activeScreen.Show();
    }

    IEnumerator HideScreen(MenuScreen screen) {
        screen.Hide();
        yield return new WaitForSeconds(1f); // FIXME: aaaa
        screen.gameObject.SetActive(false);
    }

}