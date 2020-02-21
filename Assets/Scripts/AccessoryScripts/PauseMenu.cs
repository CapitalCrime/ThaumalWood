using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    public GameObject Menu;
    public GameObject MenuButton;
    bool playing = false;
    bool opening = true;

    public void PauseAnimationWrapper()
    {
        if (!playing)
        {
            playing = true;
            StartCoroutine(PauseAnimation());
        }
    }

    IEnumerator PauseAnimation()
    {
        float playTime = 0.0f;
        if (opening)
        {
            MenuButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Close menu";
            while (playTime < 1.0f)
            {
                Menu.transform.localScale = new Vector3(1, 1.0f * playTime, 1);
                playTime += Time.deltaTime * 3.5f;
                yield return null;
            }
            Menu.transform.localScale = Vector3.one;
        } else {
            MenuButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Open menu";
            while (playTime < 1.0f)
            {
                Menu.transform.localScale = new Vector3(1, 1.0f - (1.0f * playTime), 1);
                playTime += Time.deltaTime * 3.5f;
                yield return null;
            }
            Menu.transform.localScale = Vector3.zero;
        }
        playing = false;
        opening = !opening;
        yield return null;
    }
}
