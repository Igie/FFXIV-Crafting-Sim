using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSimLib.Solving
{
    public class CraftingActionSequence
    {
        public List<PossibleCraftingAction> PossibleIds { get; private set; }
        public CraftingActionSequence(IEnumerable<PossibleCraftingAction> possibleIds = null)
        {
            PossibleIds = new List<PossibleCraftingAction>();
            if (possibleIds != null)
                PossibleIds.AddRange(possibleIds);
        }

        public int GetPossibilities()
        {
            if (PossibleIds.Count == 0)
                return 0;
            int result = PossibleIds[0].Ids.Count;

            for (int i =1; i < PossibleIds.Count; i++)
            {
                result *= PossibleIds[i].Ids.Count;
            }

            return result;
        }

        public void AddPossibleAction(PossibleCraftingAction action)
        {
            PossibleIds.Add(action);
        }
    }
}
