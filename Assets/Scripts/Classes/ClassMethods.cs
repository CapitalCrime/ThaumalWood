using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassMethods
{

    public static class ClassExtensions
    {
        public static AttackMove[] GetStartingMoves(this Class job, int num)
        {
            AttackMove[] attackRoster = new AttackMove[num];
            attackRoster[0] = Move.Pass.GetAttackMove();
            switch (job)
            {
                case Class.None:
                    break;
                case Class.Knight:
                    attackRoster[1] = Move.Slice.GetAttackMove();
                    attackRoster[2] = Move.GetOverHere.GetAttackMove();
                    attackRoster[3] = Move.DefenceStance.GetAttackMove();
                    attackRoster[4] = Move.FieldAid.GetAttackMove();
                    break;
                case Class.Archer:
                    attackRoster[1] = Move.Volley.GetAttackMove();
                    attackRoster[2] = Move.LongShot.GetAttackMove();
                    attackRoster[3] = Move.Force.GetAttackMove();
                    break;
                case Class.Priest:
                    attackRoster[1] = Move.Heal.GetAttackMove();
                    attackRoster[2] = Move.Fireball.GetAttackMove();
                    attackRoster[3] = Move.CureWounds.GetAttackMove();
                    break;
                case Class.Thief:
                    attackRoster[1] = Move.Dice.GetAttackMove();
                    attackRoster[2] = Move.PoisonFlask.GetAttackMove();
                    break;
                case Class.Bartender:
                    attackRoster[1] = Move.Hook.GetAttackMove();
                    attackRoster[2] = Move.OnTheHouse.GetAttackMove();
                    attackRoster[3] = Move.FieldAid.GetAttackMove();
                    break;
            }
            return attackRoster;
        }
        public static AttackMove GetAttackMove(this Move attackName)
        {
            switch (attackName)
            {
                case Move.Pass:
                    return new AttackMove("Pass", 0, 0, new bool[] { false, false, false, false }, "Pass the turn.", true);
                case Move.Slice:
                    return new AttackMove("Slice", 13, 0, new bool[] { true, true, false, false }, "Just your good ol' slice.", true);
                case Move.Whirlwind:
                    return new AttackMove("Whirlwind", 6, 0, new bool[] { true, true, true, false }, "Spin to win.", true);
                case Move.Dice:
                    return new AttackMove("Dice", 4, 0, new bool[] { false, true, false, false }, "It slices, it dices, the whole shebang. Can also poison too.", true, Special.Poison, 1);
                case Move.Volley:
                    return new AttackMove("Volley", 7, 0, new bool[] { false, true, true, false }, "You fire two arrows at once, resulting in a large spread with medium damage.", true);
                case Move.LongShot:
                    return new AttackMove("Long Shot", 12, 3, new bool[] { false, false, true, false }, "You draw back your bow with full force, firing a single arrow with high precision and range.", true);
                case Move.Heal:
                    return new AttackMove("Heal", 8, 3, new bool[] { false, false, true, true }, "Surround your teammates with a lesser healing aura.", false);
                case Move.MassHeal:
                    return new AttackMove("Mass Heal", 6, 3, new bool[] { true, true, true, true }, "You collect energy from the forest surrounding you, healing your team in a large spread.", false);
                case Move.OnTheHouse:
                    return new AttackMove("House special", 0, 3, new bool[] { true, true, true, true }, "Give a round of ale to your team. Although it tastes bad, enraging them and boosting their attack.", false, Special.AttackBuff, 25);
                case Move.KillMove:
                    return new AttackMove("KillMove", 2000, 0, new bool[] { true, true, true, true }, "You shouldn't have this. I did something really wrong.", true);
                case Move.GetOverHere:
                    return new AttackMove("Get over here!", 0, 0, new bool[] { false, false, true, true}, "You swear the line sounds familiar, but don't know where it's from. Pulls opponents in two spots.", true, Special.Pull, 2);
                case Move.Hook:
                    return new AttackMove("Hooked!", 4, 0, new bool[] { false, false, true, true }, "Reel 'em in! Pulls opponents in two spots.", true, Special.Pull, 2);
                case Move.Force:
                    return new AttackMove("Force", 0, 0, new bool[] { true, true, false, false }, "Pushes opponents away two spots if they're getting too close for comfort.", true, Special.Pull, -2);
                case Move.PoisonFlask:
                    return new AttackMove("Poison Flask", 0, 3, new bool[] { false, false, true, false }, "Throw a vial of poison at a single foe.", true, Special.Poison, 2);
                case Move.PoisonCloud:
                    return new AttackMove("Poison Cloud", 2, 1, new bool[] { false, false, true, true }, "A cloud of smog envelops the battlefield, poisoning anyone in its vicinity.", true, Special.Poison, 2);
                case Move.FieldAid:
                    return new AttackMove("Field Aid", 3, 3, new bool[] { false, false, true, true }, "Patch up minor wounds. Will also cure poinsoning.", false, Special.Clean, 0);
                case Move.CureWounds:
                    return new AttackMove("Cure Wounds", 5, 3, new bool[] { false, false, true, true }, "A weak healing spell. Will also cure poisoning.", false, Special.Clean, 0);
                case Move.DefenceStance:
                    return new AttackMove("Defensive Stance", 0, 3, new bool[] { false, false, false, true }, "Bulk yourself up for an impending attack.", false, Special.DefenseBuff, 75);
                case Move.Fireball:
                    return new AttackMove("Fireball", 5, 0, new bool[] { false, false, true, true }, "Goodness gracious!", true);
                case Move.BearTraps:
                    return new AttackMove("Bear traps", 4, 0, new bool[] { true, true, true, true }, "Ensnare your opponents in a bear trap, causing damage and impeding their movement.", true, Special.Pull, 0);
            }
            return null;
        }

        public static int[] GetStats(this Class _class)
        {
            // ORDER IS: HEALTH, SPEED
            switch (_class)
            {
                case Class.None:
                    return new int[] { 0, 0 };
                case Class.Knight:
                    return new int[] { 60, 20 };
                case Class.Archer:
                    return new int[] { 50, 17 };
                case Class.Priest:
                    return new int[] { 45, 17 };
                case Class.Thief:
                    return new int[] { 40, 15 };
                case Class.Bartender:
                    return new int[] { 40, 15 };
            }
            return new int[] { 0,0 };
        }

        public static int GetPrice(this Class _class)
        {
            switch (_class)
            {
                case Class.None:
                    return 0;
                case Class.Knight:
                    return 150;
                case Class.Archer:
                    return 175;
                case Class.Priest:
                    return 150;
                case Class.Thief:
                    return 125;
                case Class.Bartender:
                    return 100;
            }
            return 0;
        }
    }

    public enum Class
    {
        None,
        Knight,
        Archer,
        Priest,
        Thief,
        Bartender
    }

    public enum Move
    {
        Pass,
        Slice,
        Whirlwind,
        Dice,
        Volley,
        LongShot,
        Heal,
        MassHeal,
        OnTheHouse,
        KillMove,
        GetOverHere,
        Force,
        PoisonFlask,
        FieldAid,
        DefenceStance,
        PoisonCloud,
        Fireball,
        CureWounds,
        Hook,
        BearTraps
    }

    public enum Special
    {
        None, 
        AttackBuff,
        DefenseBuff,
        Pull,
        Poison,
        Clean
    }

    public class AttackMove
    {
        string name;
        int power;
        int position;
        bool[] areaEffect;
        bool offensive;
        string description;
        Special special;
        int specialPower;

        public AttackMove(string name, int power, int position, bool[] areaEffect, string description, bool offensive, Special special, int specialPower)
        {
            this.name = name;
            this.power = power;
            this.position = position;
            this.areaEffect = areaEffect;
            this.offensive = offensive;
            this.description = description;
            this.special = special;
            this.specialPower = specialPower;
        }

        public AttackMove() : this("", 0, 0, new bool[] { false, false, false, false}, "", false, Special.None, 0)
        {}

        public AttackMove(string name, int power, int position, bool[] areaEffect, string description, bool offensive) : this(name, power, position, areaEffect, description, offensive, Special.None, 0)
        {}

        public string getName()
        {
            return name;
        }

        public int getPower()
        {
            return power;
        }

        public int getSpecialPower()
        {
            return specialPower;
        }

        public int getPosition()
        {
            return position;
        }
        public bool[] getSpaces()
        {
            return areaEffect;
        }

        public int numberSpaces()
        {
            int spaces = 0;
            for (int i = 0; i < 4; i++)
            {
                if (areaEffect[i] == true)
                {
                    spaces++;
                }
            }
            return spaces;
        }

        public bool isOffensive()
        {
            return offensive;
        }

        public string getDescription()
        {
            return description;
        }

        public Special getSpecial()
        {
            return special;
        }
    }
}
