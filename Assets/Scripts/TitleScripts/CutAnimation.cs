using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class CutAnimation : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject Canvas;
    GameObject top;
    GameObject bottom;
    Vector2 startTop;
    Vector2 finalTop;
    Vector2 startBottom;
    Vector2 finalBottom;
    float time = 0.0f;
    Coroutine o = null;
    void Start()
    {
        top = Canvas.transform.Find("Top").gameObject;
        bottom = Canvas.transform.Find("Bottom").gameObject;
        Color tempColor = top.GetComponent<Image>().color;
        tempColor.a = 1;
        top.GetComponent<Image>().color = tempColor;
        bottom.GetComponent<Image>().color = tempColor;

        startTop = top.GetComponent<RectTransform>().anchoredPosition;
        startBottom = bottom.GetComponent<RectTransform>().anchoredPosition;
        finalTop = new Vector2(0, 2800);
        finalBottom = new Vector2(0, -2800);

        o = StartCoroutine(Animation(false));
    }

    public void AnimationStarter()
    {
        gameObject.SetActive(true);
        time = 0.0f;
        if (o != null) { StopCoroutine(o); }
        StartCoroutine(Animation(true));
    }

    IEnumerator Animation(bool backWards)
    {
        gameObject.SetActive(true);
        AsyncOperation op = null;
        if (backWards) { op = SceneManager.LoadSceneAsync(0); op.allowSceneActivation = false; }
        while (time < 1.0f)
        {
            if (backWards)
            {
                top.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(finalTop, startTop, time);
                bottom.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(finalBottom, startBottom, time);
            }
            else
            {
                top.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(startTop, finalTop, time);
                bottom.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(startBottom, finalBottom, time);
            }
            time += Time.deltaTime * 1.25f;
            yield return null;
        }
        if (backWards) { op.allowSceneActivation = true; yield break; }
        top.GetComponent<RectTransform>().anchoredPosition = finalTop;
        bottom.GetComponent<RectTransform>().anchoredPosition = finalBottom;
        gameObject.SetActive(false);
        yield return null;
    }
}
