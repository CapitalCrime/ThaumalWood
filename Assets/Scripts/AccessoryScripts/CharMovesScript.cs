using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ClassMethods;

public class CharMovesScript : MonoBehaviour
{
    MazeScript ms;
    Color red;
    Color green;
    Color grey;
    Color gold;
    Transform optimal;
    static public GameObject[] enemyPanels;
    static public GameObject[] yourPanels;
    private void Start()
    {
        ms = GameObject.Find("EventSystem").GetComponent<MazeScript>();
        red = new Color(164, 20, 20, 255) / 255.0f;
        green = new Color(130, 255, 130, 255) / 255.0f;
        gold = new Color(255, 255, 0, 255) / 255.0f;
        grey = new Color(0.5f, 0.5f, 0.5f, 1);
    }

    public static void destroyPanels()
    {
        for(int i = 0; i<enemyPanels.Length; i++)
        {
            Destroy(enemyPanels[i].gameObject);
            Destroy(yourPanels[i].gameObject);
        }
    }

    public void overFunction()
    {
        AttackMove attackMove = ms.alliance.Forces[ms.curTeam].getMember(ms.curChar).getAttackMove(int.Parse(transform.name));
        if (attackMove.getName() != "")
        {
            int position = ms.alliance.Forces[ms.curTeam].getMember(ms.curChar).getIndex();

            yourPanels[attackMove.getPosition()].transform.GetChild(0).GetComponent<Renderer>().material.color = gold;
            int adjustAmount = 0;
            bool offensive = attackMove.isOffensive();
            string SpecialSprite = "";
            string attackOffensive = "";

            switch (attackMove.getSpecial())
            {
                case Special.AttackBuff:
                    SpecialSprite = "<sprite=1>";
                    break;
                case Special.DefenseBuff:
                    SpecialSprite = "<sprite=0>";
                    break;
                case Special.Pull:
                    SpecialSprite = "<sprite=3>";
                    break;
                case Special.Poison:
                    SpecialSprite = "<sprite=2>";
                    break;
            }

            transform.parent.Find("Description").Find("NA").GetComponent<Image>().enabled = false;

            if (offensive)
            {
                adjustAmount = attackMove.getPosition() - position;
                attackOffensive = "Offensive";
            }
            else
            {
                adjustAmount = position - attackMove.getPosition();
                if((attackMove.getSpecial() == Special.None || attackMove.getSpecial() == Special.Clean) && ms.alliance.Forces[ms.curTeam].getMember(ms.curChar).healCooldown)
                {
                    transform.parent.Find("Description").Find("NA").GetComponent<Image>().enabled = true;
                }
                attackOffensive = "Defensive";
            }
            transform.parent.Find("Description").Find("Text").GetComponent<TextMeshProUGUI>().text = attackMove.getName() + ": " + attackMove.getDescription() + " \n"+attackOffensive+"\tBase power: " + attackMove.getPower() + "  " + SpecialSprite;
            if(attackMove.getSpecialPower() != 0)
            {
                transform.parent.Find("Description").Find("Text").GetComponent<TextMeshProUGUI>().text += "x" + attackMove.getSpecialPower();
            }
            for (int i = 0; i < 4; i++)
            {
                int attackIndex = i + adjustAmount;
                if (attackMove.getSpaces()[i] == true)
                {
                    if (offensive)
                    {
                        enemyPanels[i].transform.GetChild(0).GetComponent<Renderer>().material.color = gold;
                    }
                    else
                    {
                        yourPanels[i].transform.GetChild(0).GetComponent<Renderer>().material.color = gold;
                    }
                }
                if (attackIndex < 0) { continue; }
                if (attackIndex > 3) { continue; }
                if (attackMove.getSpaces()[i] == true)
                {
                    if (offensive)
                    {
                        enemyPanels[attackIndex].GetComponent<Renderer>().material.color = red;
                    }
                    else
                    {
                        yourPanels[attackIndex].GetComponent<Renderer>().material.color = green;
                    }
                }
                else
                {
                    enemyPanels[i].GetComponent<Renderer>().material.color = grey;
                }
            }
        }
    }

    public void resetFunction()
    {
        transform.parent.Find("Description").Find("NA").GetComponent<Image>().enabled = false;
        transform.parent.Find("Description").Find("Text").GetComponent<TextMeshProUGUI>().text = "";
        for (int i = 0; i < 4; i++)
        {
            enemyPanels[i].GetComponent<Renderer>().material.color = grey;
            enemyPanels[i].transform.GetChild(0).GetComponent<Renderer>().material.color = grey;
            yourPanels[i].GetComponent<Renderer>().material.color = grey;
            yourPanels[i].transform.GetChild(0).GetComponent<Renderer>().material.color = grey;
        }
    }
}
