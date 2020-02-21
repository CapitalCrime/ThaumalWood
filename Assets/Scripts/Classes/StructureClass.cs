using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StructureMethods
{
    public static class StructureExtensions
    {
        public static int GetPrice(this Structure structure)
        {
            switch (structure)
            {
                case Structure.Market:
                    return 150;
                case Structure.Bank:
                    return 250;
                case Structure.Mine:
                    return 125;
                case Structure.Forestry:
                    return 100;
            }
            return 0;
        }
    }

    public enum Structure
    {
        Market,
        Bank,
        Mine,
        Forestry,
        Nothing
    }
}
