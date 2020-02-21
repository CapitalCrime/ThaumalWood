using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TutorialText : MonoBehaviour
{
    [System.Serializable]
    public class Dialog
    {
        public string text;
        public GameObject display;

        public Dialog(string text, GameObject display)
        {
            this.text = text;
            this.display = display;
        }
    }
    public Dialog[] dialog;
    int currentText = 0;
    bool open = false;
    public TextMeshProUGUI TextField;
    public TextMeshProUGUI NextButtonText;
    public ScrollRect ScrollView;

    public void StartTutorial()
    {
        if (!open)
        {
            if (dialog[0].display != null)
            {
                dialog[0].display.SetActive(true);
            }
            TextField.text = dialog[0].text;
            gameObject.SetActive(true);
            currentText = 0;
            open = true;
        } else
        {
            if(dialog[currentText].display != null)
            {
                dialog[currentText].display.SetActive(false);
            }
            gameObject.SetActive(false);
            NextButtonText.text = ">";
            open = false;
        }
    }
    public void NextTutorial(GameObject button)
    {
        if(dialog[currentText].display != null) {
            dialog[currentText].display.SetActive(false);
        }
        
        currentText++;

        if(currentText == dialog.Length-1)
        {
            button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "X";
        }

        if(currentText < dialog.Length)
        {
            if (dialog[currentText].display != null) {
                dialog[currentText].display.SetActive(true);
            }
            TextField.text = dialog[currentText].text;
            if(ScrollView != null)
            {
                ScrollView.verticalScrollbar.value = 1;
            }
        } else
        {
            NextButtonText.text = ">";
            gameObject.SetActive(false);
            open = false;
        }
    }
}
