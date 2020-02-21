using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ClassMethods;
using TMPro;

public class ShopScript : MonoBehaviour {
    public GameObject buyButton;
    public GameObject sellButton;
    public GameObject MovesFullPanel;
    public TMP_InputField amount;

    public enum Shop
    {
        General = 1,
        Warrior = 2,
        Archer = 3,
        Magic = 4,
        Barracks = 5,
        Market = 6
    }
    int max = 5;
    public class Item{
        private object item;
        private string name;
        private int price;
        private string description;
        private bool sellOption;
        private bool buyOption;
        private Class classOnly;
        public Item(object item, string name, int price, string description, bool buyOption, bool sellOption, Class _class)
        {
            this.item = item;
            this.name = name;
            this.price = price;
            this.description = description;
            this.sellOption = sellOption;
            this.buyOption = buyOption;
            this.classOnly = _class;
        }

        public Item(object item, string name, int price, string description) : this(item, name, price, description, true, false, Class.None)
        {}

        public Item(object item, string name, int price, string description, Class _class) : this(item, name, price, description, true, false, _class)
        { }

        public Item(object item, string name, int price, string description, bool buyOption, bool sellOption) : this(item, name, price, description, buyOption, sellOption, Class.None)
        { }

        public int getPrice()
        {
            return price;
        }
        public object getItem()
        {
            return item;
        }
        public string getName()
        {
            return name;
        }
        public string getDesc()
        {
            return description;
        }
        public bool getSellOption()
        {
            return sellOption;
        }

        public bool getBuyOption()
        {
            return buyOption;
        }

        public Class getClass()
        {
            return classOnly;
        }
    }

    Shop shop;
    float timer = 0.0f;
    bool opening = true;
    Vector2 initPos;
    Vector2 finalPos;
    Coroutine messageRoutine;

    IEnumerator PurchaseMessage(string message)
    {
        string name = "";
        string description = "";
        string price = "";
        if(num != -1)
        {
            name = "" + shopItems[num].getName();
            description = "" + shopItems[num].getDesc();
            price = "" + shopItems[num].getPrice();
        }
        transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = "";
        transform.Find("ItemDescription").GetComponent<TextMeshProUGUI>().text = message;
        transform.Find("ItemPrice").GetComponent<TextMeshProUGUI>().text = "";
        yield return new WaitForSeconds(1.5f);
        transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = name;
        transform.Find("ItemDescription").GetComponent<TextMeshProUGUI>().text = description;
        transform.Find("ItemPrice").GetComponent<TextMeshProUGUI>().text = price;
        yield return null;
    }

    IEnumerator MoveCam(Vector2 posInit, Vector2 posFinal, float speed)
    {
        while (timer < 1.0f)
        {
            transform.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(initPos, finalPos, timer);
            timer += Time.deltaTime*2*speed;
            yield return null;
        }
        transform.GetComponent<RectTransform>().anchoredPosition = posFinal;
        if (!opening)
        {
            gameObject.SetActive(false);
        }
        timer = 0;
    }

    public void chooseAction(Button thisButton)
    {
        if (messageRoutine != null) StopCoroutine(messageRoutine);
        int num = int.Parse(thisButton.name);
        buyButton.name = thisButton.name;
        buyButton.SetActive(shopItems[num].getBuyOption());
        sellButton.name = thisButton.name;
        sellButton.SetActive(shopItems[num].getSellOption());
        amount.gameObject.SetActive(shopItems[num].getSellOption() || shopItems[num].getItem() is Class);
        amount.text = "";
        if (shopItems[num].getItem() is Class)
        {
            amount.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Team (0-4)";
        } else
        {
            amount.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Amount";
        }
        transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = shopItems[num].getName();
        transform.Find("ItemDescription").GetComponent<TextMeshProUGUI>().text = shopItems[num].getDesc();
        transform.Find("ItemPrice").GetComponent<TextMeshProUGUI>().text = shopItems[num].getPrice()+" ea";
    }

    int num = -1;
    public void buyAction(Button button)
    {
        if (messageRoutine != null) StopCoroutine(messageRoutine);
        if (buyButton.name == "Buy")
        {
            messageRoutine = StartCoroutine(PurchaseMessage("Select an option!"));
            return;
        }
        num = int.Parse(button.name);
        MazeScript scr = GameObject.Find("EventSystem").GetComponent<MazeScript>();
        if (shopItems[num].getItem() == null) return;

        int totalPrice = shopItems[num].getPrice();
        int itemAmount = 1;
        if (shopItems[num].getItem() is int && amount.gameObject.activeSelf) {
            if(amount.text == "") { messageRoutine = StartCoroutine(PurchaseMessage("You have to choose an amount to buy")); return; }
            itemAmount = int.Parse(amount.text);
            totalPrice += itemAmount;
        }

        if (scr.alliance.Cash >= totalPrice)
        {
            if (shopItems[num].getItem() is AttackMove)
            {

                if (shopItems[num].getClass() != scr.alliance.Forces[scr.curTeam].getMember(scr.curChar).GetClass() && shopItems[num].getClass() != Class.None)
                {
                    messageRoutine = StartCoroutine(PurchaseMessage("Your current character can't learn this move")); return;
                }

                int ind;
                ind = GameObject.Find("EventSystem").GetComponent<MazeScript>().giveAttackMove(shopItems[num].getItem() as AttackMove);
                if (ind != -1)
                {
                    scr.alliance.Cash -= totalPrice;
                } else
                {
                    boughtMove = shopItems[num].getItem() as AttackMove;
                    boughtMovePrice = totalPrice;
                    MovesFull();
                }
            }
            else if (shopItems[num].getItem() is Class)
            {
                int type = (int)shopItems[num].getItem();
                if (amount.text == "") { messageRoutine = StartCoroutine(PurchaseMessage("You have to choose a team to add to")); return; }
                int teamAdd = int.Parse(amount.text);
                if (teamAdd > Stats.numberOfTeams-1 || teamAdd < 0) { amount.text = ""; return; }
                if (scr.alliance.Forces[teamAdd].numberMems < 4)
                {
                    scr.alliance.Forces[teamAdd].addMember((Class)type);
                    scr.alliance.Cash -= totalPrice;
                    if(teamAdd == scr.curTeam)
                    {
                        scr.updateCharPic(teamAdd);
                    }
                } else
                {
                    messageRoutine = StartCoroutine(PurchaseMessage("Your team is full!"));
                }
            }
            else if(shopItems[num].getItem() is int)
            {
                if(shopItems[num].getName() == "Wood")
                {
                    scr.alliance.Wood += (int)shopItems[num].getItem() * itemAmount;
                    GameObject.Find("Canvas").transform.Find("OutOfBattleGUI").Find("CharPics").Find("TeamStats").Find("Wood").GetChild(0).GetComponent<TextMeshProUGUI>().text = ""+scr.alliance.Wood;
                } else if (shopItems[num].getName() == "Iron")
                {
                    scr.alliance.Iron += (int)shopItems[num].getItem() * itemAmount;
                    GameObject.Find("Canvas").transform.Find("OutOfBattleGUI").Find("CharPics").Find("TeamStats").Find("Iron").GetChild(0).GetComponent<TextMeshProUGUI>().text = "" + scr.alliance.Iron;
                } else if (scr.alliance.Forces[scr.curTeam].getMember(scr.curChar).getHealth() < scr.alliance.Forces[scr.curTeam].getMember(scr.curChar).getHealthCap())
                {
                    scr.alliance.Forces[scr.curTeam].heal((int)shopItems[num].getItem(), scr.curChar);
                } else
                {
                    messageRoutine = StartCoroutine(PurchaseMessage("Health is already full!"));
                    return;
                }
                scr.alliance.Cash -= totalPrice;
            }
            GameObject.Find("Canvas").transform.Find("OutOfBattleGUI").Find("CharPics").Find("TeamStats").Find("Cash").Find("Value").GetComponent<TextMeshProUGUI>().text = "" + scr.alliance.Cash;
        } else
        {
            messageRoutine = StartCoroutine(PurchaseMessage("Not enough money in vault to purchase!"));
        }
    }

    AttackMove boughtMove;
    int boughtMovePrice = 0;
    void MovesFull()
    {
        MazeScript scr = GameObject.Find("EventSystem").GetComponent<MazeScript>();
        Member member = scr.alliance.Forces[scr.curTeam].getMember(scr.curChar);
        for (int i = 1; i < 5; i++)
        {
            MovesFullPanel.transform.Find(""+i).GetChild(0).GetComponent<TextMeshProUGUI>().text = ""+member.getAttackMove(i).getName();
        }
        MovesFullPanel.SetActive(true);
    }

    public void MovesFullChoice(Button button)
    {
        MazeScript scr = GameObject.Find("EventSystem").GetComponent<MazeScript>();
        Member member = scr.alliance.Forces[scr.curTeam].getMember(scr.curChar);

        if(shopItems[num].getClass() != member.GetClass() && shopItems[num].getClass() != Class.None)
        {
            messageRoutine = StartCoroutine(PurchaseMessage("Your current character can't learn this move")); return;
        }

        int temp = int.Parse(button.name);

        member.removeAttackMove(temp);
        member.addAttackMove(boughtMove);
        scr.alliance.Cash -= boughtMovePrice;
        GameObject.Find("Canvas").transform.Find("OutOfBattleGUI").Find("CharPics").Find("TeamStats").Find("Cash").Find("Value").GetComponent<TextMeshProUGUI>().text = "" + scr.alliance.Cash;
    }

    public void sellAction(Button button)
    {
        if (messageRoutine != null) StopCoroutine(messageRoutine);
        if (sellButton.name == "Sell")
        {
            messageRoutine = StartCoroutine(PurchaseMessage("Select an option!"));
            return;
        }

        num = int.Parse(button.name);
        MazeScript scr = GameObject.Find("EventSystem").GetComponent<MazeScript>();
        if (shopItems[num].getItem() == null) return;

        if (shopItems[num].getItem() is int)
        {
            if (amount.text == "" || amount.text == null)
            {
                messageRoutine = StartCoroutine(PurchaseMessage("Choose an amount to sell!"));
                return;
            }
            if (shopItems[num].getName() == "Wood" && scr.alliance.Wood >= (int)shopItems[num].getItem()*int.Parse(amount.text))
            {
                scr.alliance.Wood -= (int)shopItems[num].getItem() * int.Parse(amount.text);
                GameObject.Find("Canvas").transform.Find("OutOfBattleGUI").Find("CharPics").Find("TeamStats").Find("Wood").GetChild(0).GetComponent<TextMeshProUGUI>().text = "" + scr.alliance.Wood;
            }
            else if (shopItems[num].getName() == "Iron" && scr.alliance.Iron >= (int)shopItems[num].getItem() * int.Parse(amount.text))
            {
                scr.alliance.Iron -= (int)shopItems[num].getItem() * int.Parse(amount.text);
                GameObject.Find("Canvas").transform.Find("OutOfBattleGUI").Find("CharPics").Find("TeamStats").Find("Iron").GetChild(0).GetComponent<TextMeshProUGUI>().text = "" + scr.alliance.Iron;
            } else
            {
                messageRoutine = StartCoroutine(PurchaseMessage("You don't have enough resources"));
                return;
            }
            int amountGained = (int)(shopItems[num].getPrice() * int.Parse(amount.text) * 0.70f);
            scr.alliance.Forces[scr.curTeam].getMember(scr.curChar).addGold((int)(shopItems[num].getPrice() * int.Parse(amount.text) * 0.70f));
            GameObject.Find("Canvas").transform.Find("OutOfBattleGUI").Find("CharPics").Find("TeamStats").Find("Cash").Find("Value").GetComponent<TextMeshProUGUI>().text = "" + scr.alliance.Cash;
        }
    }

    public void closeShop()
    {
        MovesFullPanel.SetActive(false);
        if (messageRoutine != null) StopCoroutine(messageRoutine);
        initPos = transform.GetComponent<RectTransform>().anchoredPosition;
        finalPos = new Vector2(-3400, 10);
        opening = false;
        StartCoroutine(MoveCam(initPos, finalPos, 2));
    }

    public void openShop()
    {
        if (messageRoutine != null) StopCoroutine(messageRoutine);
        num = -1;
        buyButton.name = "Buy";
        sellButton.name = "Sell";
        transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = "";
        transform.Find("ItemDescription").GetComponent<TextMeshProUGUI>().text = "";
        transform.Find("ItemPrice").GetComponent<TextMeshProUGUI>().text = "";
        amount.gameObject.SetActive(false);
        buyButton.SetActive(false);
        sellButton.SetActive(false);
        gameObject.SetActive(true);
        initPos = transform.GetComponent<RectTransform>().anchoredPosition;
        finalPos = new Vector2(0, 10);
        opening = true;
        StartCoroutine(MoveCam(initPos, finalPos, 1));
    }

    Item[] shopItems;

    public void initShop(string shopName)
    {
        gameObject.SetActive(true);
        try
        {
            shop = (Shop)(System.Enum.Parse(typeof(Shop), shopName));
        } catch
        {
            shop = 0;
        }
        switch (shop)
        {
            case Shop.General:
                Item[] gItems = new Item[]{
                    new Item(1, "Wood", Stats.woodPrice, "Wood", false, true),
                    new Item(1, "Iron", Stats.ironPrice, "Iron", false, true),
                    new Item(Move.Slice.GetAttackMove(), Move.Slice.GetAttackMove().getName(), 50, "Slice", Class.None),
                    new Item(Stats.lowHealAmount, "Chicken", Stats.lowHealPrice, "Heals "+Stats.lowHealAmount+" HP to the current character"),
                    new Item(Stats.highHealAmount, "Steak", Stats.highHealPrice, "Heals "+Stats.highHealAmount+" HP to the current character"),
                };
                stockShop(gItems);
                shopItems = gItems;
                openShop();
                break;
            case Shop.Archer:
                Item[] aItems = new Item[]{
                    new Item(Class.Archer, "Archer", Class.Archer.GetPrice(), "Archer"),
                    new Item(Move.BearTraps.GetAttackMove(), Move.BearTraps.GetAttackMove().getName(), 450, "Ensnare your enemies, causing a small amount of damage while also stunning them (Archer)", Class.Archer),
                    new Item(3, "", 10, ""),
                    new Item("", "", 10, ""),
                    new Item(5, "", 10, "")
                };
                stockShop(aItems);
                shopItems = aItems;
                openShop();
                break;
            case Shop.Warrior:
                Item[] wItems = new Item[]{
                    new Item(Class.Knight, "Knight", Class.Knight.GetPrice(), "Knight"),
                    new Item(Move.Whirlwind.GetAttackMove(), Move.Whirlwind.GetAttackMove().getName(), 600, "A powerful swing, capable of doing massive damage to an entire army", Class.Knight),
                    new Item(2, "", 10, ""),
                    new Item("", "", 10, ""),
                    new Item(4, "", 10, "")
                };
                stockShop(wItems);
                shopItems = wItems;
                openShop();
                break;
            case Shop.Magic:
                Item[] mItems = new Item[]{
                    new Item(Class.Priest, "Priest", Class.Priest.GetPrice(), "Priest"),
                    new Item(Move.MassHeal.GetAttackMove(), Move.MassHeal.GetAttackMove().getName(), 400, "A strong heal, able to heal your entire party all at once", Class.Priest),
                    new Item(Move.PoisonCloud.GetAttackMove(), Move.PoisonCloud.GetAttackMove().getName(), 600, "", Class.Thief),
                    new Item(null, "", 10, ""),
                    new Item(null, "", 10, "")
                };
                stockShop(mItems);
                shopItems = mItems;
                openShop();
                break;
            case Shop.Barracks:
                Item[] bItems = new Item[]{
                    new Item(Class.Knight, "Knight", Class.Knight.GetPrice(), "Knight"),
                    new Item(Class.Archer, "Archer", Class.Archer.GetPrice(), "Archer"),
                    new Item(Class.Priest, "Priest", Class.Priest.GetPrice(), "Priest"),
                    new Item(Class.Thief, "Thief", Class.Thief.GetPrice(), "Thief"),
                    new Item(Class.Bartender, "Bartender", Class.Bartender.GetPrice(), "Bartender")
                };
                stockShop(bItems);
                shopItems = bItems;
                openShop();
                break;
            case Shop.Market:
                Item[] marketItems = new Item[]{
                    new Item(1, "Wood", Stats.woodPrice, "Wood", false, true),
                    new Item(1, "Iron", Stats.ironPrice, "Iron", false, true),
                    new Item(Stats.lowHealAmount, "Chicken", Stats.lowHealPrice, "Heals "+Stats.lowHealAmount+" HP to the current character"),
                    new Item(Stats.highHealAmount, "Steak", Stats.highHealPrice, "Heals "+Stats.highHealAmount+" HP to the current character"),
                    new Item(null, "", 0, "")
                };
                stockShop(marketItems);
                shopItems = marketItems;
                openShop();
                break;
            case 0:
                Item[] lItems = new Item[]{
                    new Item(1, "Damn", 50, ""),
                    new Item("", "You", 10, ""),
                    new Item(3, "Messed", 10, ""),
                    new Item("", "Up", 10, ""),
                    new Item(5, "Big", 10, "")
                };
                stockShop(lItems);
                shopItems = lItems;
                openShop();
                break;
        }
    }

    void stockShop(Item[] items)
    {
        for(int i = 0; i<max; i++)
        {
            transform.Find("" + i).GetChild(0).GetComponent<TextMeshProUGUI>().text = ""+items[i].getName();
        }
        transform.Find("Title").GetComponent<TextMeshProUGUI>().text = "" + shop;
        if(shop != Shop.Barracks && shop != Shop.Market)
        {
            transform.Find("Title").GetComponent<TextMeshProUGUI>().text += " Shop";
        }
    }
}
