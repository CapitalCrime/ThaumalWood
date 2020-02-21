using UnityEngine;
using UnityEngine.UI;

public class OptionsScript : MonoBehaviour
{
    public Slider boardSlider;
    public Slider enemySlider;
    public Text enemyMax;
    public Text enemyText;
    public Text boardText;

    public void Exit()
    {
        Stats.boardSize = (int)boardSlider.value;
        Stats.enemies = (int)enemySlider.value;
    }

    public void UpdateEnemyText()
    {
        enemyText.text = "" + enemySlider.value;
    }

    public void UpdateBoardText()
    {
        boardText.text = "" + boardSlider.value + " x "+ boardSlider.value;
        enemySlider.maxValue = (int)(boardSlider.value * 1.5f);
        enemyMax.text = "" + enemySlider.maxValue;
    }
}
