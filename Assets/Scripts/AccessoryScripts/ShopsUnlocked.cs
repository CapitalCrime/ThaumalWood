using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ClassMethods;
using StructureMethods;

public class ShopsUnlocked : MonoBehaviour
{
    bool[] unlockedShops = new bool[System.Enum.GetNames(typeof(ShopScript.Shop)).Length];
    int currentShop = 0;

    void Start()
    {
        try{ currentShop = (int)System.Enum.Parse(typeof(ShopScript.Shop), transform.name)-1; }
        catch { currentShop = 0; }
        unlockedShops[currentShop] = true;
        transform.parent.Find("TextBox").Find("Text").GetComponent<Text>().text = ""+(ShopScript.Shop)(currentShop+1);
    }

    public void unlockShop(ShopScript.Shop shop)
    {
        unlockedShops[(int)shop-1] = true;
    }

    public void changeShop(ShopScript.Shop shop)
    {
        if (unlockedShops[(int)shop-1])
        {
            transform.name = "" + shop;
        }
    }

    public void nextShop()
    {
        int i = currentShop+1;
        while(i != currentShop) {
            if (i >= unlockedShops.Length)
            {
                i = 0;
            }
            if(unlockedShops[i] == true)
            {
                transform.name = "" + (ShopScript.Shop)(i+1);
                transform.parent.Find("TextBox").Find("Text").GetComponent<Text>().text = "" + (ShopScript.Shop)(i+1);
                currentShop = i;
                break;
            } else
            {
                i++;
            }
        }
    }
    public void prevShop()
    {
        int i = currentShop - 1;
        while (i != currentShop)
        {
            if (i < 0)
            {
                i = unlockedShops.Length-1;
            }
            if (unlockedShops[i] == true)
            {
                transform.name = "" + (ShopScript.Shop)(i+1);
                transform.parent.Find("TextBox").Find("Text").GetComponent<Text>().text = "" + (ShopScript.Shop)(i+1);
                currentShop = i;
                break;
            }
            else
            {
                i--;
            }
        }
    }

    bool crRunning;

    IEnumerator alertMessage(int price, Text text)
    {
        crRunning = true;
        string prevText = text.text;
        text.text = "Not enough money in vault to buy " + Structure.Market + ". You need: " + price;
        yield return new WaitForSeconds(3.0f);
        text.text = prevText;
        yield return crRunning = false;
    }

    public void BaseShopButton(Button button)
    {
        Teams alliance = GameObject.Find("EventSystem").GetComponent<MazeScript>().alliance;
        int price = Structure.Market.GetPrice();
        if (alliance.Cash < price)
        {
            if (!crRunning)
            {
                StartCoroutine(alertMessage(price, transform.parent.Find("TextBox").Find("Text").GetComponent<Text>()));
            }
        }
        else
        {
            alliance.Cash -= price;
            button.transform.parent.Find("Next").gameObject.SetActive(true);
            button.transform.parent.Find("Prev").gameObject.SetActive(true);
            GameObject.Find("OutOfBattleGUI").transform.Find("CharPics").Find("TeamStats").Find("Cash").Find("Value").GetComponent<TextMeshProUGUI>().text = "" + alliance.Cash;
            gameObject.GetComponent<ShopsUnlocked>().unlockShop(ShopScript.Shop.Market);
            Destroy(button.gameObject);
            return;
        }

    }
}
