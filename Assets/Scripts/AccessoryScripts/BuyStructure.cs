using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ClassMethods;
using StructureMethods;

public class BuyStructure : MonoBehaviour
{
    // Start is called before the first frame update

    void Start()
    {
        try
        {
            transform.GetChild(0).GetChild(0).GetComponent<Text>().text = "Buy " + structure;
        } catch { }
        price = structure.GetPrice();
    }

    public Structure structure;
    int price = 0;
    bool crRunning;
    
    IEnumerator alertMessage(int price)
    {
        crRunning = true;
        transform.GetChild(0).GetChild(0).GetComponent<Text>().text = "Not enough money in vault to buy " + structure + ". You need: " + price;
        yield return new WaitForSeconds(3.0f);
        transform.GetChild(0).GetChild(0).GetComponent<Text>().text = "Buy "+structure;
        yield return crRunning = false;
    }

    void OnMouseDown()
    {
        Teams alliance = GameObject.Find("EventSystem").GetComponent<MazeScript>().alliance;

        if(alliance.Cash < price)
        {
            if (!crRunning)
            {
                StartCoroutine(alertMessage(price));
            }
            return;
        } else
        {
            alliance.Cash -= price;
            GameObject.Find("OutOfBattleGUI").transform.Find("CharPics").Find("TeamStats").Find("Cash").Find("Value").GetComponent<TextMeshProUGUI>().text = "" + alliance.Cash;
            string itemName = "Structures/" + structure;
            if(structure == Structure.Mine)
            {
                alliance.setStructOwned(0);
            } else if(structure == Structure.Forestry)
            {
                alliance.setStructOwned(1);
            }
            else if (structure == Structure.Bank)
            {
                alliance.setStructOwned(2);
            }
            GameObject fort = Resources.Load(itemName) as GameObject;
            Instantiate(fort, new Vector3(transform.position.x, 0, transform.position.z), transform.rotation);
            Destroy(gameObject);
            return;
        }
    }
}
