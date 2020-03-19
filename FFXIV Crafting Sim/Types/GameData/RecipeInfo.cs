using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIV_Crafting_Sim.Types.GameData
{
    public enum ClassJobInfo
    {
        CRP = 8,
        BLM = 9,
        ARM = 10,
        GSM = 11,
        LTW = 12,
        WVR = 13,
        ALC = 14,
        CUL = 15
    }
    public class RecipeInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ClassJobInfo ClassJob { get; set; }
        public int Level { get; set; }
        public int RequiredCraftsmanship { get; set; }
        public int RequiredControl { get; set; }

        public int Durability { get; set; }
        public int MaxProgress { get; set; }
        public int MaxQuality { get; set; }

        public string SearchString
        {
            get
            {
                return $"{Id} {Name} {Level}";
            }
        }
    }
}
