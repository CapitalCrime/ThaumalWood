using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ClassMethods
{
    public static class TeamExtensions
    {
        public static void GetLoadOut(this Team team, TeamLoadOut LoadOut)
        {
            Member[] newTeam = new Member[4];
            int numberMembers = 0;
            switch (LoadOut)
            {
                case TeamLoadOut.NPCOne:
                    newTeam[0] = new Member(Class.Thief, 0);
                    newTeam[1] = new Member(Class.Thief, 1);
                    newTeam[0].setGold(65);
                    newTeam[1].setGold(65);
                    break;
                case TeamLoadOut.NPCTwo:
                    newTeam[0] = new Member(Class.Archer, 0);
                    newTeam[0].setGold(80);
                    break;
                case TeamLoadOut.NPCThree:
                    newTeam[0] = new Member(Class.Bartender, 0);
                    newTeam[1] = new Member(Class.Priest, 1);
                    newTeam[0].setGold(55);
                    newTeam[1].setGold(65);
                    break;
                case TeamLoadOut.Alliance:
                    newTeam[0] = new Member(Class.Knight, 0);
                    newTeam[1] = new Member(Class.Archer, 1);
                    break;
                case TeamLoadOut.AllianceTwo:
                    newTeam[0] = new Member(Class.Archer, 0);
                    newTeam[1] = new Member(Class.Priest, 1);
                    break;
                case TeamLoadOut.AllianceOP:
                    newTeam[0] = new Member(Class.Archer, 0);
                    newTeam[1] = new Member(Class.Archer, 1);
                    newTeam[2] = new Member(Class.Archer, 2);
                    newTeam[3] = new Member(Class.Archer, 3);
                    break;
            }
            for (int i = 0; i < 4; i++)
            {
                if(newTeam[i] == null)
                {
                    newTeam[i] = new Member(i);
                } else
                {
                    if(team.getGroup() != null)
                    {
                        newTeam[i].role.transform.parent = team.getGroup().transform;
                    }
                    numberMembers++;
                }
            }
            team.numberMems = numberMembers;
            team.setTeam(newTeam);
            return;
        }
    }

    public enum TeamLoadOut{
        NPCOne,
        NPCTwo,
        NPCThree,
        Alliance,
        AllianceTwo,
        AllianceOP
    }

    public class Member
    {
        public GameObject role { get; set; }
        private int swapSafety;
        private int poisonCounter;
        private Class Class;
        private AttackMove[] attackMoves;
        private int health;
        private int healthCap;
        private int speed;
        private int speedCap;
        private int index;
        private int level;
        private double experience;
        private int gold;
        public bool healCooldown { get; set; }
        private int attackMult;
        private int defenceMult;
        private int damageDone;

        public Member(Class playerClass, int i)
        {
            int[] statArray = playerClass.GetStats();
            GameObject member = null;
            if(playerClass != Class.None)
            {
                member = Object.Instantiate(Resources.Load("Characters/" + playerClass) as GameObject, Vector3.zero, Quaternion.identity);
                member.name = "" + playerClass;
                member.SetActive(false);
            }
            this.role = member;
            Class = playerClass;
            health = statArray[0];
            healthCap = statArray[0];
            speed = statArray[1];
            speedCap = statArray[1];
            attackMoves = Class.GetStartingMoves(5);
            level = 0;
            experience = 0.0f;
            index = i;
            gold = 0;
            healCooldown = false;
            attackMult = 0;
            defenceMult = 0;
            damageDone = 0;
            swapSafety = 0;
            poisonCounter = 0;
        }

        public Member(int i) : this(Class.None, i)
        {}

        public void resetValues()
        {
            health = 0;
            healthCap = 0;
            speed = 0;
            speedCap = 0;
            attackMoves = new AttackMove[5];
            level = 0;
            experience = 0.0f;
            damageDone = 0;
            swapSafety = 0;
            healCooldown = false;
            poisonCounter = 0;
        }

        public Class GetClass()
        {
            return Class;
        }

        #region Attack Move
        public int addAttackMove(AttackMove move)
        {
            for (int i = 0; i < 5; i++)
            {
                if ((attackMoves[i] == null) || (attackMoves[i].getName() == null))
                {
                    attackMoves[i] = move;
                    return i;
                }
            }
            return -1;
        }

        public void removeAttackMove(int i)
        {
            attackMoves[i] = null;
        }

        public AttackMove getAttackMove(int i)
        {
            if ((i < 5) && (attackMoves[i] != null))
            {
                return attackMoves[i];
            }
            else
            {
                return new AttackMove();
            }
        }
        #endregion

        #region Damage Done
        public void addDamageDone(int amount)
        {
            damageDone += amount;
        }

        public int getDamageDone()
        {
            return damageDone;
        }

        public void setDamageDone(int amount)
        {
            damageDone = amount;
        }
        #endregion

        #region Attack Multiplier
        public void addAttackMult(int amount)
        {
            attackMult += amount;
        }
        public int getAttackMult()
        {
            return attackMult;
        }
        public void resetAttackMult()
        {
            attackMult = 0;
        }
        #endregion

        #region Swap Safety
        public void addSwapSafety(int amount)
        {
            swapSafety += amount;
        }

        public void setSwapSafety(int amount)
        {
            swapSafety = amount;
        }

        public int getSwapSafety()
        {
            return swapSafety;
        }
        #endregion

        #region Poison Counter
        public int getPoisonCounter()
        {
            return poisonCounter;
        }

        public void setPoisonCounter(int amount)
        {
            poisonCounter = amount;
        }

        public void addPoisonCounter(int amount)
        {
            poisonCounter += amount;
        }
        #endregion

        #region Defence Multiplier
        public void addDefenceMult(int amount)
        {
            defenceMult += amount;
        }

        public void resetDefenceMult()
        {
            defenceMult = 0;
        }

        public int getDefenceMult()
        {
            return defenceMult;
        }
        #endregion

        #region Speed and Speed Cap
        public int getSpeed()
        {
            return speed;
        }

        public int getSpeedCap()
        {
            return speedCap;
        }

        public void subSpeed(int amount)
        {
            speed -= amount;
            if (speed < 0)
            {
                speed = 0;
            }
        }

        public void resetSpeed()
        {
            speed = speedCap;
        }

        public void setSpeedCap(int amount)
        {
            speedCap = amount;
            speed = speedCap;
        }

        public void setSpeed(int amount)
        {
            speed = amount;
            if(speedCap < speed)
            {
                speedCap = speed;
            }
        }
        #endregion

        #region Health and Health Cap
        public int getHealth()
        {
            return health;
        }

        public int getHealthCap()
        {
            return healthCap;
        }

        public void setHealth(int amount)
        {
            health = amount;
        }

        public void hurt(int amount)
        {
            health -= amount;
        }
        #endregion

        #region Experience and Level
        public void addExperience(double amount, bool ai)
        {
            experience += amount;
            if (level < 5)
            {
                int levelFormula = (int)(Mathf.Pow(1.85f, (level / 1.5f) * 1.1f) * 65.0f);
                while (experience >= levelFormula)
                {
                    experience = experience - levelFormula;
                    level += 1;
                    healthCap += (int)(Class.GetStats()[0]/4.0f);
                    health = healthCap;
                    if (level == 5) break;
                }
            }
        }
        public double getExperience()
        {
            return experience;
        }

        public int getLevel()
        {
            return level;
        }
        #endregion

        #region Index
        public void setIndex(int position)
        {
            index = position;
        }
        public int getIndex()
        {
            return index;
        }
        #endregion

        #region Gold
        public void setGold(int amount)
        {
            gold = amount;
        }
        public int getGold()
        {
            return gold;
        }
        public void addGold(int amount)
        {
            gold += amount;
        }
        #endregion

    }

    public class EnemyTeam : Team
    {
        private Objective objective;
        private Position direction;
        private float intelligence;
        public int bloodlust = 0;

        public EnemyTeam(float intelligence) : base(true) {
            objective = Objective.Nothing;
            direction = new Position();
            this.intelligence = intelligence;
        }

        public Objective getObjective()
        {
            return objective;
        }

        public void setObjective(Objective objective)
        {
            this.objective = objective;
        }

        public void setDirection(Position position)
        {
            if(position != null)
            {
                direction = position;
            }
        }
        public Position getDirection()
        {
            return direction;
        }

        public float getIntelligence()
        {
            return intelligence;
        }
    }

    public class NPCTeam : EnemyTeam
    {
        public NPCTeam(float intelligence = 0.85f) : base(intelligence)
        {
            GameObject del = group;
            group = null;
            if(del != null) { GameObject.Destroy(del); }
        }
    }

    public enum Objective
    {
        Nothing,
        GoToBase,
        GetGold, GetWood, GetIron,
        Bloodlust
    }

    public class Position : System.IEquatable<Position>
    {
        public int x;
        public int y;

        public Position()
        {
            x = -1;
            y = -1;
        }
        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public bool Equals(Position other)
        {
            if ((x == other.x) && (y == other.y))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class Team
    {
        const int max_Team = 4;
        const int max_Forces = 5;

        public bool ai;
        private Member[] teamMates;
        public GameObject group;
        public int numberMems = 0;
        private Position position;

        public Team(bool ai)
        {
            this.ai = ai;
            position = new Position();
            teamMates = new Member[max_Team];
            group = new GameObject();
            for (int i = 0; i < max_Team; i++)
            {
                teamMates[i] = new Member(i);
            }
        }

        public void addMember(Class playerClass)
        {
            for (int i = 0; i < max_Team; i++)
            {
                if (this.teamMates[i].role == null)
                {
                    teamMates[i] = new Member(playerClass, i);

                    if (group != null)
                    {
                        this.teamMates[i].role.transform.parent = group.transform;
                    }
                    numberMems++;
                    if (!ai)
                    {
                        GameObject.Find("CharPics").transform.Find("TeamStats").Find("" + i).Find("Experience").GetComponent<TextMeshProUGUI>().text = "" + teamMates[i].getExperience();
                        GameObject.Find("CharPics").transform.Find("TeamStats").Find("" + i).Find("Level").GetComponent<TextMeshProUGUI>().text = "" + teamMates[i].getLevel();
                    }
                    return;
                }
            }
        }

        public void swapMember(int swapFrom, int swapTo)
        {
            if((teamMates[swapTo].role != null) || (teamMates[swapFrom].role != null))
            {
                Member temp = teamMates[swapFrom];
                teamMates[swapFrom] = teamMates[swapTo];
                teamMates[swapTo] = temp;
                teamMates[swapFrom].setIndex(swapFrom);
                teamMates[swapTo].setIndex(swapTo);
            }
        }
        public void setPosition(int x, int y)
        {
            position.x = x;
            position.y = y;
        }
        public Position getPosition()
        {
            return position;
        }
        public Member calculateFastest()
        {
            int fastest = -1;
            for (int i = 0; i < max_Team; i++)
            {
                if (this.teamMates[i].role != null)
                {
                    if (fastest == -1)
                    {
                        fastest = i;
                    } else
                    {
                        if (this.teamMates[i].getSpeed() < this.teamMates[fastest].getSpeed())
                        {
                            fastest = i;
                            break;
                        }
                        else if (this.teamMates[i].getSpeed() == this.teamMates[fastest].getSpeed())
                        {
                            if (this.teamMates[i].getSpeedCap() > this.teamMates[fastest].getSpeed())
                            {
                                fastest = i;
                                break;
                            }
                        }
                    }
                }
            }
            return teamMates[fastest];
        }
        public Member[] getTeam()
        {
            return teamMates;
        }
        public void setTeam(Member[] mem)
        {
            teamMates = mem;
        }
        public void subtractSpeeds(int amount)
        {
            for (int i = 0; i < max_Team; i++)
            {
                this.teamMates[i].subSpeed(amount);
            }
        }
        public Member getMember(int i)
        {
            return this.teamMates[i];
        }
        public void setTeamName(string name)
        {
            if (group != null)
            {
                group.name = name;
            }
        }

        public GameObject getGroup()
        {
            return group;
        }
        public void dealDamage(int amount, int index)
        {
            if(amount < 0){ amount = 0; }

            getMember(index).hurt(amount);
            if (getMember(index).role != null && (getMember(index).getHealth() <= 0))
            {
                Object.Destroy(getMember(index).role);
                getMember(index).role = null;
                getMember(index).resetValues();
                numberMems--;
                if (numberMems == 0)
                {
                    setPosition(-1, -1);
                }
            }
        }

        public void heal(int amount, int index)
        {
            getMember(index).hurt(-amount);
            if (getMember(index).getHealth() > getMember(index).getHealthCap())
            {
                getMember(index).setHealth(getMember(index).getHealthCap());
            }
        }
    }

    public enum Faction
    {
        Neutral,
        Ally,
        Enemy
    }

    public enum RoomType
    {
        Nothing,
        EnemyBase,
        PlayerBase,
        Shop,
        Enemy,
        Elite,
        Forest,
        Mine
    }

    class EnemyTeams
    {
        public EnemyTeam[] Forces { get; set; }
        public int Cash { get; set; }
        public int Iron { get; set; }
        public int Wood { get; set; }
        public bool raidReady { get; set; }

        private List<Objective> objectives;
        private StructureMethods.Structure structObjective;
        private int numObjectives = Stats.numberOfTeams;
        private Position[] importantLocations;
        private bool[] structOwned;
        private bool makeSoldiers;
        public Class soldierType;

        public EnemyTeams(float intelligence = 1.0f)
        {
            importantLocations = new Position[3] { new Position(), new Position(), new Position()};
            objectives = new List<Objective>();
            structObjective = StructureMethods.Structure.Market;
            structOwned = new bool[(int)StructureMethods.Structure.Nothing];
            makeSoldiers = false;
            soldierType = Class.None;

            Forces = new EnemyTeam[Stats.numberOfTeams];
            for (int i = 0; i < Stats.numberOfTeams; i++)
            {
                Forces[i] = new EnemyTeam(intelligence);
                Forces[i].setTeamName("EnemyTeam" + i + "");
            }
        }

        public void setMakeSoldiers(bool value)
        {
            makeSoldiers = value;
        }

        public bool getMakeSoldiers()
        {
            return makeSoldiers;
        }

        public void setStructObj(int structure)
        {
            structObjective = (StructureMethods.Structure)structure;
        }

        public StructureMethods.Structure getStructObj()
        {
            return structObjective;
        }

        /// <summary>
        /// _type = 0 is SHOP
        /// _type = 1 is FOREST
        /// _type = 2 is MINE
        /// </summary>
        /// <param name="_type"></param>
        /// <returns></returns>
        public void addLocation(Position location, int _type)
        {
            if(_type < importantLocations.Length)
            {
                importantLocations[_type] = location;
            }
        }

        public void addObjective(Objective objective)
        {
            //4
            if (objectives.Count < numObjectives)
            {
                objectives.Add(objective);
            }
        }

        public bool removeObjective(Objective objective)
        {
            return objectives.Remove(objective);
        }

        public void popObjective()
        {
            objectives.RemoveAt(0);
        }

        public bool[] getStructOwnedArray()
        {
            return structOwned;
        }

        public bool getStructOwned(int index)
        {
            return index < structOwned.Length ? structOwned[index] : false;
        }

        public void setStructOwned(int i, bool state)
        {
            if(i < structOwned.Length)
            {
                structOwned[i] = state;
            }
        }

        public void setObjective(Objective objective, int i)
        {
            //4
            if (objectives.Count < numObjectives)
            {
                objectives.Insert(i, objective);
            }
        }
        /*
        public void setDirection(Position direction)
        {
            this.direction = direction;
        }*/

        public List<Objective> getObjectives()
        {
            return objectives;
        }

        /// <summary>
        /// _type = 0 is SHOP
        /// _type = 1 is FOREST
        /// _type = 2 is MINE
        /// </summary>
        /// <param name="_type"></param>
        /// <returns></returns>
        public Position[] getLocations()
        {
            return importantLocations;
        }
    }

    public class Teams
    {

        public Team[] Forces { get; set; }
        public int Cash { get; set; }
        public int Iron { get; set; }
        public int Wood { get; set; }

        private bool[] structOwned;

        public Teams()
        {
            Forces = new Team[Stats.numberOfTeams];
            Cash = 0;
            Iron = 0;
            Wood = 0;
            structOwned = new bool[3];

            for (int i = 0; i < Stats.numberOfTeams; i++)
            {
                Forces[i] = new Team(false);
                Forces[i].setTeamName("PlayerTeam" + i + "");
            }
        }
        public Teams(string dummy)
        {
            Cash = 0;
            Iron = 0;
            Wood = 0;
        }


        public void setStructOwned(int index)
        {
            if (index < structOwned.Length)
            {
                structOwned[index] = true;
            }
        }

        public bool getStructOwned(int index)
        {
            return index < structOwned.Length ? structOwned[index] : false;
        }
    }
}
