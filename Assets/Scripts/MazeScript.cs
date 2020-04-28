using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ClassMethods;
using StructureMethods;
using UnityEngine.EventSystems;
using TMPro;

/* || TODO ||
 * Optimize
 * Make save function
 * 
 * 
 * 
 * 
 */

public class MazeScript : MonoBehaviour
{
    public Canvas canvas;
    public Transform WorldCanvas;
    public GameObject OutOfBattleGUI;
    public GameObject EnemyAttackCS;
    public GameObject CharacterAttackCS;
    public GameObject TextAttackCS;
    public LayerMask clickMask;
    public GameObject CameraOne;
    public GameObject CameraTwo;
    public GameObject ExitUI;
    public GameObject Map;
    public TextMeshProUGUI activityFeed;

    int boardSize = 10;
    int enemies = 10;
    int MOVEAMOUNT = 5;
    bool skipAIBattles = true;

    mazeArray maze;
    public Teams alliance;
    EnemyTeams enemyAlliance;
    int prevx = -1;
    int prevy = -1;

    public int curTeam = 0;
    public int curChar = 0;

    int eCurTeam = 0;

    int max_Team = Stats.membersPerTeam;
    int max_Forces = Stats.numberOfTeams;

    Position playerBasePosition;
    Position enemyBasePosition;
    GameObject playerRoom;
    GameObject playerInterior;
    GameObject playerEvent;

    bool playerPhase = false;
    AttackMove chosenMove = null;
    public bool inBattle = false;
    List<Position> NPCdefeat;

    bool playerTurn = true;
    bool swapping = false;
    bool cancelSwap = false;

    bool raidWarning = false;
    bool aiBattle = false;
    bool raiding = false;

    int turnNumber = 0;
    int playerMoves = 0;
    int enemyMoves = 0;

    [System.Serializable]
    public class Pool
    {
        public string type;
        public GameObject _object;
        public int amount;

        public Pool(string type, GameObject _object, int amount)
        {
            this.type = type;
            this._object = _object;
            this.amount = amount;
        }
    }

    public List<Pool> roomPools;
    public List<Pool> interiorPools;
    public List<Pool> eventPools;
    Dictionary<string, Queue<GameObject>> roomDictionary;

    void Start()
    {
        OutOfBattleGUI.transform.Find("StartTutorial").gameObject.SetActive(Stats.showTutorial);
        boardSize = Stats.boardSize;
        enemies = Stats.enemies;
        MOVEAMOUNT = (int)(boardSize / 1.25f);
        NPCdefeat = new List<Position>();

        alliance = new Teams();
        enemyAlliance = new EnemyTeams();
        Debug.Log(enemyAlliance.Forces[0].getIntelligence());
        alliance.Forces[curTeam].GetLoadOut(TeamLoadOut.AllianceTwo);
        enemyAlliance.Forces[0].GetLoadOut(TeamLoadOut.Alliance);
        enemyAlliance.Forces[1].GetLoadOut(TeamLoadOut.Alliance);

        alliance.Forces[curTeam].getMember(0).role.SetActive(true);
        
        updateCharPic(curTeam);
        changeArrow(curChar, true);
        gameObject.GetComponent<PlayerScript>().initChar(alliance.Forces[curTeam].getMember(0).role);

        createBoard();
        OutOfBattleGUI.transform.Find("CharPics").transform.Find("TeamText").GetComponent<TextMeshProUGUI>().text = "Team " + curTeam;
        placeRooms();
        PoolSetup();
        Map.GetComponent<MapSetup>().SetUpMap(playerBasePosition);
        moveRooms(playerBasePosition.x, playerBasePosition.y);
        StartCoroutine(PlayerTurn());
    }

    public void watchAIBattles(Toggle toggle)
    {
        skipAIBattles = toggle.isOn;
    }

    //
    // SETS UP POOLS FOR ROOMS AND INTERIORS

    void PoolSetup()
    {
        roomDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in roomPools)
        {
            Queue<GameObject> objPool = new Queue<GameObject>();

            for (int i = 0; i < pool.amount; i++)
            {
                GameObject obj = Instantiate(pool._object);
                obj.SetActive(false);
                objPool.Enqueue(obj);
            }

            roomDictionary.Add(pool.type, objPool);
        }
        foreach (Pool pool in interiorPools)
        {
            Queue<GameObject> intPool = new Queue<GameObject>();

            for (int i = 0; i < pool.amount; i++)
            {
                GameObject obj = Instantiate(pool._object);
                obj.SetActive(false);
                intPool.Enqueue(obj);
            }

            roomDictionary.Add(pool.type, intPool);
        }
        foreach (Pool pool in eventPools)
        {
            Queue<GameObject> ePool = new Queue<GameObject>();

            for (int i = 0; i < pool.amount; i++)
            {
                GameObject obj = Instantiate(pool._object);
                obj.SetActive(false);
                ePool.Enqueue(obj);
            }

            roomDictionary.Add(pool.type, ePool);
        }
        for (int i = 2; i < 5; i++)
        {
            Queue<GameObject> eventPool = new Queue<GameObject>();
            for (int j = 0; j < 2; j++)
            {
                GameObject obj = Instantiate(Resources.Load("Events/Shop") as GameObject);
                obj.transform.GetChild(0).Find("General").GetComponent<ShopsUnlocked>().unlockShop((ShopScript.Shop)i);
                obj.SetActive(false);
                eventPool.Enqueue(obj);
            }
            roomDictionary.Add("" + (ShopScript.Shop)i, eventPool);
        }
    }

    //
    // SPAWNS AN OBJECT FROM THE SPECIFIED POOL, WITH A SPECIFIED POSITION AND ROTATION

    GameObject SpawnFromPool(string poolType, Vector3 position, Quaternion rotation)
    {
        if (!roomDictionary.ContainsKey(poolType))
        {
            return null;
        }
        GameObject spawnObj = roomDictionary[poolType].Dequeue();

        spawnObj.SetActive(true);
        spawnObj.transform.position = position;
        spawnObj.transform.rotation = rotation;

        roomDictionary[poolType].Enqueue(spawnObj);
        return spawnObj;
    }

    //
    // ALLOWS YOU TO GIVE A MEMBER AN ATTACK MOVE

    public int giveAttackMove(AttackMove att)
    {
        return alliance.Forces[curTeam].getMember(curChar).addAttackMove(att);
    }

    //
    // WRAPPER CLASS FOR SWAP FUNCTION FOR BUTTON USAGE

    public void swapWrapper()
    {
        if(!swapping)
        {
            swapping = true;
            StartCoroutine("swapAction");
        } else
        {
            cancelSwap = true;
        }
    }

    //
    // SWAPS TO DESIRED POSITION

    void swapTeam(Team team, int fromInd, int toInd, int x, int y, bool firstPics)
    {
        if(toInd > max_Team-1) { toInd = max_Team - 1; }
        if(toInd < 0) { toInd = 0; }
        if(team.getMember(toInd).role == null)
        {
            swapMembers(team, fromInd, toInd, x, y, firstPics);
        } else {
            int direction = (int)Mathf.Sign(toInd-fromInd);
            for (int i = fromInd; i != toInd; i+=direction)
            {
                swapMembers(team, i, i+direction, x, y, firstPics);
            }
        }
    }

    //
    // SWAPS TWO MEMBERS ON A TEAM

    void swapMembers(Team team, int fromInd, int toInd, int x, int y, bool firstPics)
    {
        if(toInd < 0) { toInd = 0; }
        if(toInd > max_Team-1) { toInd = max_Team - 1; }
        Transform indexStats = null;

        if (firstPics)
        {
            indexStats = WorldCanvas.Find("First").GetChild(toInd);
            indexStats.Find("Attack").GetComponent<TextMeshProUGUI>().text = (100 + team.getMember(fromInd).getAttackMult()) + "";
            indexStats.Find("Defence").GetComponent<TextMeshProUGUI>().text = (100 + team.getMember(fromInd).getDefenceMult()) + "";
            indexStats.Find("Poison").GetComponent<TextMeshProUGUI>().text = "" + team.getMember(fromInd).getPoisonCounter();
            indexStats.Find("Dazed").GetComponent<TextMeshProUGUI>().text = "" + team.getMember(fromInd).getSwapSafety();
            indexStats.gameObject.SetActive(true);
            if(team.getMember(fromInd).role != null)
            {
                team.getMember(fromInd).role.transform.position = new Vector3(maze.roomPos[x, y].x - (x - prevx) * (7 + (3 * toInd)), 0.1f, maze.roomPos[x, y].z - (y - prevy) * (7 + (3 * toInd)));
            }
            if (team.getMember(toInd).role != null)
            {
                WorldCanvas.Find("First").GetChild(fromInd).Find("Attack").GetComponent<TextMeshProUGUI>().text = (100 + team.getMember(toInd).getAttackMult()) + "";
                WorldCanvas.Find("First").GetChild(fromInd).Find("Defence").GetComponent<TextMeshProUGUI>().text = (100 + team.getMember(toInd).getDefenceMult()) + "";
                WorldCanvas.Find("First").GetChild(fromInd).Find("Poison").GetComponent<TextMeshProUGUI>().text = "" + team.getMember(toInd).getPoisonCounter();
                WorldCanvas.Find("First").GetChild(fromInd).Find("Dazed").GetComponent<TextMeshProUGUI>().text = "" + team.getMember(toInd).getSwapSafety();
                team.getMember(toInd).role.transform.position = new Vector3(maze.roomPos[x, y].x - (x - prevx) * (7 + (3 * fromInd)), 0.1f, maze.roomPos[x, y].z - (y - prevy) * (7 + (3 * fromInd)));
            } else
            {
                WorldCanvas.Find("First").GetChild(fromInd).gameObject.SetActive(false);
            }
        }
        else
        {
            indexStats = WorldCanvas.Find("Second").GetChild(toInd);
            indexStats.Find("Attack").GetComponent<TextMeshProUGUI>().text = (100 + team.getMember(fromInd).getAttackMult()) + "";
            indexStats.Find("Defence").GetComponent<TextMeshProUGUI>().text = (100 + team.getMember(fromInd).getDefenceMult()) + "";
            indexStats.Find("Poison").GetComponent<TextMeshProUGUI>().text = "" + team.getMember(fromInd).getPoisonCounter();
            indexStats.Find("Dazed").GetComponent<TextMeshProUGUI>().text = "" + team.getMember(fromInd).getSwapSafety();
            indexStats.gameObject.SetActive(true);
            if(team.getMember(fromInd).role != null)
            {
                team.getMember(fromInd).role.transform.position = new Vector3(maze.roomPos[x, y].x + (x - prevx) * (7 + (3 * toInd)), 0.1f, maze.roomPos[x, y].z + (y - prevy) * (7 + (3 * toInd)));
            }
            if (team.getMember(toInd).role != null)
            {
                WorldCanvas.Find("Second").GetChild(fromInd).Find("Attack").GetComponent<TextMeshProUGUI>().text = (100 + team.getMember(toInd).getAttackMult()) + "";
                WorldCanvas.Find("Second").GetChild(fromInd).Find("Defence").GetComponent<TextMeshProUGUI>().text = (100 + team.getMember(toInd).getDefenceMult()) + "";
                WorldCanvas.Find("Second").GetChild(fromInd).Find("Poison").GetComponent<TextMeshProUGUI>().text = "" + team.getMember(toInd).getPoisonCounter();
                WorldCanvas.Find("Second").GetChild(fromInd).Find("Dazed").GetComponent<TextMeshProUGUI>().text = "" + team.getMember(toInd).getSwapSafety();
                team.getMember(toInd).role.transform.position = new Vector3(maze.roomPos[x, y].x + (x - prevx) * (7 + (3 * fromInd)), 0.1f, maze.roomPos[x, y].z + (y - prevy) * (7 + (3 * fromInd)));
            }else
            {
                WorldCanvas.Find("Second").GetChild(fromInd).gameObject.SetActive(false);
            }
        }
        
        team.swapMember(fromInd, toInd);

        if (inBattle)
        {
            if (team.ai) { changeArrow(toInd, firstPics); }

            Transform Canvas = canvas.transform.Find("HealthPics").Find("Character");
            if (!firstPics)
            {
                Canvas = canvas.transform.Find("HealthPics").Find("Enemy");
            }

            Slider HealthBar = Canvas.Find("" + fromInd).Find("HealthBar").GetComponent<Slider>();
            HealthBar.maxValue = team.getMember(fromInd).getHealthCap();
            HealthBar.value = team.getMember(fromInd).getHealth();
            HealthBar.transform.Find("HealthText").GetComponent<TextMeshProUGUI>().text = "" + HealthBar.value + " / " + HealthBar.maxValue;

            float fromSpeedValue = Canvas.Find("" + fromInd).Find("SpeedBar").GetComponent<Slider>().value;
            Canvas.Find("" + fromInd).Find("SpeedBar").GetComponent<Slider>().value = Canvas.Find("" + toInd).Find("SpeedBar").GetComponent<Slider>().value;
            Canvas.Find("" + fromInd).Find("SpeedBar").Find("Title").GetComponent<TextMeshProUGUI>().text = "" + team.getMember(fromInd).getSpeed() + "/" + team.getMember(fromInd).getSpeedCap();
            Canvas.Find("" + fromInd).Find("Level").GetChild(0).GetComponent<TextMeshProUGUI>().text = "" + team.getMember(fromInd).getLevel();

            HealthBar = Canvas.Find("" + toInd).Find("HealthBar").GetComponent<Slider>();
            HealthBar.maxValue = team.getMember(toInd).getHealthCap();
            HealthBar.value = team.getMember(toInd).getHealth();
            HealthBar.transform.Find("HealthText").GetComponent<TextMeshProUGUI>().text = "" + HealthBar.value + " / " + HealthBar.maxValue;

            Canvas.Find("" + toInd).Find("SpeedBar").GetComponent<Slider>().value = fromSpeedValue;
            Canvas.Find("" + toInd).Find("SpeedBar").Find("Title").GetComponent<TextMeshProUGUI>().text = "" + team.getMember(toInd).getSpeed() + "/" + team.getMember(toInd).getSpeedCap();
            Canvas.Find("" + toInd).Find("Level").GetChild(0).GetComponent<TextMeshProUGUI>().text = "" + team.getMember(toInd).getLevel();
        }
    }

    //
    // PERFORMS THE SWAPPING ACTION DURING BATTLE

    IEnumerator swapAction()
    {
        if (inBattle)
        {
            GameObject[] gameObjects = new GameObject[max_Team];
            GameObject block = Resources.Load("Events/SwapBlock") as GameObject;
            SwapScript swapScript = gameObject.GetComponent<SwapScript>();
            int x = (int)((alliance.Forces[curTeam].getMember(curChar).role.transform.position.x + 20) / 40f);
            int y = (int)((alliance.Forces[curTeam].getMember(curChar).role.transform.position.z + 20) / 40f);
            
            for (int i = 0; i < max_Team; i++)
            {
                if (i == curChar) continue;
                gameObjects[i] = Instantiate(block, Vector3.one, Quaternion.identity);
                gameObjects[i].transform.name = "" + i;
                gameObjects[i].transform.position = new Vector3(maze.roomPos[x, y].x - (x - prevx) * (7 + (3 * i)), 2.5f, maze.roomPos[x, y].z - (y - prevy) * (7 + (3 * i)));
            }

            while (swapScript.swapWith == -1) { if (cancelSwap) { break; } yield return null; }
            foreach (GameObject obj in gameObjects) { Destroy(obj); }

            if (!cancelSwap)
            {
                swapTeam(alliance.Forces[curTeam], curChar, swapScript.swapWith, x, y, true);
                curChar = swapScript.swapWith;
                switchChar(alliance.Forces[curTeam], curChar, swapScript.swapWith);
                canvas.transform.Find("CharMoves").Find("SwapPlayer").gameObject.SetActive(false);
            }

            cancelSwap = false;
            swapScript.swapWith = -1;
        }
        swapping = false;
        yield return null;
    }

    void Update()
    {
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.Confined;
        } else
        {
            Cursor.lockState = CursorLockMode.None;
        }
        if(!inBattle)
        {
            if (Input.GetMouseButtonDown(0) && playerTurn && alliance.Forces[curTeam].numberMems > 0)
            {
                RaycastHit hit = new RaycastHit();
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out hit, 100, clickMask))
                {
                    if (hit.transform.tag == "Door" && inBattle == false && playerMoves > 0)
                    {
                        int currentchar = curChar;
                        GameObject player = alliance.Forces[curTeam].getMember(curChar).role;
                        Vector3 dirCubePos = hit.transform.parent.parent.Find("DirectionCube").transform.position;
                        Vector3 dir = (hit.transform.position - dirCubePos).normalized;
                        Vector3 newPos = new Vector3(hit.transform.position.x + dir.x * 10, player.transform.position.y, hit.transform.position.z + dir.z * 10);
                        Debug.Log("Direction, x: " + dir.x + ", y: " + dir.z);
                        Debug.Log("Direction cube, x: " + dirCubePos.x + ", y: " + dirCubePos.z);
                        int roox = (int)Mathf.Round((dirCubePos.x + (40 * dir.x)) / 40f);
                        int rooy = (int)Mathf.Round((dirCubePos.z + (40 * dir.z)) / 40f);
                        Debug.Log("roox: " + roox + ", rooy: " + rooy);
                        if (!enemyBasePosition.Equals(new Position(roox, rooy)))
                        {
                            raidWarning = false;
                            player.transform.position = newPos;
                            if (roox > -1 && roox < boardSize && rooy > -1 && rooy < boardSize)
                            {
                                playerMoves--;
                                canvas.transform.Find("OutOfBattleGUI").Find("MovesLeft").Find("Moves").GetComponent<TextMeshProUGUI>().text = "" + playerMoves;
                                moveRooms(roox, rooy);
                            }
                            else
                            {
                                if (OutOfBattleGUI.transform.Find("Shop").gameObject.activeSelf)
                                {
                                    OutOfBattleGUI.transform.Find("Shop").GetComponent<ShopScript>().closeShop();
                                }

                                Transform charIm = OutOfBattleGUI.transform.Find("CharPics").transform.Find("TeamStats");
                                foreach (Member mem in alliance.Forces[curTeam].getTeam())
                                {
                                    alliance.Cash += mem.getGold();
                                    mem.setGold(0);
                                    charIm.Find("" + mem.getIndex()).Find("Gold").GetComponent<TextMeshProUGUI>().text = "" + alliance.Forces[curTeam].getMember(mem.getIndex()).getGold();
                                }
                                charIm.Find("Cash").GetChild(0).GetComponent<TextMeshProUGUI>().text = "" + alliance.Cash;
                            }
                        }
                        else
                        {
                            if (raidWarning == false)
                            {
                                Transform ListenText = WorldCanvas.Find("ListenText");
                                ListenText.Find("Text").GetComponent<TextMeshProUGUI>().text = "You found the enemy base! Click on this door again to start a raid";
                                ListenText.position = hit.transform.position - hit.transform.right;
                                ListenText.transform.LookAt(hit.transform.position);
                                ListenText.GetComponent<Animator>().Play("ListenText", 0, 0);
                                raidWarning = true;
                            }
                            else
                            {
                                if (roox > -1 && roox < boardSize && rooy > -1 && rooy < boardSize)
                                {
                                    playerMoves--;
                                    canvas.transform.Find("OutOfBattleGUI").Find("MovesLeft").Find("Moves").GetComponent<TextMeshProUGUI>().text = "" + playerMoves;
                                    moveRooms(roox, rooy);
                                }
                            }
                        }
                    }
                    else if (hit.transform.tag == "Shop")
                    {
                        OutOfBattleGUI.transform.Find("Shop").GetComponent<ShopScript>().initShop(hit.transform.name);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.E) && alliance.Forces[curTeam].numberMems > 0)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out hit, 100, clickMask))
                {
                    if (hit.transform.tag == "Door" && inBattle == false)
                    {
                        int currentchar = curChar;
                        GameObject player = alliance.Forces[curTeam].getMember(curChar).role;
                        Vector3 dirCubePos = hit.transform.parent.parent.Find("DirectionCube").transform.position;
                        Vector3 dir = (hit.transform.position - dirCubePos).normalized;
                        Vector3 newPos = new Vector3(hit.transform.position.x + dir.x * 10, player.transform.position.y, hit.transform.position.z + dir.z * 10);
                        int roox = (int)Mathf.Round((dirCubePos.x + (40 * dir.x)) / 40f);
                        int rooy = (int)Mathf.Round((dirCubePos.z + (40 * dir.z)) / 40f);
                        Transform ListenText = WorldCanvas.Find("ListenText");
                        if (enemyBasePosition.Equals(new Position(roox, rooy)))
                        {
                            ListenText.Find("Text").GetComponent<TextMeshProUGUI>().text = "You found the enemy base! Enter this room to start a raid";
                            ListenText.position = hit.transform.position - hit.transform.right;
                            ListenText.LookAt(hit.transform.position);
                            ListenText.GetComponent<Animator>().Play("ListenText", 0, 0);
                        }
                        else if (roox > -1 && roox < boardSize && rooy > -1 && rooy < boardSize)
                        {
                            bool enemyHere = false;
                            Position thisPos = new Position(roox, rooy);
                            for (int i = 0; i < max_Forces; i++)
                            {
                                if (enemyAlliance.Forces[i].getPosition().Equals(thisPos))
                                {
                                    enemyHere = true;
                                    break;
                                }
                            }
                            if (maze.roomName[roox, rooy] == RoomType.Enemy || enemyHere)
                            {
                                ListenText.Find("Text").GetComponent<TextMeshProUGUI>().text = "You hear something passed this entrance";
                            }
                            else
                            {
                                ListenText.Find("Text").GetComponent<TextMeshProUGUI>().text = "Nothing is heard passed this entrance";
                            }
                            ListenText.position = hit.transform.position - hit.transform.right;
                            ListenText.LookAt(hit.transform.position);
                            ListenText.GetComponent<Animator>().Play("ListenText", 0, 0);
                        }
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.L) && alliance.Forces[curTeam].numberMems > 0)
            {
                switchChar(alliance.Forces[curTeam], curChar, (curChar + 1) % max_Team);
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                buttonPrevTeam();
            }
        }
    }

    //
    // MAKES THE ARROW POINT AT THE CURRENT ACTIVE PLAYER

    public void changeArrow(int i, bool firstPics)
    {
        Transform Canvas = canvas.transform;
        if (!firstPics)
        {
            Canvas.Find("Arrow").rotation = Quaternion.Euler(0, 0, -90);
            Canvas.Find("Arrow").position = Canvas.transform.Find("HealthPics").Find("Enemy").Find("" + i).Find("ArrowPos").position;
        }
        else if (inBattle)
        {
            Canvas.Find("Arrow").rotation = Quaternion.Euler(0, 0, 90);
            Canvas.Find("Arrow").position = Canvas.transform.Find("HealthPics").Find("Character").Find("" + i).Find("ArrowPos").position;
        }
        else
        {
            Canvas.Find("Arrow").rotation = Quaternion.Euler(0, 0, 90);
            Canvas.Find("Arrow").position = OutOfBattleGUI.transform.Find("CharPics").Find("TeamStats").Find("" + i).position;
        }
    }

    //
    // ADDS A KNIGHT TO THE PLAYER TEAM (TESTING FUNCTION)

    public void playerbuttonAction()
    {
        string itemName = "Characters/Knight";
        GameObject play = Resources.Load(itemName) as GameObject;
        if (alliance.Forces[curTeam].numberMems < max_Team)
        {
            alliance.Forces[curTeam].addMember(Class.Knight);
        }
        updateCharPic(curTeam);
    }

    public void buttonNextTeam()
    {
        switchTeams(alliance.Forces, true);
    }

    public void buttonPrevTeam()
    {
        switchTeams(alliance.Forces, false);
    }

    //
    // TELEPORTS THE PLAYER TEAM TO ROOM 0,0 (TESTING FUNCTION)

    public void teleportButton()
    {
        if (!inBattle && playerTurn)
        {
            alliance.Forces[curTeam].getMember(curChar).role.transform.position = new Vector3(0, 3, 0);
            moveRooms(0, 0);
        }
    }

    //
    // ADDS A KNIGHT TO THE AI TEAM (TESTING FUNCTION)

    public void enemybuttonAction()
    {
        if (enemyAlliance.Forces[curTeam].numberMems < max_Team)
        {
            enemyAlliance.Forces[curTeam].addMember(Class.Knight);
            enemyAlliance.Forces[curTeam].getMember(enemyAlliance.Forces[curTeam].numberMems - 1).setSpeedCap(25);
        }
    }

    //
    // REMOVES A MEMBER FROM THE PLAYER TEAM, AND UPDATES THE RELEVANT UI

    void removeTeamMember(int i)
    {
        if (alliance.Forces[curTeam].getMember(i).role != null)
        {
            PlayerScript plrScrt = gameObject.GetComponent<PlayerScript>();
            Destroy(alliance.Forces[curTeam].getMember(i).role);
            alliance.Forces[curTeam].getMember(i).resetValues();
            alliance.Forces[curTeam].getMember(i).role = null;
            switchChar(alliance.Forces[curTeam], curChar, (i + 1) % max_Team);
            alliance.Forces[curTeam].numberMems--;
            updateCharPic(curTeam);
        }
    }

    //
    // SWITCHES TO ANOTHER CHARACTER, UPDATING UI

    void switchChar(Team team, int currentCharacter, int nextCharacter)
    {
        PlayerScript plrScrt = gameObject.GetComponent<PlayerScript>();
        Vector3 cpos = new Vector3(team.getPosition().x*40, 1, team.getPosition().y*40+5);
        if (team.getMember(currentCharacter).role != null)
        {
            cpos = team.getMember(currentCharacter).role.transform.position;
            team.getMember(currentCharacter).role.SetActive(false);
        }
        while (team.getMember(nextCharacter).role == null)
        {
            nextCharacter = ((nextCharacter + 1) % max_Team);
        }
        changeArrow(nextCharacter, true);
        team.getMember(nextCharacter).role.SetActive(true);
        gameObject.GetComponent<PlayerScript>().enabled = true;
        plrScrt.initChar(team.getMember(nextCharacter).role);
        curChar = nextCharacter;
        for (int j = 0; j < 4; j++)
        {
            GameObject.Find("Canvas").transform.Find("CharMoves").transform.Find("" + j).GetChild(0).GetComponent<TextMeshProUGUI>().text = "" + team.getMember(curChar).getAttackMove(j).getName();
            if (alliance.Forces[curTeam].getMember(j).role != null)
            {
                OutOfBattleGUI.transform.Find("CharPics").Find("TeamStats").Find("" + j).Find("Image").GetComponent<Image>().color = new Color(0, 1, 0);
            }
            else
            {
                OutOfBattleGUI.transform.Find("CharPics").Find("TeamStats").Find("" + j).Find("Image").GetComponent<Image>().color = new Color(1, 0, 0);
            }
        }
        team.getMember(curChar).role.transform.position = cpos;
    }

    //
    // SWITCHES TO ANOTHER TEAM, UPDATING UI AND ROOM BLOCKS

    void switchTeams(Team[] teams, bool next)
    {
        PlayerScript plrScrt = gameObject.GetComponent<PlayerScript>();
        plrScrt.mainCam.transform.parent = null;
        if (teams[curTeam].getMember(curChar).role != null)
        {
            teams[curTeam].getMember(curChar).role.SetActive(false);
            gameObject.GetComponent<PlayerScript>().enabled = false;
        }

        int nextTeam = 0;
        if (next)
        {
            nextTeam = (curTeam + 1) % max_Forces;
            while (teams[nextTeam].numberMems == 0)
            {
                nextTeam = (nextTeam + 1) % max_Forces;
            }
        } else
        {
            nextTeam = (curTeam - 1) % max_Forces;
            if (nextTeam < 0) { nextTeam = max_Forces - 1; }
            while (teams[nextTeam].numberMems == 0)
            {
                nextTeam = (nextTeam - 1) % max_Forces;
                if (nextTeam < 0) { nextTeam = max_Forces - 1; }
            }
        }
        curTeam = nextTeam;
        OutOfBattleGUI.transform.Find("CharPics").transform.Find("TeamText").GetComponent<TextMeshProUGUI>().text = "Team " + nextTeam;
        if (teams[nextTeam].numberMems != 0)
        {
            int changeto = curChar;
            while (teams[nextTeam].getMember(changeto).role == null)
            {
                changeto = ((changeto + 1) % max_Team);
            }
            int roox = 0;
            int rooy = 0;
            if (teams[nextTeam].getPosition().Equals(new Position(-1, -1)))
            {
                roox = playerBasePosition.x;
                rooy = playerBasePosition.y;
                switchChar(teams[nextTeam], curChar, changeto);
                setRooms(roox, rooy, playerTurn);
                teams[nextTeam].setPosition(playerBasePosition.x, playerBasePosition.y);
                teams[nextTeam].getMember(curChar).role.transform.position = maze.roomPos[roox, rooy] + transform.forward*5;
            }
            else {
                roox = teams[nextTeam].getPosition().x;
                rooy = teams[nextTeam].getPosition().y;
                switchChar(teams[nextTeam], curChar, changeto);
                Vector3 pos = teams[nextTeam].getMember(curChar).role.transform.position;
                setRooms(roox, rooy, playerTurn);
                teams[nextTeam].getMember(curChar).role.transform.position = pos;
            }
        }
        updateCharPic(nextTeam);
    }

    //
    // UPDATES ALL RELEVANT UI RELATING TO CHARACTER STATS FOR THE ENTIRE TEAM, DOESN'T MOVE ARROW

    public void updateCharPic(int currentTeam)
    {
        Transform charIm = OutOfBattleGUI.transform.Find("CharPics").transform.Find("TeamStats");
        for (int i = 0; i < max_Team; i++)
        {
            if (alliance.Forces[currentTeam].getMember(i).role == null)
            {
                charIm.Find("" + i).Find("Experience").GetComponent<TextMeshProUGUI>().text = "" + 0;
                charIm.Find("" + i).Find("Level").GetComponent<TextMeshProUGUI>().text = "" + 0;
                charIm.Find("" + i).Find("Gold").GetComponent<TextMeshProUGUI>().text = "" + 0;
                OutOfBattleGUI.transform.Find("CharPics").Find("TeamStats").Find("" + i).Find("Image").GetComponent<Image>().color = new Color(1, 0, 0);
            }
            else
            {
                charIm.Find("" + i).Find("Experience").GetComponent<TextMeshProUGUI>().text = "" + alliance.Forces[currentTeam].getMember(i).getExperience();
                charIm.Find("" + i).Find("Level").GetComponent<TextMeshProUGUI>().text = "" + alliance.Forces[currentTeam].getMember(i).getLevel();
                charIm.Find("" + i).Find("Gold").GetComponent<TextMeshProUGUI>().text = "" + alliance.Forces[currentTeam].getMember(i).getGold();
                OutOfBattleGUI.transform.Find("CharPics").Find("TeamStats").Find("" + i).Find("Image").GetComponent<Image>().color = new Color(0, 1, 0);
            }
        }
    }

    //
    // CLASS FOR THE MAZE, HOLDING DATA SUCH AS BLOCK DATA AND EVENT TYPE FOR EACH ROOM

    class mazeArray
    {
        public string[,] room { get; set; }
        private string[,] roomEvent;
        public Vector3[,] roomPos { get; set; }
        public Quaternion[,] roomRot { get; set; }
        public RoomType[,] roomName { get; set; }
        public Faction[,] roomOccupied { get; set; }
        public bool[,] fireFlies { get; set; }
        public string[,] interior { get; set; }
        public mazeArray(int size)
        {
            room = new string[size, size];
            roomEvent = new string[size, size];
            roomPos = new Vector3[size, size];
            roomRot = new Quaternion[size, size];
            roomName = new RoomType[size, size];
            roomOccupied = new Faction[size, size];
            fireFlies = new bool[size, size];
            interior = new string[size, size];
        }
        public int getLength()
        {
            return (int)Mathf.Sqrt(this.roomPos.Length);
        }
        public void setEvent(int i, int j, string _event)
        {
            this.roomEvent[i, j] = _event;
            //this.roomEvent[i,j].transform.parent = this.room[i,j].transform;
        }
        public string getEvent(int i, int j)
        {
            return this.roomEvent[i, j];
        }
    }

    void battleInitValues(Team firstTeam, Team eTeam, int x, int y)
    {
        alliance.Forces[curTeam].getMember(curChar).role.GetComponent<Rigidbody>().velocity = Vector3.zero;
        Transform Canvas = GameObject.Find("Canvas").transform.Find("HealthPics");
        OutOfBattleGUI.SetActive(false);
        if (firstTeam.ai)
        {
            alliance.Forces[curTeam].getMember(curChar).role.SetActive(false);
        }

        int ydiff = 0;
        int secondydiff = 0;
        if ((y - prevy) == -1)
        {
            ydiff = 180;
        }
        else if ((y - prevy) == 1)
        {
            secondydiff = 180;
        }
        WorldCanvas.position = new Vector3(x * 40, 7.0f, y * 40);
        WorldCanvas.rotation = Quaternion.Euler(0, (y - prevy) * 90, 0);
        if ((x - prevx) == 1) { WorldCanvas.Rotate(0, 180, 0); }

        GameObject[] yourPanels = new GameObject[max_Team];
        GameObject[] enemyPanels = new GameObject[max_Team];

        for (int i = 0; i < max_Team; i++)
        {
            Canvas.gameObject.SetActive(true);
            Vector3 placePosition = new Vector3(maze.roomPos[x, y].x + (x - prevx) * (7 + (3 * i)), 2.0f, maze.roomPos[x, y].z + (y - prevy) * (7 + (3 * i)));
            if (eTeam.getMember(i).role != null)
            {
                WorldCanvas.Find("Second").GetChild(i).Find("Attack").GetComponent<TextMeshProUGUI>().text = (100 + eTeam.getMember(i).getAttackMult()) + "";
                WorldCanvas.Find("Second").GetChild(i).Find("Defence").GetComponent<TextMeshProUGUI>().text = (100 + eTeam.getMember(i).getDefenceMult()) + "";
                WorldCanvas.Find("Second").GetChild(i).Find("Poison").GetComponent<TextMeshProUGUI>().text = "0";
                eTeam.getMember(i).setPoisonCounter(0);
                WorldCanvas.Find("Second").GetChild(i).Find("Dazed").GetComponent<TextMeshProUGUI>().text = "0";
                eTeam.getMember(i).setSwapSafety(0);
                WorldCanvas.Find("Second").GetChild(i).gameObject.SetActive(true);
                eTeam.getMember(i).role.transform.rotation = Quaternion.Euler(0, secondydiff + ((x - prevx) * -90), 0);
                eTeam.getMember(i).role.transform.position = placePosition;
                eTeam.getMember(i).role.SetActive(true);
                Animator animator = eTeam.getMember(i).role.transform.GetChild(0).GetComponent<Animator>();
                animator.SetBool("Equipping", true);
                animator.SetBool("InBattle", true);
            }
            enemyPanels[i] = Instantiate(Resources.Load("Events/Panel") as GameObject, placePosition - Vector3.up * 1.5f, Quaternion.identity);
            Slider HealthBar = Canvas.Find("Enemy").transform.Find("" + i).Find("HealthBar").GetComponent<Slider>();

            HealthBar.maxValue = eTeam.getMember(i).getHealthCap();
            HealthBar.value = eTeam.getMember(i).getHealth();
            HealthBar.transform.Find("HealthText").GetComponent<TextMeshProUGUI>().text = "" + HealthBar.value + " / " + HealthBar.maxValue;
            Canvas.Find("Enemy").transform.Find("" + i).Find("Level").GetChild(0).GetComponent<TextMeshProUGUI>().text = "" + eTeam.getMember(i).getLevel();

            placePosition = new Vector3(maze.roomPos[x, y].x - (x - prevx) * (7 + (3 * i)), 2.0f, maze.roomPos[x, y].z - (y - prevy) * (7 + (3 * i)));
            if (firstTeam.getMember(i).role != null)
            {
                WorldCanvas.Find("First").GetChild(i).Find("Attack").GetComponent<TextMeshProUGUI>().text = (100 + firstTeam.getMember(i).getAttackMult()) + "";
                WorldCanvas.Find("First").GetChild(i).Find("Defence").GetComponent<TextMeshProUGUI>().text = (100 + firstTeam.getMember(i).getDefenceMult()) + "";
                WorldCanvas.Find("First").GetChild(i).Find("Poison").GetComponent<TextMeshProUGUI>().text = "0";
                firstTeam.getMember(i).setPoisonCounter(0);
                WorldCanvas.Find("First").GetChild(i).Find("Dazed").GetComponent<TextMeshProUGUI>().text = "0";
                firstTeam.getMember(i).setSwapSafety(0);
                WorldCanvas.Find("First").GetChild(i).gameObject.SetActive(true);
                firstTeam.getMember(i).role.transform.rotation = Quaternion.Euler(0, ydiff + ((x - prevx) * 90), 0);
                firstTeam.getMember(i).role.transform.position = placePosition;
                firstTeam.getMember(i).role.SetActive(true);
                Animator animator = firstTeam.getMember(i).role.transform.GetChild(0).GetComponent<Animator>();
                animator.SetBool("Equipping", true);
                animator.SetBool("InBattle", true);
            }
            yourPanels[i] = Instantiate(Resources.Load("Events/Panel") as GameObject, placePosition - Vector3.up * 1.5f, Quaternion.identity);
            HealthBar = Canvas.Find("Character").transform.Find("" + i).Find("HealthBar").GetComponent<Slider>();

            HealthBar.maxValue = firstTeam.getMember(i).getHealthCap();
            HealthBar.value = firstTeam.getMember(i).getHealth();
            HealthBar.transform.Find("HealthText").GetComponent<TextMeshProUGUI>().text = "" + HealthBar.value + " / " + HealthBar.maxValue;
            Canvas.Find("Character").transform.Find("" + i).Find("Level").GetChild(0).GetComponent<TextMeshProUGUI>().text = "" + firstTeam.getMember(i).getLevel();
        }
        CharMovesScript.yourPanels = yourPanels;
        CharMovesScript.enemyPanels = enemyPanels;
    }

    //
    // BEGINS A BATTLE, SETS HEALTHBARS AND POSITIONS TEAM MEMBERS ON THE BATTLEFIELD
    IEnumerator battleSystem(Team firstTeam, Team eTeam, int x, int y, Vector3 camPosFinal, Quaternion camRotFinal)
    {
        if(playerEvent != null)
        {
            playerEvent.SetActive(false);
        }
        canvas.transform.Find("OpenMenu").Find("OptionsMenu").Find("WatchAIToggle").GetComponent<Toggle>().interactable = false;
        aiBattle = (firstTeam.ai && eTeam.ai) ? true : false;
        gameObject.GetComponent<AudioSystem>().PlayBattleMusic();
        Transform HealthCanvas = GameObject.Find("Canvas").transform.Find("HealthPics");
        battleInitValues(firstTeam, eTeam, x, y);

        Transform cam = Camera.main.transform;
        yield return StartCoroutine(MoveCam(Camera.main, 1.0f, cam.position, camPosFinal, cam.rotation, camRotFinal));

        Member playerFast = firstTeam.calculateFastest();
        GameObject.Find("Canvas").transform.Find("CharMoves").gameObject.SetActive(!firstTeam.ai);

        while (inBattle)
        {
            if ((firstTeam.numberMems != 0) && (eTeam.numberMems != 0))
            {
                yield return Attacking(firstTeam, eTeam, x, y);
            }
            else
            {
                canvas.transform.Find("CharMoves").gameObject.SetActive(false);
                HealthCanvas.gameObject.SetActive(false);
                for (int i = 0; i < max_Team; i++)
                {
                    WorldCanvas.Find("First").GetChild(i).gameObject.SetActive(false);
                    WorldCanvas.Find("Second").GetChild(i).gameObject.SetActive(false);
                    if (eTeam.getMember(i).role != null)
                    {
                        eTeam.getMember(i).role.SetActive(false);
                    }
                }
                yield return endBattle(firstTeam, x, y);
            }
        }

        foreach (Member character in firstTeam.getTeam())
        {
            if (character.role != null)
            {
                int index = character.getIndex();
                firstTeam.getMember(index).addExperience(firstTeam.getMember(index).getDamageDone(), firstTeam.ai);
                firstTeam.getMember(index).setDamageDone(0);
            }
        }

        if (!firstTeam.ai)
        {
            if(firstTeam.numberMems <= 0)
            {
                yield return null;
                switchTeams(alliance.Forces, true);
            }
            updateCharPic(curTeam);
        }

        canvas.transform.Find("OpenMenu").Find("OptionsMenu").Find("WatchAIToggle").GetComponent<Toggle>().interactable = true;
        yield return null;
    }

    //
    // BEGINS A RAID, ENDING ONLY WHEN AN ALLIANCE HAS RUN OUT OF USABLE TEAMS
    IEnumerator StartRaid(Teams firstAlliance, EnemyTeams eAlliance, int x, int y, Vector3 camPosFinal, Quaternion camRotFinal)
    {
        raiding = true;
        int i = 0;
        int j = 0;
        int posX = x;
        int posY = y;
        while (true)
        {
            while(i < max_Forces && firstAlliance.Forces[i].numberMems == 0)
            {
                i++;
            }
            while(j < max_Forces && eAlliance.Forces[j].numberMems == 0)
            {
                j++;
            }
            if (i >= max_Forces || j >= max_Forces) break;
            eCurTeam = j;
            inBattle = true;
            yield return battleSystem(firstAlliance.Forces[i], eAlliance.Forces[j], posX, posY, camPosFinal, camRotFinal);
        }
        yield return null;
    }

    //
    // SMOOTHLY TRANSITIONS SLIDER VALUES FROM POINT TO POINT

    IEnumerator SliderAnimator(Slider slider, float startVal, float finalVal)
    {
        float timer = 0.0f;
        while (timer < 1.0f)
        {
            slider.value = (startVal * (1.0f - timer)) + (finalVal * timer);
            timer += Time.deltaTime * 3;
            yield return null;
        }
        slider.value = finalVal;
    }

    //
    // SMOOTHLY UPDATES SPEED SLIDERS AFTER EACH ATTACK 

    void UpdateSpeedPics(Team teamOne, Team teamTwo)
    {
        Transform Canvas = GameObject.Find("Canvas").transform.Find("HealthPics");
        Slider speedBar = null;
        for (int i = 0; i < max_Team; i++)
        {
            speedBar = Canvas.Find("Character").transform.Find("" + i).transform.Find("SpeedBar").GetComponent<Slider>();
            if (teamOne.getMember(i).role == null)
            {
                speedBar.value = 0;
                speedBar.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = "0/0";
            }
            else
            {
                float finalValue = 100 - (((float)teamOne.getMember(i).getSpeed() / teamOne.getMember(i).getSpeedCap()) * 100);
                StartCoroutine(SliderAnimator(speedBar, speedBar.value, finalValue));
                speedBar.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = "" + (teamOne.getMember(i).getSpeedCap() - teamOne.getMember(i).getSpeed()) + " / " + teamOne.getMember(i).getSpeedCap();
            }
            speedBar = Canvas.Find("Enemy").transform.Find("" + i).transform.Find("SpeedBar").GetComponent<Slider>();
            if (teamTwo.getMember(i).role == null)
            {
                speedBar.value = 0;
                speedBar.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = "0/0";
            }
            else
            {
                float finalValue = 100 - (((float)teamTwo.getMember(i).getSpeed() / teamTwo.getMember(i).getSpeedCap()) * 100);
                StartCoroutine(SliderAnimator(speedBar, speedBar.value, finalValue));
                speedBar.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = "" + (teamTwo.getMember(i).getSpeedCap() - teamTwo.getMember(i).getSpeed()) + " / " + teamTwo.getMember(i).getSpeedCap();
            }
        }
    }

    //
    // HANDLES WHICH SPACES GET ATTACKED BY AN ATTACK MOVE

    IEnumerator Attack(Member member, Team thisTeam, Team targetedTeam, AttackMove move, int x, int y, bool firstTeamTarget)
    {
        Transform Canvas = canvas.transform.Find("HealthPics").Find("Character");
        GameObject camMain = Camera.main.gameObject;
        Transform hitSplat = WorldCanvas.Find("HitSplat");
        bool physical = true;
        bool hit = false;
        bool skipCutscene = (aiBattle && skipAIBattles) ? true : false;
        GameObject CameraOneTemp = CameraOne;
        GameObject CameraTwoTemp = CameraTwo;
        Camera CamTwo;
        Camera CamOne;
        int shiftAmount = 0;

        if (!firstTeamTarget)
        {
            Canvas = canvas.transform.Find("HealthPics").Find("Enemy");
        }

        #region CAMERA SETUP
        EnemyAttackCS.transform.rotation = Quaternion.Euler(0, 0, 0);
        CharacterAttackCS.transform.rotation = Quaternion.Euler(0, 0, 0);

        if (move.isOffensive())
        {
            if (firstTeamTarget)
            {
                CameraOneTemp = CameraTwo;
                CameraTwoTemp = CameraOne;
                EnemyAttackCS.transform.rotation = Quaternion.Euler(0, 180, 0);
                CharacterAttackCS.transform.rotation = Quaternion.Euler(0, 180, 0);
            }
            CamTwo = CameraTwoTemp.transform.GetChild(0).GetComponent<Camera>();
            CamOne = CameraOneTemp.transform.GetChild(0).GetComponent<Camera>();

            Color backColor = new Color(231, 56, 68, 255) / 255f;
            shiftAmount = move.getPosition() - member.getIndex();
            CamOne.backgroundColor = backColor;
            CamTwo.backgroundColor = backColor;
        }
        else
        {
            CamTwo = CameraTwoTemp.transform.GetChild(0).GetComponent<Camera>();
            CamOne = CameraOneTemp.transform.GetChild(0).GetComponent<Camera>();

            Color backColor = new Color(145, 195, 125, 255) / 255f;
            shiftAmount = member.getIndex() - move.getPosition();
            CamOne.backgroundColor = backColor;
            CamTwo.backgroundColor = backColor;
        }

        if (!skipCutscene)
        {
            CamOne.gameObject.SetActive(true);
            CamTwo.gameObject.SetActive(true);
        }

        if (move.getName() != "Pass")
        {
            CameraOneTemp.transform.position = camMain.transform.position;
            CameraOneTemp.transform.rotation = camMain.transform.rotation;
            CameraTwoTemp.transform.position = -Vector3.up;
            CameraTwoTemp.transform.rotation = Quaternion.Euler(0, 0, 180);
            Vector3 finalPos = member.role.transform.position + member.role.transform.forward * 6 - member.role.transform.right * 5 + member.role.transform.up * 4;

            if (!skipCutscene)
            {
                EnemyAttackCS.SetActive(true);
                CharacterAttackCS.SetActive(true);
                TextAttackCS.SetActive(true);
            }

            TextAttackCS.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = move.getName();
            if (move.isOffensive())
            {
                StartCoroutine(MoveCam(CamOne, 3.0f, camMain.transform.position, finalPos, CameraOneTemp.transform.rotation, Quaternion.LookRotation((member.role.transform.position + member.role.transform.up*2.5f) - finalPos)));
                member.role.transform.GetChild(0).GetComponent<Animator>().SetBool("Attacking", true);
            }

            #endregion

            float attackMult = member.getAttackMult()/100.0f;
            for (int i = 0; i < max_Team; i++)
            {
                if (move.getSpaces()[i] == true)
                {
                    int attackIndex = i + shiftAmount;
                    if (attackIndex < 0) continue;
                    if (attackIndex > 3) continue;
                    float defenceMult = targetedTeam.getMember(attackIndex).getDefenceMult() / 100.0f;
                    int damageAmount = (int)(move.getPower() * (1.0f + attackMult) / (1.0f + defenceMult));
                    if (targetedTeam.getMember(attackIndex).role != null)
                    {
                        #region OPPOSITE TEAM CUTSCENE
                        if (move.isOffensive())
                        {
                            member.role.transform.Find("AS").GetComponent<AudioSource>().Play();
                        }
                        else
                        {
                            member.role.transform.Find("HS").GetComponent<AudioSource>().Play();
                        }

                        if (!hit)
                        {
                            CameraTwoTemp.transform.position = camMain.transform.position; CameraTwoTemp.transform.rotation = camMain.transform.rotation; hit = true;
                        }

                        finalPos = targetedTeam.getMember(attackIndex).role.transform.position + targetedTeam.getMember(attackIndex).role.transform.right * 5 + targetedTeam.getMember(attackIndex).role.transform.up * 4;

                        if (move.getSpecial() != Special.AttackBuff && move.getSpecial() != Special.DefenseBuff && move.getSpecial() != Special.Pull)
                        {
                            Transform memPos = targetedTeam.getMember(attackIndex).role.transform;
                            float yRot = 51.34f;
                            Vector3 posAdjust = memPos.forward + memPos.right;
                            if (!move.isOffensive())
                            {
                                yRot = -51.34f;
                                posAdjust = memPos.forward - memPos.right;
                                hitSplat.GetComponent<Image>().color = new Color(1, 0, 1, 1);
                                hitSplat.GetChild(0).GetComponent<TextMeshProUGUI>().text = "" + move.getPower();
                            }
                            else
                            {
                                hitSplat.GetComponent<Image>().color = new Color(1, 0, 0, 1);
                                hitSplat.GetChild(0).GetComponent<TextMeshProUGUI>().text = "" + damageAmount;
                            }
                            if (firstTeamTarget)
                            {
                                hitSplat.position = memPos.position + Vector3.up * 2.5f + posAdjust;
                                hitSplat.localRotation = Quaternion.Euler(15.184f, -yRot, 0);
                            } else
                            {
                                hitSplat.position = memPos.position + Vector3.up * 2.5f + memPos.forward + memPos.right;
                                hitSplat.localRotation = Quaternion.Euler(-15.184f, -51.34f, 0);
                            }

                            hitSplat.localScale = Vector3.one;
                        }

                        if (move.isOffensive() || !firstTeamTarget)
                        {
                            finalPos += targetedTeam.getMember(attackIndex).role.transform.forward * 6;
                            Quaternion camLook = Quaternion.LookRotation((targetedTeam.getMember(attackIndex).role.transform.position + targetedTeam.getMember(attackIndex).role.transform.up * 2.5f) - finalPos);
                            yield return MoveCam(CamTwo, 3.0f, CameraTwoTemp.transform.position, finalPos, CameraTwoTemp.transform.rotation, camLook);
                            if (move.isOffensive())
                            {
                                CameraTwoTemp.GetComponent<Animator>().Play("CamHit");
                                if (!skipCutscene)
                                {
                                    yield return new WaitForSeconds(0.45f);
                                }
                            }
                        }
                        else
                        {
                            finalPos += targetedTeam.getMember(attackIndex).role.transform.forward * 6 - targetedTeam.getMember(attackIndex).role.transform.right * 10;
                            yield return MoveCam(CamOne, 3.0f, CameraOneTemp.transform.position, finalPos, CameraOneTemp.transform.rotation, Quaternion.LookRotation((targetedTeam.getMember(attackIndex).role.transform.position + targetedTeam.getMember(attackIndex).role.transform.up * 2.5f) - finalPos));
                        }

                        #endregion
                        if (!skipCutscene)
                        {
                            yield return new WaitForSeconds(0.4f);
                        }

                        #region ATTACK ACTION

                        if(move.getPower() == 0) { physical = false; }

                        if(physical)
                        {
                            if (move.isOffensive())
                            {
                                if (targetedTeam.getMember(attackIndex).getHealth() <= damageAmount)
                                {
                                    member.addGold(targetedTeam.getMember(attackIndex).getGold());
                                    targetedTeam.getMember(attackIndex).setGold(0);
                                    if (firstTeamTarget)
                                    {
                                        WorldCanvas.Find("First").GetChild(attackIndex).gameObject.SetActive(false);
                                    }
                                    else
                                    {
                                        WorldCanvas.Find("Second").GetChild(attackIndex).gameObject.SetActive(false);
                                    }
                                    targetedTeam.getMember(attackIndex).resetDefenceMult();
                                    targetedTeam.dealDamage(damageAmount, attackIndex);
                                }
                                else
                                {
                                    targetedTeam.dealDamage(damageAmount, attackIndex);
                                    targetedTeam.getMember(attackIndex).resetDefenceMult();
                                    if (firstTeamTarget)
                                    {
                                        WorldCanvas.Find("First").GetChild(attackIndex).Find("Defence").GetComponent<TextMeshProUGUI>().text = (100 + targetedTeam.getMember(attackIndex).getDefenceMult()) + "";
                                    }
                                    else
                                    {
                                        WorldCanvas.Find("Second").GetChild(attackIndex).Find("Defence").GetComponent<TextMeshProUGUI>().text = (100 + targetedTeam.getMember(attackIndex).getDefenceMult()) + "";
                                    }
                                }
                            }
                            else
                            {
                                targetedTeam.heal(move.getPower(), attackIndex);
                                member.healCooldown = true;
                            }
                        }

                        if (move.getSpecial() != Special.None)
                        {
                            switch (move.getSpecial())
                            {
                                case Special.Pull:
                                    if (targetedTeam.getMember(attackIndex).getSwapSafety() == 0)
                                    {
                                        targetedTeam.getMember(attackIndex).setSwapSafety(2);
                                        swapTeam(targetedTeam, attackIndex, attackIndex - move.getSpecialPower(), x, y, firstTeamTarget);
                                        if (firstTeamTarget)
                                        {
                                            WorldCanvas.Find("First").GetChild(attackIndex).Find("Dazed").GetComponent<TextMeshProUGUI>().text = "" + targetedTeam.getMember(attackIndex).getSwapSafety();
                                        }
                                        else
                                        {
                                            WorldCanvas.Find("Second").GetChild(attackIndex).Find("Dazed").GetComponent<TextMeshProUGUI>().text = "" + targetedTeam.getMember(attackIndex).getSwapSafety();
                                        }
                                    }
                                    break;
                                case Special.AttackBuff:
                                    thisTeam.getMember(attackIndex).addAttackMult(move.getSpecialPower());
                                    if (firstTeamTarget)
                                    {
                                        WorldCanvas.Find("First").GetChild(attackIndex).Find("Attack").GetComponent<TextMeshProUGUI>().text = (100 + targetedTeam.getMember(attackIndex).getAttackMult()) + "";
                                    }
                                    else
                                    {
                                        WorldCanvas.Find("Second").GetChild(attackIndex).Find("Attack").GetComponent<TextMeshProUGUI>().text = (100 + targetedTeam.getMember(attackIndex).getAttackMult()) + "";
                                    }
                                    break;
                                case Special.DefenseBuff:
                                    thisTeam.getMember(attackIndex).addDefenceMult(move.getSpecialPower());
                                    if (firstTeamTarget)
                                    {
                                        WorldCanvas.Find("First").GetChild(attackIndex).Find("Defence").GetComponent<TextMeshProUGUI>().text = (100 + targetedTeam.getMember(attackIndex).getDefenceMult()) + "";
                                    }
                                    else
                                    {
                                        WorldCanvas.Find("Second").GetChild(attackIndex).Find("Defence").GetComponent<TextMeshProUGUI>().text = (100 + targetedTeam.getMember(attackIndex).getDefenceMult()) + "";
                                    }
                                    break;
                                case Special.Poison:
                                    int poisonDamage = Mathf.RoundToInt(move.getSpecialPower());
                                    if (poisonDamage < 1) poisonDamage = 1;
                                    targetedTeam.getMember(attackIndex).addPoisonCounter(poisonDamage);
                                    if (firstTeamTarget)
                                    {
                                        WorldCanvas.Find("First").GetChild(attackIndex).Find("Poison").GetComponent<TextMeshProUGUI>().text = "" + targetedTeam.getMember(attackIndex).getPoisonCounter();
                                    }
                                    else
                                    {
                                        WorldCanvas.Find("Second").GetChild(attackIndex).Find("Poison").GetComponent<TextMeshProUGUI>().text = "" + targetedTeam.getMember(attackIndex).getPoisonCounter();
                                    }
                                    break;
                                case Special.Clean:
                                    thisTeam.getMember(attackIndex).setPoisonCounter(0);
                                    if (firstTeamTarget)
                                    {
                                        WorldCanvas.Find("First").GetChild(attackIndex).Find("Poison").GetComponent<TextMeshProUGUI>().text = "0";
                                    }
                                    else
                                    {
                                        WorldCanvas.Find("Second").GetChild(attackIndex).Find("Poison").GetComponent<TextMeshProUGUI>().text = "0";
                                    }
                                    break;
                            }
                        }

                        Slider HealthBar = Canvas.Find("" + attackIndex).Find("HealthBar").GetComponent<Slider>();
                        HealthBar.value = targetedTeam.getMember(attackIndex).getHealth();
                        HealthBar.transform.Find("HealthText").GetComponent<TextMeshProUGUI>().text = "" + HealthBar.value + " / " + HealthBar.maxValue;
                        if(move.getSpecial() != Special.AttackBuff && move.getSpecial() != Special.DefenseBuff)
                        {
                            member.addDamageDone(move.getPower());
                        }

                        #endregion

                    }
                }
            }
            #region CUTSCENE END
            if (!hit) {
                TextAttackCS.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Missed!";
                if (!skipCutscene) { yield return new WaitForSeconds(1.5f); }
            }
            hitSplat.localScale = Vector3.zero;
            EnemyAttackCS.SetActive(false);
            CharacterAttackCS.SetActive(false);
            TextAttackCS.SetActive(false);

            if (move.isOffensive())
            {
                if (physical)
                {
                    member.resetAttackMult();
                    if (firstTeamTarget)
                    {
                        WorldCanvas.Find("Second").GetChild(member.getIndex()).Find("Attack").GetComponent<TextMeshProUGUI>().text = (100 + member.getAttackMult()) + "";
                    }
                    else
                    {
                        WorldCanvas.Find("First").GetChild(member.getIndex()).Find("Attack").GetComponent<TextMeshProUGUI>().text = (100 + member.getAttackMult()) + "";
                    }
                }
            }
            #endregion
        }
        CamOne.gameObject.SetActive(false);
        CamTwo.gameObject.SetActive(false);
        yield return null;
    }

    //
    // HANDLES WHICH TEAM AND WHICH MEMBER IS ATTACKING THIS TURN

    IEnumerator Attacking(Team firstTeam, Team eTeam, int x, int y)
    {

        Member member = firstTeam.calculateFastest();
        Member eFast = eTeam.calculateFastest();
        if (member.getSpeed() <= eFast.getSpeed())
        {
            #region First Team Fastest
            //SUBTRACTING SPEED OF BOTH TEAMS FROM FASTEST
            eTeam.subtractSpeeds(member.getSpeed());
            firstTeam.subtractSpeeds(member.getSpeed());
            UpdateSpeedPics(firstTeam, eTeam);
            if (member.getSwapSafety() > 0)
            {
                member.addSwapSafety(-1);
                WorldCanvas.Find("First").GetChild(member.getIndex()).Find("Dazed").GetComponent<TextMeshProUGUI>().text = "" + member.getSwapSafety();
            }
            //RESETING SPEED FOR FASTEST PLAYER
            member.resetSpeed();
            if (firstTeam.ai == false)
            {
                curChar = member.getIndex();
                changeArrow(curChar, true);
                Transform CharMoves = GameObject.Find("Canvas").transform.Find("CharMoves");
                for (int j = 0; j < 5; j++)
                {
                    CharMoves.Find("" + j).GetChild(0).GetComponent<TextMeshProUGUI>().text = "" + firstTeam.getMember(curChar).getAttackMove(j).getName();
                }
                if (member.getSwapSafety() == 0)
                {
                    CharMoves.Find("SwapPlayer").gameObject.SetActive(true);
                } else
                {
                    CharMoves.Find("SwapPlayer").gameObject.SetActive(false);
                }
                CharMoves.gameObject.SetActive(true);
            }
            else
            {
                changeArrow(member.getIndex(), true);
                if (!skipAIBattles)
                {
                    yield return new WaitForSeconds(0.4f);
                }
                chosenMove = AIAttackDecision(member, (EnemyTeam)firstTeam, eTeam, true);
                if (!skipAIBattles)
                {
                    yield return new WaitForSeconds(0.4f);
                }
            }
            playerPhase = true;
            while (playerPhase)
            {
                if (!inBattle)
                {
                    break;
                }
                if (chosenMove != null)
                {
                    if (swapping) { cancelSwap = true; }
                    member.healCooldown = false;

                    if (chosenMove.isOffensive())
                    {
                        yield return Attack(member, firstTeam, eTeam, chosenMove, x, y, false);
                    }
                    else
                    {
                        yield return Attack(member, firstTeam, firstTeam, chosenMove, x, y, true);
                    }
                    playerPhase = false;
                    chosenMove = null;
                } 
                yield return null;
            }

            if (member.getPoisonCounter() > 0)
            {
                if (member.getHealth() <= member.getPoisonCounter())
                {
                    for (int i = 0; i < max_Team; i++)
                    {
                        if (eTeam.getMember(i).role != null)
                        {
                            eTeam.getMember(i).addGold(member.getGold());
                        }
                    }
                    member.setGold(0);
                    WorldCanvas.Find("First").GetChild(member.getIndex()).gameObject.SetActive(false);
                }
                firstTeam.dealDamage(member.getPoisonCounter(), member.getIndex());
                Slider HealthBar = canvas.transform.Find("HealthPics").Find("Character").Find("" + member.getIndex()).Find("HealthBar").GetComponent<Slider>();
                HealthBar.value = member.getHealth();
                HealthBar.transform.Find("HealthText").GetComponent<TextMeshProUGUI>().text = "" + HealthBar.value + " / " + HealthBar.maxValue;
                member.addPoisonCounter(-1);
                WorldCanvas.Find("First").GetChild(member.getIndex()).Find("Poison").GetComponent<TextMeshProUGUI>().text = "" + member.getPoisonCounter();
            }
            #endregion
        }
        else
        {
            #region Second Team Fastest
            //SUBTRACTING SPEED OF BOTH TEAMS FROM FASTEST
            firstTeam.subtractSpeeds(eFast.getSpeed());
            eTeam.subtractSpeeds(eFast.getSpeed());
            UpdateSpeedPics(firstTeam, eTeam);
            if (eFast.getSwapSafety() > 0)
            {
                eFast.addSwapSafety(-1);
                WorldCanvas.Find("Second").GetChild(eFast.getIndex()).Find("Dazed").GetComponent<TextMeshProUGUI>().text = "" + eFast.getSwapSafety();
            }
            //RESETING SPEED FOR FASTEST PLAYER
            eFast.resetSpeed();

            changeArrow(eFast.getIndex(), false);
            GameObject.Find("Canvas").transform.Find("CharMoves").gameObject.SetActive(false);
            if (eFast != null)
            {
                if(!skipAIBattles || !firstTeam.ai)
                {
                    yield return new WaitForSeconds(0.4f);
                }
                AttackMove eMove = AIAttackDecision(eFast, (EnemyTeam)eTeam, firstTeam, false);
                if(!skipAIBattles || !firstTeam.ai)
                {
                    yield return new WaitForSeconds(0.4f);
                }
                eFast.healCooldown = false;
                if (eMove.isOffensive())
                {
                    yield return Attack(eFast, eTeam, firstTeam, eMove, x, y, true);
                }
                else
                {
                    yield return Attack(eFast, eTeam, eTeam, eMove, x, y, false);
                }
            }

            if (eFast.getPoisonCounter() > 0)
            {
                if (eFast.getHealth() <= eFast.getPoisonCounter())
                {
                    for (int i = 0; i < max_Team; i++)
                    {
                        if (firstTeam.getMember(i).role != null)
                        {
                            firstTeam.getMember(i).addGold(member.getGold());
                        }
                    }
                    eFast.setGold(0);
                    WorldCanvas.Find("Second").GetChild(member.getIndex()).gameObject.SetActive(false);
                }
                eTeam.dealDamage(eFast.getPoisonCounter(), eFast.getIndex());
                Slider HealthBar = canvas.transform.Find("HealthPics").Find("Enemy").Find("" + eFast.getIndex()).Find("HealthBar").GetComponent<Slider>();
                HealthBar.value = eFast.getHealth();
                HealthBar.transform.Find("HealthText").GetComponent<TextMeshProUGUI>().text = "" + HealthBar.value + " / " + HealthBar.maxValue;
                eFast.addPoisonCounter(-1);
                WorldCanvas.Find("Second").GetChild(eFast.getIndex()).Find("Poison").GetComponent<TextMeshProUGUI>().text = "" + eFast.getPoisonCounter();
            }
            #endregion
        }
        yield return null;
    }

    //
    // HANDLES THE END OF THE BATTLE, REMOVING ANY UNNECESSARY ACTORS

    IEnumerator endBattle(Team team, int x, int y)
    {
        bool noTeamsLeft = true;
        bool noEnemyTeamsLeft = true;
        CharMovesScript.destroyPanels();
        gameObject.GetComponent<AudioSystem>().PlayAmbientMusic();

        for (int i = 0; i < max_Forces; i++)
        {
            if (alliance.Forces[i].numberMems != 0)
            {
                noTeamsLeft = false;
            }
            if (enemyAlliance.Forces[i].numberMems != 0)
            {
                noEnemyTeamsLeft = false;
            }
        }

        if (noTeamsLeft)
        {
            canvas.transform.Find("Arrow").gameObject.SetActive(false);
            canvas.transform.Find("TurnText").GetComponent<TextMeshProUGUI>().text = "You lost!";
            canvas.transform.Find("TurnText").GetComponent<Animator>().Play("TurnTextTween");
            yield return new WaitForSeconds(0.75f);
            ExitUI.GetComponent<CutAnimation>().AnimationStarter();
            while (true) yield return null;
        } else if (noEnemyTeamsLeft)
        {
            canvas.transform.Find("Arrow").gameObject.SetActive(false);
            canvas.transform.Find("TurnText").GetComponent<TextMeshProUGUI>().text = "You Won!";
            canvas.transform.Find("TurnText").GetComponent<Animator>().Play("TurnTextTween");
            yield return new WaitForSeconds(0.75f);
            ExitUI.GetComponent<CutAnimation>().AnimationStarter();
            OutOfBattleGUI.SetActive(true);
            while (true) yield return null;
        }

        if (playerEvent != null)
        {
            playerEvent.SetActive(true);
        }

        if (alliance.Forces[curTeam].getMember(curChar).role != null)
        {
            Vector3 posInit;
            Vector3 posFinal;
            Quaternion rotInit;
            Quaternion rotFinal;

            Vector3 playerPos = maze.roomPos[alliance.Forces[curTeam].getPosition().x, alliance.Forces[curTeam].getPosition().y];
            if(playerRoom != null) { playerRoom.SetActive(true); playerRoom.transform.position = playerPos; }
            if (playerInterior != null) { playerInterior.SetActive(true); playerInterior.transform.position = playerPos; }
            if (playerEvent != null) { playerEvent.SetActive(true); playerEvent.transform.position = playerPos; }

            Animator animator;

            if (!team.ai)
            {
                switchChar(team, curChar, 0);
                foreach (Member character in team.getTeam())
                {
                    if (!character.getIndex().Equals(curChar) && (character.role != null))
                    {
                        character.role.SetActive(false);
                        Transform weapon = character.role.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).Find("Weapon");
                        GameObject hip = character.role.transform.GetChild(0).transform.GetChild(0).GetChild(0).gameObject;
                        if (character.GetClass() == Class.Archer)
                        {
                            if (weapon != null)
                            {
                                weapon.parent = hip.transform.GetChild(0);
                                weapon.localPosition = new Vector3(0.58f, 0.185f, -0.58f);
                                weapon.localRotation = Quaternion.Euler(0, 0, 75);
                            }
                        } else
                        {
                            if (weapon != null)
                            {
                                weapon.parent = hip.transform;
                                weapon.localPosition = new Vector3(-1f, -0.86f, 0.22f);
                                weapon.localRotation = Quaternion.Euler(90, 85, -90);
                            }
                        }
                    }
                }
                maze.roomOccupied[x, y] = Faction.Ally;
                updateCharPic(curTeam);
            }
            else
            {

                if (team.ai && skipAIBattles)
                {
                    yield return new WaitForSeconds(0.5f);
                }

                foreach (Member character in team.getTeam())
                {
                    if (character.role != null)
                    {
                        character.role.SetActive(false);
                    }
                }
                enemyAlliance.Forces[eCurTeam].bloodlust = 0;
                if (enemyAlliance.getStructOwned(0))
                {
                    enemyAlliance.Forces[eCurTeam].setObjective(Objective.GoToBase);
                    enemyAlliance.Forces[eCurTeam].setDirection(enemyBasePosition);
                } else if(!enemyAlliance.getLocations()[0].Equals(new Position()))
                {
                    enemyAlliance.Forces[eCurTeam].setObjective(Objective.GetGold);
                    enemyAlliance.Forces[eCurTeam].setDirection(enemyAlliance.getLocations()[0]);
                }

                enemyAlliance.getLocations();
                maze.roomOccupied[x, y] = Faction.Enemy;
            }

            if (!raiding)
            {
                prevx = x;
                prevy = y;
            }

            playerPos = alliance.Forces[curTeam].getMember(curChar).role.transform.position;
            alliance.Forces[curTeam].getMember(curChar).role.transform.position = new Vector3(playerPos.x, 0.5f, playerPos.z);

            GameObject head = alliance.Forces[curTeam].getMember(curChar).role.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).gameObject;
            posInit = Camera.main.transform.position;
            posFinal = head.transform.position + head.transform.forward * 6 + head.transform.right * 0.5f;
            rotInit = Camera.main.transform.rotation;
            rotFinal = head.transform.rotation * Quaternion.Euler(0, 180, 0);

            yield return StartCoroutine(MoveCam(Camera.main, 1.0f, posInit, posFinal, rotInit, rotFinal));

            animator = alliance.Forces[curTeam].getMember(curChar).role.transform.GetChild(0).GetComponent<Animator>();
            alliance.Forces[curTeam].getMember(curChar).role.SetActive(true);
            animator.SetBool("InBattle", false);
            Camera.main.transform.parent = alliance.Forces[curTeam].getMember(curChar).role.transform.Find("Head");
        }
        else
        {
            prevx = -1;
            prevy = -1;
        }
        if (!noTeamsLeft)
        {
            if (maze.roomName[x, y] == RoomType.Enemy && team.numberMems != 0)
            {
                NPCdefeat.Add(new Position(x, y));
                maze.roomName[x, y] = RoomType.Nothing;
            }
            OutOfBattleGUI.SetActive(true);
            inBattle = false;
            changeArrow(curChar, true);
        }
        yield return null;
    }

    //
    // GETS THE MOVE COORESPONDING THE BUTTON PRESSED

    public void attackButton(Button button)
    {
        AttackMove move = alliance.Forces[curTeam].getMember(curChar).getAttackMove(int.Parse(button.name));
        if (move != null)
        {
            if (alliance.Forces[curTeam].getMember(curChar).healCooldown && !move.isOffensive() && (move.getSpecial() == Special.None || move.getSpecial() == Special.Clean)) return;
            if (button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text != "")
            {
                chosenMove = move;
            }
        }
    }

    //
    // SMOOTHLY TRANSISTIONS THE CAMERA FROM ONE POINT TO ANOTHER

    IEnumerator MoveCam(Camera cam, float camSpeed, Vector3 posInit, Vector3 posFinal, Quaternion rotInit, Quaternion rotFinal)
    {
        float camTime = 0.0f;
        bool colour = false;
        if (cam.tag == "CutSceneCam") colour = true;
        GameObject mover = cam.gameObject;
        if(cam.transform.parent != null) { mover = cam.transform.parent.gameObject; }
        cam.transform.position = posInit;
        cam.transform.rotation = rotInit;
        while (camTime < 1.0f)
        {
            if (colour) { Color color = cam.backgroundColor; color.a += camTime; cam.backgroundColor = color; }
            mover.transform.position = Vector3.Slerp(posInit, posFinal, camTime);
            mover.transform.rotation = Quaternion.Slerp(rotInit, rotFinal, camTime);
            camTime += Time.deltaTime * camSpeed;
            yield return null;
        }
        mover.transform.position = posFinal;
        mover.transform.rotation = rotFinal;
    }

    //
    // DECIDES WHERE EACH BASE WILL BE, AND SETS THE TYPE OF EVENT FOR EACH ROOM

    public void createBoard()
    {
        maze = new mazeArray(boardSize);
        int firstv = 0;
        int secondv = 0;
        int numShops = boardSize / 2;
        int numEnem = enemies;
        int numElite = boardSize / 5;
        int numForest = boardSize / 2;
        int numMine = boardSize / 2;
        bool horizontal = (Random.value > 0.5f);

        //////////////////
        // PLAYER BASE //
        ////////////////

        if (horizontal == true)                                         //IF HORIZONTAL MAP
        {
            firstv = (((Random.value > 0.5f) ? 1 : 0) * (boardSize - 1));     //WHICH SIDE ALONG X-AXIS
            secondv = ((int)Random.Range(0, boardSize));                      //WHICH POSITION ON THE SIDE
            maze.roomName[firstv, secondv] = RoomType.PlayerBase;                       //MARK POSITION AS PLAYER BASE
        }
        else                                                            //IF NOT HORIZONTAL MAP
        {
            firstv = ((int)Random.Range(0, boardSize));                       //WHICH POSITION ON THE SIDE
            secondv = (((Random.value > 0.5f) ? 1 : 0) * (boardSize - 1));    //WHICH SIDE ALONG Y-AXIS
            maze.roomName[firstv, secondv] = RoomType.PlayerBase;                       //MARK POSTION AS PLAYER BASE
        }
        playerBasePosition = new Position(firstv, secondv);

        /////////////////
        // ENEMY BASE //
        ///////////////

        if (horizontal == true)
        {
            firstv = (firstv == (boardSize - 1)) ? 0 : (boardSize - 1);                       //MAKE SIDE OPPOSITE TO PLAYER BASE
            secondv = ((int)Random.Range(0, boardSize));
            maze.roomName[firstv, secondv] = RoomType.EnemyBase;
        }
        else
        {
            firstv = ((int)Random.Range(0, boardSize));
            secondv = (secondv == (boardSize - 1)) ? 0 : (boardSize - 1);
            maze.roomName[firstv, secondv] = RoomType.EnemyBase;
        }

        enemyBasePosition = new Position(firstv, secondv);

        ////////////////////
        // PLACING SHOPS //
        //////////////////

        while (numShops > 0)
        {
            firstv = ((int)Random.Range(1, boardSize - 1));                 //X COORDINATE, NOT ON EDGE
            secondv = ((int)Random.Range(1, boardSize - 1));                //Y COORDINATE, NOT ON EDGE
            while (maze.roomName[firstv, secondv] != 0)          //IF ROOM ALREADY TAKEN, ROLL AGAIN
            {
                firstv = ((int)Random.Range(1, boardSize - 1));
                secondv = ((int)Random.Range(1, boardSize - 1));
            }
            maze.roomName[firstv, secondv] = RoomType.Shop;                //MARK POSITION AS SHOP
            numShops--;                                             //REDUCE NUMBER OF SHOPS BEING PLACED
        }

        /////////////////////////////
        // PLACING NORMAL ENEMIES //
        ///////////////////////////

        while (numEnem > 0)
        {
            firstv = ((int)Random.Range(1, boardSize - 1));
            secondv = ((int)Random.Range(1, boardSize - 1));
            while (maze.roomName[firstv, secondv] != 0)
            {
                firstv = ((int)Random.Range(1, boardSize - 1));
                secondv = ((int)Random.Range(1, boardSize - 1));
            }
            maze.roomName[firstv, secondv] = RoomType.Enemy;              //MARK POSITION AS NORMAL ENEMY SPAWN
            numEnem--;                                             //REDUCE NUMBER OF ENEMIES BEING PLACED
        }

        while (numForest > 0)
        {
            firstv = ((int)Random.Range(1, boardSize));
            secondv = ((int)Random.Range(1, boardSize));
            while (maze.roomName[firstv, secondv] != 0)
            {
                firstv = ((int)Random.Range(1, boardSize));
                secondv = ((int)Random.Range(1, boardSize));
            }
            maze.roomName[firstv, secondv] = RoomType.Forest;
            numForest--;
        }

        while (numMine > 0)
        {
            firstv = ((int)Random.Range(1, boardSize));
            secondv = ((int)Random.Range(1, boardSize));
            while (maze.roomName[firstv, secondv] != 0)
            {
                firstv = ((int)Random.Range(1, boardSize));
                secondv = ((int)Random.Range(1, boardSize));
            }
            maze.roomName[firstv, secondv] = RoomType.Mine;
            numMine--;
        }
    }

    //
    // PLACES THE GAMEOBJECTS NEEDED FOR EACH ROOM, SPECIFIED BY THE CREATE BOARD FUNCTION

    public void placeRooms()
    {
        int bX = 0;
        int bY = 0;
        int roomSize = 40;
        int fireflyRooms = 0;

        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                #region SET ROOM LAYOUT
                maze.roomPos[i, j] = new Vector3(bX, 0, bY);
                if (i == 0 || i == boardSize - 1)
                {
                    if (j == 0 || j == boardSize - 1)
                    {
                        if ((maze.roomName[i, j] == RoomType.EnemyBase) || (maze.roomName[i, j] == RoomType.PlayerBase))
                        {
                            //**    CORNER BASE    **//
                            maze.room[i, j] = "Map2";
                            maze.roomRot[i, j] = Quaternion.Euler(0, 90 - 180 * (i / (boardSize - 1)), 0);

                            string baseName = "Maps/Base";
                            if (new Position(i, j).Equals(enemyBasePosition))
                            {
                                baseName = "Maps/EnemyBase";
                            }
                            GameObject playerBase = Instantiate(Resources.Load(baseName) as GameObject);
                            playerBase.transform.rotation = Quaternion.Euler(0, 180 * (j / (boardSize - 1)), 0);
                            int diff = Mathf.Abs(j - i) / (boardSize - 1);
                            playerBase.transform.localPosition = maze.roomPos[i,j] + new Vector3(0, 0, -60 + 120 * (j / (boardSize - 1)));
                        }
                        else
                        {
                            //**    NORMAL CORNER   **//
                            maze.room[i, j] = "Corner";
                            maze.roomRot[i, j] = Quaternion.Euler(0, 270 * (i / (boardSize - 1)) + 90 * (j / (boardSize - 1)) + 180 * ((i + j) / ((boardSize - 1) * 2)), 0);
                        }
                    }
                    else
                    {
                        if ((maze.roomName[i, j] == RoomType.EnemyBase) || (maze.roomName[i, j] == RoomType.PlayerBase))
                        {
                            //**    BASE ON FB SIDE    *//
                            maze.room[i, j] = "Map3";
                            maze.roomRot[i, j] = Quaternion.Euler(0, 180 * (i / (boardSize - 1)), 0);

                            string baseName = "Maps/Base";
                            if(new Position(i, j).Equals(enemyBasePosition))
                            {
                                baseName = "Maps/EnemyBase";
                            }
                            GameObject playerBase = Instantiate(Resources.Load(baseName) as GameObject);
                            playerBase.transform.rotation = Quaternion.Euler(0, 90 - 180 * (i / (boardSize - 1)), 0);
                            playerBase.transform.localPosition = maze.roomPos[i, j] + new Vector3(-60 + 120 * (i / (boardSize - 1)), 0, 0);
                        }
                        else
                        {
                            //**    NORMAL WALL ON FB SIDE    **//
                            maze.room[i, j] = "Wall";
                            maze.roomRot[i, j] = Quaternion.Euler(0, 180 * (i / (boardSize - 1)), 0);
                        }
                    }
                }
                else
                {
                    if (j == 0 || j == boardSize - 1)
                    {
                        if ((maze.roomName[i, j] == RoomType.EnemyBase) || (maze.roomName[i, j] == RoomType.PlayerBase))
                        {
                            //**    WALL BASE ON LR SIDE    **//
                            maze.room[i, j] = "Map3";
                            maze.roomRot[i, j] = Quaternion.identity;

                            string baseName = "Maps/Base";
                            if (new Position(i, j).Equals(enemyBasePosition))
                            {
                                baseName = "Maps/EnemyBase";
                            }
                            GameObject playerBase = Instantiate(Resources.Load(baseName) as GameObject);
                            playerBase.transform.rotation = Quaternion.Euler(0, 180 * (j / (boardSize - 1)), 0);
                            playerBase.transform.localPosition = maze.roomPos[i, j] + new Vector3(0, 0, -60 + 120 * (j / (boardSize - 1)));
                        }
                        else
                        {
                            //**    WALL ROOM ON LR SIDE    **//
                            maze.room[i, j] = "Wall";
                            maze.roomRot[i, j] = Quaternion.Euler(0, 270 + 180 * (j / (boardSize - 1)), 0);
                        }
                    }
                    else
                    {
                        //**    NORMAL INNER ROOM    **//
                        maze.room[i, j] = "Map1";
                        maze.roomRot[i, j] = Quaternion.identity;
                    }
                }
                #endregion

                #region SET ROOM INTERIOR
                if (maze.roomName[i,j] != RoomType.Forest && maze.roomName[i, j] != RoomType.Mine)
                {
                    if (j > 0 && j < (boardSize - 1) && i > 0 && i < (boardSize - 1))
                    {
                        float random = Random.value;
                        if (random > 0.85f)
                        {
                            maze.interior[i, j] = "Inside2";
                        }
                        else if (random > 0.6f)
                        {
                            maze.interior[i, j] = "Inside1";
                        }
                        if (random > 0.825f)
                        {
                            fireflyRooms++;
                            maze.fireFlies[i, j] = true;
                        }
                    }
                    else
                    {
                        float random = Random.value;
                        if (random > 0.75f)
                        {
                            maze.interior[i, j] = "Nature2";
                        }
                        else if (random > 0.45f)
                        {
                            maze.interior[i, j] = "Nature1";
                        }
                    }
                }
                #endregion

                #region SET ROOM EVENT
                switch (maze.roomName[i, j])
                {
                    case RoomType.Nothing:
                        break;
                    case RoomType.Enemy:
                        break;
                    case RoomType.Shop:
                        int shopType = Random.Range(2, 5);
                        maze.setEvent(i, j, "" + (ShopScript.Shop)shopType);
                        break;
                    case RoomType.Forest:
                        maze.setEvent(i, j, "Forest");
                        break;
                    case RoomType.Mine:
                        maze.setEvent(i, j, "Mine");
                        break;
                }
                #endregion
                bY += roomSize;
            }
            bY = 0;
            bX += roomSize;
        }
        Debug.Log("Firefly rooms: " + fireflyRooms);
    }

    //
    // LOADS ALL NECESSARY ROOMS AND ROOM COMPONENTS WHEN NEEDED

    public void setRooms(int x, int y, bool playerTurn)
    {
        if ((x > (maze.getLength() - 1)) || (y > (maze.getLength() - 1))) return;

        GameObject curInterior = null;
        GameObject curRoom = SpawnFromPool(maze.room[x, y], maze.roomPos[x, y], maze.roomRot[x, y]);
        GameObject curEvent = null;

        if (maze.getEvent(x, y) != "" && maze.getEvent(x, y) != null) { curEvent = SpawnFromPool(maze.getEvent(x, y), maze.roomPos[x, y], maze.roomRot[x, y]); }
        if(maze.interior[x,y] != null) { curInterior = SpawnFromPool(maze.interior[x, y], maze.roomPos[x, y], maze.roomRot[x, y]); }
        if (maze.fireFlies[x, y]) { SpawnFromPool("Fireflies", maze.roomPos[x, y], maze.roomRot[x, y]); }

        if (playerTurn)
        {
            if (OutOfBattleGUI.transform.Find("Shop").gameObject.activeSelf)
            {
                OutOfBattleGUI.transform.Find("Shop").GetComponent<ShopScript>().closeShop();
            }

            if (Map != null)
            {
                Debug.Log("Setting Map position: " + x + ", " + y);
                Map.GetComponent<MapSetup>().ShowPosition(new Position(x, y));
            }
            if (alliance.Forces[curTeam].getPosition().Equals(new Position(-1,-1)))
            {
                if(alliance.Forces[curTeam].getMember(curChar).role != null)
                {
                    alliance.Forces[curTeam].getMember(curChar).role.transform.position = maze.roomPos[x, y] + Vector3.up * 0.5f + Vector3.forward * 5;
                }
            }
            else
            {
                if (alliance.Forces[curTeam].getMember(curChar).role != null)
                {
                    alliance.Forces[curTeam].getMember(curChar).role.transform.position = maze.roomPos[x, y] + Vector3.up * 0.5f + new Vector3(prevx - x, 0, prevy - y) * 12;
                }
            }
            if(playerRoom != null && !ReferenceEquals(playerRoom, curRoom)) { playerRoom.SetActive(false); }
            if (playerEvent != null && !ReferenceEquals(playerEvent, curEvent)) { playerEvent.SetActive(false); }
            if (playerInterior != null && !ReferenceEquals(playerInterior, curInterior)) { playerInterior.SetActive(false); }
            playerRoom = curRoom;
            playerEvent = curEvent;
            playerInterior = curInterior;
        }
    }

    //
    // HANDLES ALL MOVEMENT OPERATIONS, LOADING ROOMS ON MOVING AND STARTING BATTLES WHEN NEEDED

    public void moveRooms(int x, int y)
    {
        Vector3 posFinal;
        Quaternion rotFinal;

        #region PLAYER ACTIONS
        if (playerTurn)
        {
            if (!alliance.Forces[curTeam].getPosition().Equals(new Position(-1, -1)))
            {
                prevx = alliance.Forces[curTeam].getPosition().x;
                prevy = alliance.Forces[curTeam].getPosition().y;
            }

            if (prevx == -1)
            {
                if (alliance.Forces[curTeam].getPosition().x == -1)
                {
                    prevx = x;
                    prevy = y;
                }
                if (maze.roomOccupied[prevx, prevy] == Faction.Ally)
                {
                    maze.roomOccupied[prevx, prevy] = Faction.Neutral;
                }
            }
            else
            {
                maze.roomOccupied[prevx, prevy] = Faction.Neutral;
                for (int i = 0; i < alliance.Forces.Length; i++)
                {
                    if (alliance.Forces[i].getPosition().Equals(new Position(prevx, prevy)))
                    {
                        if (i != curTeam)
                        {
                            maze.roomOccupied[prevx, prevy] = Faction.Ally;
                        }
                    }
                }
            }

            //alliance.Forces[curTeam].setPosition(x, y);
            setRooms(x, y, true);
            alliance.Forces[curTeam].setPosition(x, y);
            Debug.Log("We did it! x is: " + x + ", and y is: " + y);

            if (maze.roomOccupied[x, y] == Faction.Neutral)
            {
                maze.roomOccupied[x, y] = Faction.Ally;
            }

            Debug.Log("Room " + x + ", " + y + ": " + maze.roomOccupied[x, y]);
            string output = string.Format("Update! \nCurrentRoom is [{0}, {1}]", x, y);
            Debug.Log(output);
            if(new Position(x, y).Equals(enemyBasePosition))
            {
                Vector3 mazeCur = maze.roomPos[x, y];
                Camera.main.transform.parent = null;
                posFinal = new Vector3(mazeCur.x - (y - prevy) * 17, mazeCur.y + 15, mazeCur.z - (prevx - x) * 17);
                rotFinal = Quaternion.Euler(40, (-(prevy - y) * 90) + (((x - prevx + 1) / 2) * 180), 0);
                inBattle = true;

                eCurTeam = 0;
                StartCoroutine(StartRaid(alliance, enemyAlliance, x, y, posFinal, rotFinal));
            }else if (maze.roomOccupied[x, y] == Faction.Enemy)
            {
                Debug.Log("There's an enemy here!");
                bool fought = false;
                for (int i = 0; i < enemyAlliance.Forces.Length; i++)
                {
                    if (enemyAlliance.Forces[i].getPosition().x == x && enemyAlliance.Forces[i].getPosition().y == y)
                    {
                        Vector3 mazeCur = maze.roomPos[x, y];
                        Camera.main.transform.parent = null;
                        posFinal = new Vector3(mazeCur.x - (y - prevy) * 17, mazeCur.y + 15, mazeCur.z - (prevx - x) * 17);
                        rotFinal = Quaternion.Euler(40, (-(prevy - y) * 90) + (((x - prevx + 1) / 2) * 180), 0);
                        inBattle = true;
                        fought = true;

                        eCurTeam = i;
                        StartCoroutine(battleSystem(alliance.Forces[curTeam], enemyAlliance.Forces[eCurTeam], x, y, posFinal, rotFinal));
                        break;
                    }
                }
                if (fought == false)
                {
                    maze.roomOccupied[x, y] = Faction.Ally;
                }
            }
            else if (maze.roomName[x, y] == RoomType.Enemy)
            {
                NPCTeam roomTeam = new NPCTeam(0.65f);

                switchChar(alliance.Forces[curTeam], curChar, 0);

                int randLoadOut = Random.Range(0, 3);
                roomTeam.GetLoadOut((TeamLoadOut)randLoadOut);

                Vector3 mazeCur = maze.roomPos[x, y];
                Camera.main.transform.parent = null;
                posFinal = new Vector3(mazeCur.x - (y - prevy) * 17, mazeCur.y + 15, mazeCur.z - (prevx - x) * 17);
                rotFinal = Quaternion.Euler(40, (-(prevy - y) * 90) + (((x - prevx + 1) / 2) * 180), 0);
                inBattle = true;

                StartCoroutine(battleSystem(alliance.Forces[curTeam], roomTeam, x, y, posFinal, rotFinal));
            }


            if (!inBattle)
            {
                prevx = x;
                prevy = y;
            }
        }
        #endregion

        #region ENEMY ACTIONS
        else
        {

            //////////////////////////////////
            // BEGINNING OF AI ROOM MOVING //
            ////////////////////////////////

            if (enemyAlliance.Forces[eCurTeam].numberMems > 0)
            {
                if (prevx == -1)
                {
                    if (enemyAlliance.Forces[eCurTeam].getPosition().x == -1)
                    {
                        prevx = x;
                        prevy = y;
                    }
                    else
                    {
                        prevx = enemyAlliance.Forces[eCurTeam].getPosition().x;
                        prevy = enemyAlliance.Forces[eCurTeam].getPosition().y;
                    }
                    maze.roomOccupied[prevx, prevy] = Faction.Neutral;
                }
                else
                {
                    maze.roomOccupied[prevx, prevy] = Faction.Neutral;
                    for (int i = 0; i < enemyAlliance.Forces.Length; i++)
                    {
                        if ((enemyAlliance.Forces[i].getPosition().x == prevx) && (enemyAlliance.Forces[i].getPosition().y == prevy))
                        {
                            if (i != eCurTeam)
                            {
                                maze.roomOccupied[prevx, prevy] = Faction.Enemy;
                            }
                        }
                    }
                }

                enemyAlliance.Forces[eCurTeam].setPosition(x, y);

                if (maze.roomOccupied[x, y] == Faction.Neutral)
                {
                    maze.roomOccupied[x, y] = Faction.Enemy;
                }

                if (new Position(x, y).Equals(playerBasePosition))
                {
                    Vector3 mazeCur = maze.roomPos[x, y];
                    Camera.main.transform.parent = null;
                    posFinal = new Vector3(mazeCur.x - (y - prevy) * 17, mazeCur.y + 15, mazeCur.z - (prevx - x) * 17);
                    rotFinal = Quaternion.Euler(40, (-(prevy - y) * 90) + (((x - prevx + 1) / 2) * 180), 0);
                    inBattle = true;

                    eCurTeam = 0;
                    StartCoroutine(StartRaid(alliance, enemyAlliance, x, y, posFinal, rotFinal));
                }else if (maze.roomOccupied[x, y] == Faction.Ally)
                {
                    bool fought = false;
                    for (int i = 0; i < alliance.Forces.Length; i++)
                    {
                        if (alliance.Forces[i].getPosition().x == x && alliance.Forces[i].getPosition().y == y)
                        {
                            Vector3 mazeCur = maze.roomPos[x, y];
                            Camera.main.transform.parent = null;
                            posFinal = new Vector3(mazeCur.x - (y - prevy) * 17, mazeCur.y + 15, mazeCur.z - (prevx - x) * 17);
                            rotFinal = Quaternion.Euler(40, (-(prevy - y) * 90) + (((x - prevx + 1) / 2) * 180), 0);
                            inBattle = true;
                            fought = true;

                            setRooms(x, y, false);

                            StartCoroutine(battleSystem(alliance.Forces[i], enemyAlliance.Forces[eCurTeam], x, y, posFinal, rotFinal));
                            break;
                        }
                    }
                    if (fought == false)
                    {
                        maze.roomOccupied[x, y] = Faction.Enemy;
                    }
                }
                else if (maze.roomName[x, y] == RoomType.Enemy)
                {
                    NPCTeam roomTeam = new NPCTeam(0.65f);
                    setRooms(x, y, false);

                    int randLoadOut = Random.Range(0, 3);
                    roomTeam.GetLoadOut((TeamLoadOut)randLoadOut);

                    Vector3 mazeCur = maze.roomPos[x, y];
                    Camera.main.transform.parent = null;
                    posFinal = new Vector3(mazeCur.x - (y - prevy) * 17, mazeCur.y + 15, mazeCur.z - (prevx - x) * 17);
                    rotFinal = Quaternion.Euler(40, (-(prevy - y) * 90) + (((x - prevx + 1) / 2) * 180), 0);
                    inBattle = true;

                    StartCoroutine(battleSystem(enemyAlliance.Forces[eCurTeam], roomTeam, x, y, posFinal, rotFinal));
                }

                if (!inBattle)
                {
                    prevx = x;
                    prevy = y;
                }
            }
            #endregion

        }

    }

    //
    // ALLOWS YOU TO FINISH A TURN EARLY

    public void buttonChangeTurn()
    {
        if (playerTurn)
        {
            playerTurn = false;
        }
    }

    //
    // PERFORMS BOARD MOVEMENT OPERATIONS FOR THE PLAYER DURING ITS TURN
    // AND TOTALS THE GAINED RESOURCES ONCE THE TURN IS OVER

    IEnumerator PlayerTurn()
    {
        Debug.Log("PlayerTurn!");
        turnNumber++;
        canvas.transform.Find("TurnText").GetComponent<TextMeshProUGUI>().text = "Turn " + turnNumber;

        if (turnNumber % 5 == 0)
        {
            canvas.transform.Find("TurnText").GetChild(0).gameObject.SetActive(true);
            foreach (Position pos in NPCdefeat)
            {
                maze.roomName[pos.x, pos.y] = RoomType.Enemy;
            }
            NPCdefeat.Clear();
        }

        canvas.transform.Find("TurnText").GetComponent<Animator>().Play("TurnTextTween");

        for(int i = 0; i<max_Forces; i++)
        {
            if(alliance.Forces[i].numberMems != 0)
            {
                playerMoves += MOVEAMOUNT;
            }
        }

        canvas.transform.Find("OutOfBattleGUI").Find("MovesLeft").Find("Moves").GetComponent<TextMeshProUGUI>().text = "" + playerMoves;

        while (playerTurn)
        {
            yield return null;
        }

        playerMoves = 0;

        foreach (Team team in alliance.Forces)
        {
            if (team.getPosition().x != -1)
            {
                RoomType temp = maze.roomName[team.getPosition().x, team.getPosition().y];
                if (temp == RoomType.Mine)
                {
                    alliance.Iron += 5;
                }
                else if (temp == RoomType.Forest)
                {
                    alliance.Wood += 10;
                }
            }
        }
        if (alliance.getStructOwned(0))
        {
            alliance.Iron += 8;
        }
        if (alliance.getStructOwned(1))
        {
            alliance.Wood += 15;
        }
        if (alliance.getStructOwned(2))
        {
            alliance.Cash += 30;
        }
        Transform Canvas = OutOfBattleGUI.transform.Find("CharPics").transform.Find("TeamStats");
        Canvas.Find("Iron").Find("Value").GetComponent<TextMeshProUGUI>().text = "" + alliance.Iron;
        Canvas.Find("Wood").Find("Value").GetComponent<TextMeshProUGUI>().text = "" + alliance.Wood;
        Canvas.Find("Cash").Find("Value").GetComponent<TextMeshProUGUI>().text = "" + alliance.Cash;
        prevx = -1;
        prevy = -1;
        yield return EnemyTurn();
    }

    //
    // IF THE AI DOESN'T ALREADY HAVE A DIRECTION, GIVE IT A RANDOM DIRECTION

    IEnumerator AIGetDestination()
    {
        if (enemyAlliance.Forces[eCurTeam].getObjective() == Objective.Nothing || enemyAlliance.Forces[eCurTeam].getObjective() == Objective.Bloodlust)
        {
            Debug.Log("Team " + eCurTeam + " getting random direction");
            int distance = MOVEAMOUNT;
            int tempx = enemyAlliance.Forces[eCurTeam].getPosition().x + (int)Random.Range(-distance, distance);
            tempx = Mathf.Clamp(tempx, 0, boardSize - 1);
            distance -= tempx;
            int tempy = enemyAlliance.Forces[eCurTeam].getPosition().y + (int)Random.Range(-distance, distance);
            tempy = Mathf.Clamp(tempy, 0, boardSize - 1);
            Debug.Log("New direction = " + tempx + ", " + tempy);
            enemyAlliance.Forces[eCurTeam].setDirection(new Position(tempx, tempy));
        }
        yield return null;
    }

    //
    // GIVEN A DIRECTION, THE FUNCTION TELLS THE AI WHICH MAZE BLOCK TO MOVE TO

    void AIMoveDecision()
    {
        int xPos = enemyAlliance.Forces[eCurTeam].getPosition().x;
        int yPos = enemyAlliance.Forces[eCurTeam].getPosition().y;
        int xDir = enemyAlliance.Forces[eCurTeam].getDirection().x - xPos;
        int yDir = enemyAlliance.Forces[eCurTeam].getDirection().y - yPos;
        int finalDir = 0;
        if (Mathf.Abs(xDir) > Mathf.Abs(yDir))
        {
            if (xDir != 0)
            {
                finalDir = (int)Mathf.Sign(xDir);
            }
            if (new Position(finalDir + xPos, yPos).Equals(playerBasePosition) && !enemyAlliance.raidReady)
            {
                if(yPos >= boardSize-1)
                {
                    moveRooms(xPos, yPos - 1);
                } else
                {
                    moveRooms(xPos, yPos + 1);
                }
            } else
            {
                moveRooms(finalDir + xPos, yPos);
            }
        }
        else
        {
            if (yDir != 0)
            {
                finalDir = (int)Mathf.Sign(yDir);
            }
            if (new Position(xPos, finalDir + yPos).Equals(playerBasePosition) && !enemyAlliance.raidReady)
            {
                if (xPos >= boardSize - 1)
                {
                    moveRooms(xPos - 1, yPos);
                }
                else
                {
                    moveRooms(xPos + 1, yPos);
                }
            } else
            {
                moveRooms(xPos, finalDir + yPos);
            }
        }
    }

    //
    // THE FUNCTION THAT DETERMINES WHAT ATTACK WILL BE USED BY AI DURING COMBAT

    AttackMove AIAttackDecision(Member member, EnemyTeam teamOf, Team teamAgainst, bool firstPics)
    {
        #region Variables
        int x = (int)((member.role.transform.position.x + 20) / 40f);
        int y = (int)((member.role.transform.position.z + 20) / 40f);

        int totalHealth = 0;
        int totalMaxHealth = 0;
        int weakestEnemyUnit = -1;

        //If two positions have the same damage output, this is the threshold the random number needs to get over to pick the second choice
        float choosePositionThreshold = 0.5f;
        //Whether or not the member can swap
        bool memberCanMove = member.getSwapSafety() != 0 ? false : true;
        //Chance team will push or buff instead of healing or attacking. Threshold is the amount you need to get over for this to occur.
        float strategicPercent = 0.0f;
        float strategicThreshold = 0.6f;
        //Chance the team will do something wrong. Theshold is amount you need to get under for this to occur.
        float intelligence = Random.value * teamOf.getIntelligence();
        float lowIntelligenceThreshold = 0.2f;

        AttackMove strongestAttack = new AttackMove();
        AttackMove pull = null;
        int pullPos = 0;
        AttackMove aBuff = null;
        AttackMove dBuff = null;
        int strongestAttackDamage = 0;
        int strongestAttackPos = -1;
        AttackMove strongestHeal = new AttackMove();
        int strongestHealAmount = 0;
        int strongestHealPos = -1;

        bool offensive = true;
        #endregion

        #region Enemy Team Stats

        for (int i = 0; i < max_Team; i++)
        {
            // YOUR TEAM'S STATS //
            if (teamOf.getMember(i).role != null)
            {
                totalHealth += teamOf.getMember(i).getHealth();
                totalMaxHealth += teamOf.getMember(i).getHealthCap();
            }
            // ENEMY TEAM'S STATS //
            if (teamAgainst.getMember(i).role != null)
            {
                if (weakestEnemyUnit == -1)
                {
                    weakestEnemyUnit = i;
                    continue;
                }
                if (
                    ((float)teamAgainst.getMember(i).getHealth() / teamAgainst.getMember(i).getHealthCap()) <
                    ((float)teamAgainst.getMember(weakestEnemyUnit).getHealth() / teamAgainst.getMember(weakestEnemyUnit).getHealthCap())
                   )
                {
                    weakestEnemyUnit = i;
                }
            }
        }
        #endregion

        #region Find Strongest Moves

        for (int i = 1; i < 5; i++)
        {
            if (member.getAttackMove(i).getName() == "") continue;

            AttackMove currentMove = member.getAttackMove(i);
            //IF THE CURRENT MOVE IS A SPECIAL MOVE (BUFF OR DEBUFF)
            if (currentMove.getSpecial() != Special.None)
            {
                switch (currentMove.getSpecial())
                {
                    case Special.Pull:
                        for (int posAdjust = 0; posAdjust < 2; posAdjust++)
                        {
                            int adjustAmount = currentMove.getPosition() - posAdjust;
                            for (int index = 0; index < max_Team; index++)
                            {
                                int attackIndex = adjustAmount + index;
                                if (attackIndex < 0) { continue; }
                                if (attackIndex > 3) { continue; }
                                if ((currentMove.getSpaces()[index] == true) && (teamAgainst.getMember(attackIndex).role != null) && (teamAgainst.getMember(attackIndex).getSwapSafety() == 0))
                                {
                                    pull = currentMove;
                                    pullPos = posAdjust;
                                    posAdjust = 2;
                                    break;
                                }
                            }
                        }
                        continue;
                    case Special.AttackBuff:
                        aBuff = currentMove;
                        continue;
                    case Special.DefenseBuff:
                        dBuff = currentMove;
                        continue;
                }
            }
            //ELSE IF THE CURRENT MOVE IS AN ATTACKING MOVE
            if (currentMove.isOffensive())
            {
                int poisonDamage = 0;
                if (currentMove.getSpecial() == Special.Poison)
                {
                    poisonDamage = currentMove.getSpecialPower();
                }
                //IF THE CURRENT MOVE'S POWER CAN KILL THE OPPONENT, SEE IF A SPACE CAN REACH IT
                if ((currentMove.getPower() + poisonDamage) >= teamAgainst.getMember(weakestEnemyUnit).getHealth() && memberCanMove)
                {
                    for (int j = 0; j < max_Team; j++)
                    {
                        int space = weakestEnemyUnit + (j - currentMove.getPosition());
                        if ((space >= 0) && (space < max_Team) && (currentMove.getSpaces()[space] == true))
                        {
                            if (member.getIndex() != j)
                            {
                                swapTeam(teamOf, member.getIndex(), j, x, y, firstPics);
                            }
                            return currentMove;
                        }
                    }
                }
                else if (!memberCanMove)
                {
                    int currentDamage = 0;
                    int posAdjust = currentMove.getPosition() - member.getIndex();
                    for (int index = 0; index < max_Team; index++)
                    {
                        int attackIndex = index + posAdjust;
                        if (attackIndex < 0) { continue; }
                        if (attackIndex > 3) { continue; }
                        if ((currentMove.getSpaces()[index] == true) && (teamAgainst.getMember(attackIndex).role != null))
                        {
                            currentDamage += currentMove.getPower() + poisonDamage;
                        }
                    }
                    if (currentDamage > strongestAttackDamage)
                    {
                        strongestAttack = currentMove;
                        strongestAttackDamage = currentDamage;
                        strongestAttackPos = member.getIndex();
                    }
                }
                //IF NOT, SEE IF THE CURRENT MOVE IS THE STRONGEST ATTACK
                else
                {
                    int currentDamage = 0;
                    int currentPos = 0;
                    for (int posAdjust = 0; posAdjust < max_Team; posAdjust++)
                    {
                        int posDamage = 0;
                        int adjustAmount = currentMove.getPosition() - posAdjust;
                        for (int index = 0; index < max_Team; index++)
                        {
                            int attackIndex = adjustAmount + index;
                            if (attackIndex < 0) { continue; }
                            if (attackIndex > 3) { continue; }
                            if ((currentMove.getSpaces()[index] == true) && (teamAgainst.getMember(attackIndex).role != null))
                            {
                                posDamage += currentMove.getPower() + poisonDamage;
                            }
                        }
                        if (posDamage > currentDamage) { currentDamage = posDamage; currentPos = posAdjust; }
                        else if (posDamage == currentDamage && memberCanMove && Random.value > choosePositionThreshold)
                        {
                            Debug.Log("Alright, we'll compromise");
                            currentDamage = posDamage; currentPos = posAdjust;
                        }
                    }
                    if (currentDamage > strongestAttackDamage)
                    {
                        strongestAttack = currentMove;
                        strongestAttackDamage = currentDamage;
                        strongestAttackPos = currentPos;
                    }
                }
            }
            //ELSE THE CURRENT MOVE IS A HEALING MOVE, SO SEE IF IT'S THE STRONGEST
            else
            {
                int currentPos = 0;
                int currentHeal = 0;
                Debug.Log("Current defensive move is: " + currentMove.getName());
                for (int posAdjust = 0; posAdjust < max_Team; posAdjust++)
                {
                    int posHeal = 0;
                    int adjustAmount = posAdjust - currentMove.getPosition();
                    for (int index = 0; index < max_Team; index++)
                    {
                        int attackIndex = adjustAmount + index;
                        if (attackIndex < 0) { continue; }
                        if (attackIndex > 3) { continue; }
                        if ((currentMove.getSpaces()[index] == true) && (teamOf.getMember(attackIndex).role != null))
                        {
                            posHeal += currentMove.getPower();
                        }
                    }
                    if (posHeal > currentHeal) { currentHeal = posHeal; currentPos = posAdjust; }
                }
                if (currentHeal > strongestHealAmount)
                {
                    strongestHeal = currentMove;
                    strongestHealAmount = currentHeal;
                    strongestHealPos = currentPos;
                }
            }
        }
        #endregion

        #region Attack Decision

        if ( ((float)member.getHealth()/member.getHealthCap() < 0.35f) || (((float)(totalHealth-member.getPoisonCounter()) / totalMaxHealth) < 0.35f) )
        {
            offensive = false;
        }

        if(pull != null || aBuff != null || dBuff != null) { strategicPercent = Random.value; }
        if (intelligence < lowIntelligenceThreshold) {
            if(offensive == false)
            {
                offensive = true;
            } else
            {
                offensive = false;
            }
        }

        if (offensive)
        {
            if (strategicPercent < strategicThreshold && strongestAttack.getName() != "" && (memberCanMove || member.getIndex() == strongestAttackPos) && intelligence > lowIntelligenceThreshold)
            {
                swapTeam(teamOf, member.getIndex(), strongestAttackPos, x, y, firstPics);
                return strongestAttack;
            }
            else if(pull != null && (memberCanMove || member.getIndex() == pullPos))
            {
                swapTeam(teamOf, member.getIndex(), pullPos, x, y, firstPics);
                return pull;
            }
            else if (aBuff != null)
            {
                if(memberCanMove)
                {
                    swapTeam(teamOf, member.getIndex(), aBuff.getPosition(), x, y, firstPics);
                }
                return aBuff;
            }
            else if (dBuff != null)
            {
                if (memberCanMove)
                {
                    swapTeam(teamOf, member.getIndex(), dBuff.getPosition(), x, y, firstPics);
                }
                return dBuff;
            }
            else if (strongestHeal.getName() != "" && !member.healCooldown)
            {
                if(memberCanMove)
                {
                    if(strongestHealAmount == strongestHeal.getPower())
                    {
                        swapTeam(teamOf, member.getIndex(), 3, x, y, firstPics);
                    } else
                    {
                        swapTeam(teamOf, member.getIndex(), strongestHealPos, x, y, firstPics);
                    }
                }
                return strongestHeal;
            }
        }
        else
        {
            if (strongestHeal.getName() != "" && !member.healCooldown && strategicPercent < strategicThreshold && intelligence > lowIntelligenceThreshold)
            {
                if (memberCanMove) {
                    swapTeam(teamOf, member.getIndex(), 3, x, y, firstPics);
                }
                return strongestHeal;
            }
            else if (strongestAttack.getName() != "" && (memberCanMove || member.getIndex() == strongestAttackPos))
            {
                swapTeam(teamOf, member.getIndex(), strongestAttackPos, x, y, firstPics);
                return strongestAttack;
            }
            else if (strategicPercent > strategicThreshold && pull != null && (memberCanMove || member.getIndex() == pullPos))
            {
                swapTeam(teamOf, member.getIndex(), pullPos, x, y, firstPics);
                return pull;
            }
            else if (dBuff != null)
            {
                if (memberCanMove)
                {
                    swapTeam(teamOf, member.getIndex(), 3, x, y, firstPics);
                }
                return dBuff;
            }
            else if (aBuff != null)
            {
                if (memberCanMove)
                {
                    swapTeam(teamOf, member.getIndex(), 3, x, y, firstPics);
                }
                return aBuff;
            }
        }
        return member.getAttackMove(0);
        #endregion
    }

    //
    // ALLOWS THE AI TO SELL STOCK, EITHER AT A SHOP OR THEIR BASE WHEN THEY OWN A MARKET

    void SellOperation(int buyAmount)
    {
        float woodPrice = Stats.woodPrice * 0.7f;
        float ironPrice = Stats.ironPrice * 0.7f;
        //IF ALL THE STRUCTURES AREN'T BOUGHT, THEN SET THE GOLD AMOUNT TO THE STRUCTURE PRICE

        int goldGained = 0;
        if (enemyAlliance.Iron > 0)
        {
            goldGained += (int)(enemyAlliance.Iron * ironPrice);
            enemyAlliance.Iron = 0;
        }
        //IF WE STILL DON'T HAVE ENOUGH OR WE DIDN'T HAVE ANY IRON, START SELLING WOOD
        if ((enemyAlliance.Cash < buyAmount) && (enemyAlliance.Wood > 0))
        {
            goldGained += (int)(enemyAlliance.Wood * woodPrice);
            enemyAlliance.Wood = 0;
        }
        for (int i = 0; i<max_Team; i++)
        {
            if(enemyAlliance.Forces[eCurTeam].getMember(i).role != null)
            {
                enemyAlliance.Forces[eCurTeam].getMember(i).addGold(goldGained);
                break;
            }
        }
        enemyAlliance.Forces[eCurTeam].setObjective(Objective.GoToBase);
        enemyAlliance.Forces[eCurTeam].setDirection(enemyBasePosition);
        if(goldGained > 0)
        {
            activityFeed.text += "Enemy team sold resources and gained " + goldGained + " gold\n";
        }
    }

    //
    // ADDS POINTS OF INTEREST NOT MARKED YET TO THE LIST OF AI LOCATIONS
    // AND PERFORMS OBJECTIVE OPERATIONS AT POINTS OF INTEREST, 
    // (SUCH AS SELLING STOCK WHEN CASH IS NEEDED, OR BUYING STRUCTURES AT THE AI BASE)

    IEnumerator AIObjectiveCheck()
    {
        Position newPos = new Position(enemyAlliance.Forces[eCurTeam].getPosition().x, enemyAlliance.Forces[eCurTeam].getPosition().y);
        switch (maze.roomName[newPos.x, newPos.y])
        {
            case RoomType.Shop:
                enemyAlliance.addLocation(newPos, 0);
                break;
            case RoomType.Forest:
                enemyAlliance.addLocation(newPos, 1);
                break;
            case RoomType.Mine:
                enemyAlliance.addLocation(newPos, 2);
                break;
        }

        // IF CURRENT POSITION IS ENEMY BASE AND THE OBJECTIVE IS TO GO TO THE BASE, DO BASE OPERATIONS IF NEEDED (BUY MARKET, ETC.)
        if (enemyAlliance.Forces[eCurTeam].getPosition().Equals(enemyBasePosition) && (enemyAlliance.Forces[eCurTeam].getObjective() == Objective.GoToBase))
        {
            //PUT ANY GOLD THE TEAM HAS INTO THE BASE
            int storedGold = 0;
            if (enemyAlliance.getStructOwned((int)Structure.Market))
            {
                SellOperation(95000);
            }
            foreach (Member mem in enemyAlliance.Forces[eCurTeam].getTeam())
            {
                enemyAlliance.Cash += mem.getGold();
                storedGold += mem.getGold();
                mem.setGold(0);
            }
            if(storedGold > 0)
            {
                activityFeed.text += "Enemy team stored " + storedGold + " gold in their vault\n";
            }
            //IF WE NEED TO MAKE SOLDIERS, MAKE THEM. 
            if (enemyAlliance.getMakeSoldiers())
            {
                int forcesAmount = 2;
                if (enemyAlliance.getStructOwned(0)) { forcesAmount = 3; }
                if (enemyAlliance.getStructOwned(2)) { forcesAmount = 4; }
                if (enemyAlliance.getStructOwned(3)) { forcesAmount = 5; }
                int buyAmount = 0;
                for (int i = 0; i < forcesAmount; i++)
                {
                    Debug.Log(enemyAlliance.Forces[i].numberMems);
                    Debug.Log(enemyAlliance.Cash);
                    if (enemyAlliance.Forces[i].numberMems < 2)
                    {
                        Debug.Log("Buying members for team " + i);
                        buyAmount = Class.Archer.GetPrice() + Class.Priest.GetPrice();
                        if (enemyAlliance.Cash < buyAmount && enemyAlliance.getStructOwned((int)Structure.Market))
                        {
                            Debug.Log("Need cash for team " + i);
                            SellOperation(buyAmount);
                        }
                        else
                        {
                            enemyAlliance.Cash -= buyAmount;
                            
                            enemyAlliance.Forces[i].addMember(Class.Archer);
                            enemyAlliance.Forces[i].addMember(Class.Priest);
                            activityFeed.text += "Bought an archer and priest for team " + i+"\n";
                            enemyAlliance.setMakeSoldiers(false);
                        }
                        enemyMoves = 0;
                        //yield break;
                    }
                }

                for (int i = 0; i < forcesAmount; i++)
                {
                    if (enemyAlliance.soldierType != Class.None && enemyAlliance.Forces[i].numberMems < max_Team)
                    {
                        buyAmount = enemyAlliance.soldierType.GetPrice();
                        if (enemyAlliance.Cash < buyAmount && enemyAlliance.getStructOwned((int)Structure.Market))
                        {
                            SellOperation(buyAmount);
                        }
                        else
                        {
                            enemyAlliance.Cash -= buyAmount;
                            enemyAlliance.Forces[i].addMember(enemyAlliance.soldierType);
                            activityFeed.text += "Bought a " + enemyAlliance.soldierType + " for team " + i + "\n";
                            enemyAlliance.soldierType = (Class)Random.Range(1, 4);
                        }
                        //yield break;
                    }
                    if (enemyAlliance.Cash < buyAmount)
                    {
                        break;
                    }
                }
                enemyAlliance.setMakeSoldiers(false);
                enemyAlliance.soldierType = Class.None;
                enemyMoves = 0;
            }
            //SEE IF TEAM HAS ENOUGH MONEY FOR THE STRUCTURE, AND IF ALL STRUCTURES AREN'T BOUGHT ALREADY
            else if ((enemyAlliance.getStructObj() != Structure.Nothing) && (enemyAlliance.Cash >= enemyAlliance.getStructObj().GetPrice()))
            {
                enemyAlliance.Cash -= enemyAlliance.getStructObj().GetPrice();
                enemyAlliance.setStructOwned((int)enemyAlliance.getStructObj(), true);
                activityFeed.text += "The enemy team bought the "+enemyAlliance.getStructObj() + "\n";
                if (enemyAlliance.getStructObj() != Structure.Nothing)
                {
                    enemyAlliance.setStructObj((int)enemyAlliance.getStructObj() + 1);
                }
                enemyMoves = 0;
            }
            if (enemyAlliance.getStructOwned(0))
            {
                int foodAmount = 0;
                bool foodBought = false;
                for(int i = 0; i<max_Team; i++)
                {
                    if (enemyAlliance.Cash < Stats.lowHealPrice)
                    {
                        break;
                    }
                    if (enemyAlliance.Forces[eCurTeam].getMember(i).role != null)
                    {
                        int healthAmount = enemyAlliance.Forces[eCurTeam].getMember(i).getHealth();
                        int healthLost = enemyAlliance.Forces[eCurTeam].getMember(i).getHealthCap() - healthAmount;
                        if (healthLost >= Stats.highHealAmount && enemyAlliance.Cash >= Stats.highHealPrice)
                        {
                            enemyAlliance.Cash -= Stats.highHealPrice;
                            foodAmount += Stats.highHealPrice;
                            enemyAlliance.Forces[eCurTeam].getMember(i).setHealth(healthAmount + Stats.highHealAmount);
                            foodBought = true;
                        } else if(healthLost >= Stats.lowHealAmount)
                        {
                            enemyAlliance.Cash -= Stats.lowHealPrice;
                            foodAmount += Stats.lowHealPrice;
                            enemyAlliance.Forces[eCurTeam].getMember(i).setHealth(healthAmount + Stats.lowHealAmount);
                            foodBought = true;
                        }
                    }
                }
                if (foodBought)
                {
                    activityFeed.text += "The enemy team bought some food for " + foodAmount + " gold\n";
                }
            }
            enemyAlliance.Forces[eCurTeam].setObjective(Objective.Nothing);
            yield break;
        }

        //IF THE TEAMS OBJECTIVE IS TO GET GOLD AND IT'S AT A SHOP, START SELLING
        if (enemyAlliance.Forces[eCurTeam].getObjective() == Objective.GetGold)
        {
            if (maze.roomName[newPos.x, newPos.y] == RoomType.Shop)
            {
                SellOperation(enemyAlliance.getStructObj().GetPrice());
            }
            //END OF FUNCTION
        }
        //END OF FUNCTION

        yield return null;
    }

    //
    //  ADD TO THE LIST OF ENEMY OBJECTIVES

    IEnumerator AddObjectives()
    {
        // BEHAVIOUR FOR GET WOOD: IF WOOD < 15, ADD OBJECTIVE
        if (!enemyAlliance.getObjectives().Contains(Objective.GetWood))
        {
            if (enemyAlliance.Wood <= 15)
            {
                Debug.Log("Need wood");
                enemyAlliance.addObjective(Objective.GetWood);
            }
        }

        // BEHAVIOUR FOR GET IRON: IF IRON < 5, ADD OBJECTIVE
        if (!enemyAlliance.getObjectives().Contains(Objective.GetIron))
        {
            if (enemyAlliance.Iron <= 5)
            {
                Debug.Log("Need iron");
                enemyAlliance.addObjective(Objective.GetIron);
            }
        }

        //BEHAVIOUR FOR MAKE SOLDIERS: IF A CERTAIN AMOUNT OF TEAMS ARE EMPTY, GIVEN THE AMOUNT OF STRUCTURES OBTAINED, ADD THIS OBJECTIVE
        if (!enemyAlliance.getMakeSoldiers())
        {
            if (enemyAlliance.getStructOwned((int)Structure.Nothing - 1))
            {
                bool raidReady = true;
                for(int i = 0; i<max_Forces; i++)
                {
                    if(enemyAlliance.Forces[i].numberMems < 3)
                    {
                        raidReady = false;
                        break;
                    }
                }
                enemyAlliance.raidReady = raidReady;

                enemyAlliance.setMakeSoldiers(true);
                enemyAlliance.soldierType = (Class)Random.Range(1, 4);
                yield break;
            }
            int forcesAmount = 2;
            if (enemyAlliance.getStructOwned(0)) { forcesAmount = 3; }
            if (enemyAlliance.getStructOwned(2)) { forcesAmount = 4; }
            if (enemyAlliance.getStructOwned(3)) { forcesAmount = 5; }
            for (int i = 0; i < forcesAmount; i++)
            {
                if (enemyAlliance.Forces[i].numberMems < 2)
                {
                    enemyAlliance.setMakeSoldiers(true);
                    activityFeed.text += "Need to make soldiers for team " + i +"\n";
                    break;
                }
            }
        }

        yield return null;
    }

    //
    // GIVE THE CURRENT ENEMY TEAM AN OBJECTIVE FROM THE LIST OF OBJECTIVES

    IEnumerator SetTeamObjective()
    {
        if(enemyAlliance.Forces[eCurTeam].bloodlust > 5)
        {
            if (enemyAlliance.Forces[eCurTeam].getObjective() == Objective.Bloodlust)
            {
                enemyAlliance.Forces[eCurTeam].bloodlust = 0;
            }
            enemyAlliance.Forces[eCurTeam].setObjective(Objective.Bloodlust);
            yield break;
        } else if(enemyAlliance.Forces[eCurTeam].getObjective() == Objective.Bloodlust && enemyAlliance.Forces[eCurTeam].bloodlust == 0)
        {
            enemyAlliance.Forces[eCurTeam].setObjective(Objective.Nothing);
        }

        //WHAT TO DO IF STRUCTURES AREN'T OWNED OR WE NEED TO MAKE SOLDIERS
        if ((enemyAlliance.getStructObj() != Structure.Nothing) || enemyAlliance.getMakeSoldiers())
        {
            //GET COORESPONDING CASH AMOUNT FOR STRUCTURE
            int cashAmount = 0;
            if (enemyAlliance.getMakeSoldiers())
            {
                if(enemyAlliance.soldierType != Class.None)
                {
                    cashAmount = enemyAlliance.soldierType.GetPrice();
                }
                else
                {
                    cashAmount = Class.Archer.GetPrice() + Class.Priest.GetPrice();
                }
            }
            else
            {
                cashAmount = enemyAlliance.getStructObj().GetPrice();
            }
            
            //IF WE HAVE ENOUGH GOLD
            if (enemyAlliance.Cash >= cashAmount)
            {
                //IF WE HAVE ENOUGH GOLD, STOP COLLECTING GOLD AND A SINGLE TEAM GO THE THE BASE
                bool buyingBase = false;
                for (int j = 0; j < max_Team; j++)
                {
                    if (enemyAlliance.Forces[j].getObjective() == Objective.GoToBase) { buyingBase = true; break; }
                    else if (enemyAlliance.Forces[j].getObjective() == Objective.GetGold) { enemyAlliance.Forces[j].setObjective(Objective.Nothing); }
                }
                if (!buyingBase)
                {
                    enemyAlliance.removeObjective(Objective.GetGold);
                    enemyAlliance.Forces[eCurTeam].setObjective(Objective.GoToBase);
                    enemyAlliance.Forces[eCurTeam].setDirection(enemyBasePosition);
                    yield break;
                }
            }
            //IF WE DON'T HAVE ENOUGH GOLD, SEE IF WE CAN LIQUIDATE RESOURCES TO THE AMOUNT NEEDED
            else if ((int)(enemyAlliance.Wood * Stats.woodPrice * 0.7f) + (int)(enemyAlliance.Iron * Stats.ironPrice * 0.7f) > cashAmount)
            {
                //IF WE HAVE ENOUGH RESOURCES FOR THE GOLD NEEDED, STOP COLLECTING RESOURCES
                if ((enemyAlliance.Forces[eCurTeam].getObjective() == Objective.GetWood) || (enemyAlliance.Forces[eCurTeam].getObjective() == Objective.GetIron))
                {
                    enemyAlliance.Forces[eCurTeam].setObjective(Objective.Nothing);
                }
                //IF WE OWN A MARKET, GO TO THE BASE TO SELL. ELSE, FIND A SHOP ON THE MAP
                if (enemyAlliance.getStructOwned((int)Structure.Market))
                {
                    enemyAlliance.Forces[eCurTeam].setObjective(Objective.GoToBase);
                    enemyAlliance.Forces[eCurTeam].setDirection(enemyBasePosition);
                }
                else if (!enemyAlliance.getObjectives().Contains(Objective.GetGold))
                {
                    enemyAlliance.addObjective(Objective.GetGold);
                }
            }
        }
        yield return null;
        //END OF STRUCTURE OPERATIONS
        if (enemyAlliance.raidReady)
        {
            Debug.Log("The raid is starting!");
            activityFeed.text = "<color=red>The enemy team is declaring a raid</color>\n";
            enemyAlliance.Forces[eCurTeam].setDirection(playerBasePosition);
            enemyAlliance.Forces[eCurTeam].setObjective(Objective.GoToBase);
        }

        //IF WE STILL DON'T HAVE AN OBJECTIVE, FIND ONE FROM OUR LIST TO COMPLETE
        if ((enemyAlliance.Forces[eCurTeam].getObjective() == Objective.Nothing) && (enemyAlliance.getObjectives().Count > 0))
        {
            //CHECK LOCATIONS FOR ONES THAT SATISFY OBJECTIVES, SUCH AS FORESTS, MINES AND SHOPS
            Position[] position = enemyAlliance.getLocations();
            Debug.Log("Position0: x," + position[0].x + "   y," + position[0].y);
            if (!position[0].Equals(new Position()) && !enemyAlliance.getStructOwned((int)Structure.Market))
            {
                if (enemyAlliance.getMakeSoldiers() || enemyAlliance.removeObjective(Objective.GetGold))
                {
                    enemyAlliance.Forces[eCurTeam].setObjective(Objective.GetGold);
                    enemyAlliance.Forces[eCurTeam].setDirection(position[0]);
                    yield break;
                }
            }
            Debug.Log("Position1: x," + position[1].x + "   y," + position[1].y);
            if (!position[1].Equals(new Position()) && enemyAlliance.removeObjective(Objective.GetWood))
            {
                Debug.Log("Set new direction for FOREST");
                enemyAlliance.Forces[eCurTeam].setObjective(Objective.GetWood);
                enemyAlliance.Forces[eCurTeam].setDirection(position[1]);
                yield break;
            }
            Debug.Log("Position2: x," + position[2].x + "   y," + position[2].y);
            if (!position[2].Equals(new Position()) && enemyAlliance.removeObjective(Objective.GetIron))
            {
                Debug.Log("Set new direction for MINE");
                enemyAlliance.Forces[eCurTeam].setObjective(Objective.GetIron);
                enemyAlliance.Forces[eCurTeam].setDirection(position[2]);
                yield break;
            }
        }
        yield return null;
        //END OF LOCATION OPERATION
    }

    //
    // PERFORMS BOARD MOVEMENT OPERATIONS FOR THE AI DURING ITS TURN
    // AND TOTALS THE GAINED RESOURCES ONCE THE TURN IS OVER

    IEnumerator EnemyTurn()
    {
        Debug.Log("EnemyTurn!");
        activityFeed.text += "<u><b>Round " + turnNumber + "</b></u>\n";
        eCurTeam = 0;

        yield return AddObjectives();

        while (!playerTurn)
        {
            if (enemyAlliance.Forces[eCurTeam].numberMems > 0)
            {
                // SET DESTINATION FOR OBJECTIVE IF OBJECTIVE SATISFIER IS IN RECORDED LOCATIONS, AND THEN REMOVE OBJECTIVE
                yield return SetTeamObjective();

                //IF ENEMY TEAM IS NOT TRYING TO SELL SOMETHING AND HAS MORE THAN ONE MEMBER, INCREMENT BLOODLUST COUNTER BY THE AMOUNT OVER THAT
                if(enemyAlliance.Forces[eCurTeam].getObjective() != Objective.GetGold && enemyAlliance.Forces[eCurTeam].getObjective() != Objective.GoToBase && enemyAlliance.Forces[eCurTeam].getObjective() != Objective.Bloodlust)
                {
                    if (enemyAlliance.Forces[eCurTeam].numberMems > 1)
                    {
                        enemyAlliance.Forces[eCurTeam].bloodlust += 1;
                    }
                    else
                    {
                        enemyAlliance.Forces[eCurTeam].bloodlust = 0;
                    }
                }

                enemyMoves = MOVEAMOUNT;

                if (enemyAlliance.Forces[eCurTeam].getPosition().Equals(new Position(-1, -1)))
                {
                    moveRooms(enemyBasePosition.x, enemyBasePosition.y);
                }
                yield return AIGetDestination();
                Debug.Log("Objective = "+ enemyAlliance.Forces[eCurTeam].getObjective() + ", for Team "+ eCurTeam);

                while (enemyMoves > 0)
                {
                    AIMoveDecision();

                    enemyMoves--;
                    while (inBattle || raiding)
                    {
                        yield return null;
                    }

                    if (enemyAlliance.Forces[eCurTeam].getPosition().Equals(enemyAlliance.Forces[eCurTeam].getDirection()) || (enemyAlliance.Forces[eCurTeam].numberMems == 0))
                    {
                        enemyMoves = 0;
                    }

                    if(enemyAlliance.Forces[eCurTeam].numberMems != 0)
                    {
                        yield return AIObjectiveCheck();
                    }
                }
            }

            while (eCurTeam < enemyAlliance.Forces.Length)
            {
                eCurTeam++;
                if (eCurTeam == enemyAlliance.Forces.Length)
                {
                    playerTurn = true;
                    break;
                }
                else if (enemyAlliance.Forces[eCurTeam].numberMems != 0)
                {
                    prevx = -1;
                    prevy = -1;
                    break;
                }
            }
            yield return null;
        }

        int ironGained = 0;
        int woodGained = 0;

        foreach (Team team in enemyAlliance.Forces)
        {
            if (team.getPosition().x != -1)
            {
                RoomType temp = maze.roomName[team.getPosition().x, team.getPosition().y];
                if (temp == RoomType.Mine)
                {
                    enemyAlliance.Iron += 5;
                    ironGained += 5;
                }
                else if (temp == RoomType.Forest)
                {
                    enemyAlliance.Wood += 10;
                    woodGained += 5;
                }
            }
        }
        activityFeed.text += "Enemy team gained " + ironGained + " iron and " + woodGained + " wood this turn\n";
        if (enemyAlliance.getStructOwned((int)Structure.Mine))
        {
            enemyAlliance.Iron += 8;
            activityFeed.text += "Enemy team's mine gave them 8 iron\n";
        }
        if (enemyAlliance.getStructOwned((int)Structure.Forestry))
        {
            enemyAlliance.Wood += 15;
            activityFeed.text += "Enemy team's forestry gave them 15 wood\n";
        }
        if (enemyAlliance.getStructOwned((int)Structure.Bank))
        {
            enemyAlliance.Cash += 30;
            activityFeed.text += "Enemy team's bank gave them 30 gold\n";
        }

        activityFeed.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, activityFeed.GetComponent<RectTransform>().anchoredPosition.y);

        prevx = -1;
        prevy = -1;
        yield return PlayerTurn();
    }
}
