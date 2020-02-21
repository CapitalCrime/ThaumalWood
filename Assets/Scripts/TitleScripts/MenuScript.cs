using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuScript : MonoBehaviour
{
    public Slider loadingBar;
    public GameObject loadingScreen;
    public Text loadText;
    public GameObject OptionsScreen;

    private void Awake()
    {
        Screen.SetResolution(1920, 1080, true);
    }
    public void Play(int sceneNum)
    {
        StartCoroutine(LoadScene(sceneNum));
    }

    IEnumerator LoadScene(int sceneNum)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneNum);
        loadingScreen.SetActive(true);
        while (!loadOperation.isDone)
        {
            float progress = Mathf.Clamp01(loadOperation.progress / 0.9f);

            loadingBar.value = progress;
            loadText.text = progress * 100 + "%";

            yield return null;
        }
        yield break;
    }

    public void StartButton()
    {
        List<string> resolutions = new List<string>();
        foreach(Resolution res in Screen.resolutions)
        {
            resolutions.Add(""+res.width+"x"+res.height);
        }
        OptionsScreen.transform.Find("ResDD").GetComponent<TMP_Dropdown>().ClearOptions();
        OptionsScreen.transform.Find("ResDD").GetComponent<TMP_Dropdown>().AddOptions(resolutions);
    }

    public void changeResolution()
    {
        int num = OptionsScreen.transform.Find("ResDD").GetComponent<TMP_Dropdown>().value;
        Screen.SetResolution(Screen.resolutions[num].width, Screen.resolutions[num].height, Screen.fullScreen);
    }

    public void changeFullscreen(Toggle toggle)
    {
        Screen.fullScreen = toggle.isOn;
    }
}
