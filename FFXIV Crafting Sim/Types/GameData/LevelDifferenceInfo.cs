using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIV_Crafting_Sim.Types.GameData
{
    public class LevelDifferenceInfo
    {
        public short Difference { get; set; }
        public short ProgressFactor { get; set; }
        public short QualityFactor { get; set; }
    }
}
