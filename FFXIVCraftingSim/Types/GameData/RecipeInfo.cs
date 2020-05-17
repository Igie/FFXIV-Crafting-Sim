using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Types.GameData
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

        public List<ItemInfo> Ingredients { get; set; }

        public string SearchString
        {
            get
            {
                return $"{Id} {Name} {Level}";
            }
        }

        public RecipeInfo()
        {
            Ingredients = new List<ItemInfo>(10);
        }

        public AbstractRecipeInfo GetAbstractData()
        {
            return new AbstractRecipeInfo
            {
                Level = Level,
                RequiredCraftsmanship = RequiredCraftsmanship,
                RequiredControl = RequiredControl,
                Durability = Durability,
                MaxProgress = MaxProgress,
                MaxQuality = MaxQuality
            };
        }
    }

    public class AbstractRecipeInfo :IEquatable<AbstractRecipeInfo>
    {
        public int Level { get; set; }
        public int RequiredCraftsmanship { get; set; }
        public int RequiredControl { get; set; }

        public int Durability { get; set; }
        public int MaxProgress { get; set; }
        public int MaxQuality { get; set; }

        public override int GetHashCode()
        {
            int hash = 3301;
            hash ^= Level;
            hash ^= RequiredCraftsmanship * 13;
            hash ^= RequiredControl * 13;
            hash ^= Durability * 13;
            hash ^= MaxProgress * 13;
            hash ^= MaxQuality * 13;
            return hash;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            return Equals(obj is AbstractRecipeInfo);
        }

        public bool Equals(AbstractRecipeInfo other)
        {
            if (other is null)
                return false;
            return
                Level == other.Level &&
                RequiredCraftsmanship == other.RequiredCraftsmanship &&
                RequiredControl == other.RequiredControl &&
                Durability == other.Durability &&
                MaxProgress == other.MaxProgress &&
                MaxQuality == other.MaxQuality;
        }
    }
}
