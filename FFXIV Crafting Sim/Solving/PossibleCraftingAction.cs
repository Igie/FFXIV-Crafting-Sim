using FFXIV_Crafting_Sim.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIV_Crafting_Sim.Solving
{
    public class PossibleCraftingAction : IEquatable<PossibleCraftingAction>
    {
        public List<int> Ids { get; private set; }

        public PossibleCraftingAction(IEnumerable<int> ids = null)
        {
            Ids = new List<int>();
            if (ids != null)
                Ids.AddRange(ids);
        }

        public override int GetHashCode()
        {
            int result = 7;

            Ids.ForEach(x => {
                result = unchecked(result + x.GetHashCode());
            });

            return result;
        }

        public override bool Equals(object obj)
        {
            
            if (obj == null)
                return false;
            return Equals(obj as PossibleCraftingAction);
            
        }

        public bool Equals(PossibleCraftingAction other)
        {
            if (other == null)
                return false;
            if (GetHashCode() != other.GetHashCode())
                return false;
            return true;
        }

        public static bool operator ==(PossibleCraftingAction left, PossibleCraftingAction right)
        {
            if (left == null && right != null)
                return false;

            if (left != null && right == null)
                return false;
            if (left == null && right == null)
                return true;
            return (left.Equals(right));
        }

        public static bool operator !=(PossibleCraftingAction left, PossibleCraftingAction right)
        {
            if (left == null && right != null)
                return true;

            if (left != null && right == null)
                return true;
            if (left == null && right == null)
                return false;
            return (!left.Equals(right));
        }

        public static implicit operator PossibleCraftingAction(CraftingAction[] actions)
        {
            PossibleCraftingAction result = new PossibleCraftingAction(actions.Select(x => x.Id));
            return result;
        }

        public static implicit operator PossibleCraftingAction(int[] actions)
        {
            PossibleCraftingAction result = new PossibleCraftingAction(actions);
            return result;
        }
    }
}
