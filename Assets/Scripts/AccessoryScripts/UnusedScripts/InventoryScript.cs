/*using UnityEngine;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;
using UnityEngine.UI;*/

public class InventoryScript
{
    /*
    string[] itemNames;
    GameObject gun;
    public LayerMask clickMask;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out hit, 100, clickMask))
            {
                if (hit.transform.tag == "Door" && (GameObject.Find("EventSystem").GetComponent<MazeScript>().inBattle == false))
                {
                    MazeScript maze = GameObject.Find("EventSystem").GetComponent<MazeScript>();
                    int currentchar = maze.curChar;
                    Debug.Log(currentchar);
                    GameObject player = maze.alliance.Forces[maze.curTeam].getMember(maze.curChar).role;
                    Debug.Log(player.name);
                    Vector3 dir = (hit.transform.position - hit.transform.parent.parent.Find("DirectionCube").transform.position).normalized;
                    Vector3 newPos = new Vector3(hit.transform.position.x + dir.x*10, player.transform.position.y, hit.transform.position.z + dir.z*10);
                    player.transform.position = newPos;
                    int roox = (int)((player.transform.position.x + 20) / 40f);
                    int rooy = (int)((player.transform.position.z + 20) / 40f);
                    int max = GameObject.Find("EventSystem").GetComponent<MazeScript>().boardSize;
                    if ((roox >= 0) && (roox < max) && (rooy >= 0) && (rooy < max))
                    {
                        GameObject.Find("EventSystem").GetComponent<MazeScript>().moveRooms(roox, rooy);
                    }
                } else if(hit.transform.tag == "Shop")
                {
                    transform.parent.Find("Shop").GetComponent<ShopScript>().initShop(hit.transform.name);
                }
                else if (hit.transform.tag == "Weapon")
                {
                    int i = 0;
                    while (i < itemNames.Length)
                    {
                        if (itemNames[i] == null)
                        {
                            itemNames[i] = hit.transform.name;
                            Debug.Log("Added! " + itemNames[i] + "");
                            //string regex = Regex.Match(itemNames[i], "\\w+ - \\d+").Value;
                            //if(regex == "")
                            //{
                            //    regex = "0";
                            //}
                            //int num = int.Parse(regex);
                            //Debug.Log(num);
                            Debug.Log(Regex.Match(itemNames[i], "^[a-zA-Z]+").Value);
                            for (int j = 0; j < itemNames.Length; j++)
                            {
                                transform.Find("Inv" + (j + 1) + "").Find("Name").GetComponent<Text>().text = itemNames[j];
                            }
                            break;
                        }
                        else if (hit.transform.name == Regex.Match(itemNames[i], "[a-zA-Z]+").Value)
                        {
                            string regex = Regex.Match(itemNames[i], "\\d+").Value;
                            if (regex == "")
                            {
                                regex = "1";
                            }
                            int num = int.Parse(regex);
                            num++;
                            itemNames[i] = Regex.Match(itemNames[i], "[a-zA-Z]+").Value + " [" + num + "]";
                            for (int j = 0; j < itemNames.Length; j++)
                            {
                                transform.Find("Inv" + (j + 1) + "").Find("Name").GetComponent<Text>().text = itemNames[j];
                            }
                            break;
                        }
                        i++;
                    }
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 10, clickMask))
            {
                Debug.Log(hit.transform.name);
                int i = 0;
                while (i < itemNames.Length)
                {
                    if (itemNames[i] != null && hit.transform.name == Regex.Match(itemNames[i], "[a-zA-Z]+").Value)
                    {
                        string regex = Regex.Match(itemNames[i], "\\d+").Value;
                        if (regex == "")
                        {
                            regex = "1";
                        }
                        int num = int.Parse(regex);
                        num--;
                        if (num == 0)
                        {
                            itemNames[i] = null;
                        }
                        else
                        {
                            if (num == 1)
                            {
                                itemNames[i] = Regex.Match(itemNames[i], "[a-zA-Z]+").Value;
                            }
                            else
                            {
                                itemNames[i] = Regex.Match(itemNames[i], "[a-zA-Z]+").Value + " [" + num + "]";
                            }
                        }
                        for (int j = 0; j < itemNames.Length; j++)
                        {
                            transform.Find("Inv" + (j + 1) + "").Find("Name").GetComponent<Text>().text = itemNames[j];
                        }
                        break;
                    }
                    i++;
                }
            }
        }
    }*/

}
