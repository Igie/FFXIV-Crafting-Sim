using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FFXIVCraftingSim.Types.GameData
{
    public class ItemInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public CrafterFoodInfo FoodInfo { get; set; }

        public string SearchString
        {
            get
            {
                string foodInfo = "";
                if (FoodInfo != null)
                {
                    foodInfo = " " + string.Join(" ", FoodInfo.StatTypes);
                }
                return $"{Name}{foodInfo}";
            }
        }
    }

    public enum FoodType : byte
    {
        Food = 1,
        Tea = 2
    }
    public enum StatType : byte
    {
        Craftsmanship = 1,
        Control = 2,
        CP = 3
    }
    public class CrafterFoodInfo
    {
        public FoodType FoodType { get; set; }
        public StatType[] StatTypes { get; private set; }
        public int[] PercentageIncrease { get; private set; }
        public int[] MaxIncrease { get; private set; }
        public int[] PercentageIncreaseHQ { get; private set; }
        public int[] MaxIncreaseHQ { get; private set; }

        public string Craftsmanship
        {
            get
            {
                for (int i = 0; i < StatTypes.Length; i++)
                {
                    if (StatTypes[i] == StatType.Craftsmanship)
                    {
                        return $"NQ +{PercentageIncrease[i]}%(Max {MaxIncrease[i]}), HQ +{PercentageIncreaseHQ[i]}%(Max {MaxIncreaseHQ[i]})";
                    }
                }
                return "";
            }
        }

        public string Control
        {
            get
            {
                for (int i = 0; i < StatTypes.Length; i++)
                {
                    if (StatTypes[i] == StatType.Control)
                    {
                        return $"NQ +{PercentageIncrease[i]}%(Max {MaxIncrease[i]}), HQ +{PercentageIncreaseHQ[i]}%(Max {MaxIncreaseHQ[i]})";
                    }
                }
                return "";
            }
        }

        public string CP
        {
            get
            {
                for (int i = 0; i < StatTypes.Length; i++)
                {
                    if (StatTypes[i] == StatType.CP)
                    {
                        return $"NQ +{PercentageIncrease[i]}%(Max {MaxIncrease[i]}), HQ +{PercentageIncreaseHQ[i]}%(Max {MaxIncreaseHQ[i]})";
                    }
                }
                return "";
            }
        }

        private int GetStatIncrease(StatType type, int current, bool hq)
        {
            for (int i = 0; i < StatTypes.Length; i++)
            {
                if (StatTypes[i] == type)
                {
                    if (hq)
                        return Math.Min(MaxIncreaseHQ[i], (int)((double)current * PercentageIncreaseHQ[i] / 100));
                    else
                        return Math.Min(MaxIncrease[i], (int)((double)current * PercentageIncrease[i] / 100));
                }
            }
            return 0;
        }

        public int GetCraftsmanshipBuff(int current, bool hq)
        {
            return GetStatIncrease(StatType.Craftsmanship, current, hq);
        }

        public int GetControlBuff(int current, bool hq)
        {
            return GetStatIncrease(StatType.Control, current, hq);
        }

        public int GetMaxCPBuff(int current, bool hq)
        {
            return GetStatIncrease(StatType.CP, current, hq);
        }

        public CrafterFoodInfo(int count)
        {
            StatTypes = new StatType[count];
            PercentageIncrease = new int[count];
            MaxIncrease = new int[count];
            PercentageIncreaseHQ = new int[count];
            MaxIncreaseHQ = new int[count];
        }
    }
}
