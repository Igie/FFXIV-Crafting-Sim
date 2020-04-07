using FFXIVCraftingSim.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Types
{
    public class RotationInfo : IEquatable<RotationInfo>
    {
        public int MaxCraftsmanship { get; set; }
        public int MinCraftsmanship { get; set; }
        public int MinControl { get; set; }
        public int CP { get; set; }
        public double Score { get; set; }

        public int RotationTime
        {
            get
            {
                if (Rotation == null)
                    return 0;
                int result = 0;
                for (int i = 0; i < Rotation.Array.Length; i++)
                    result += CraftingAction.CraftingActions[Rotation.Array[i]].IsBuff ? 2 : 3;
                return result;
            }
        }

        public ExtendedArray<ushort> Rotation { get; set; }



        public override bool Equals(object obj)
        {
            return base.Equals(obj as RotationInfo);
        }

        public bool IsBetterThanOrEqual(RotationInfo other)
        {
            return MaxCraftsmanship >= other.MaxCraftsmanship &&
                MinCraftsmanship <= other.MinCraftsmanship &&
                MinControl <= other.MinControl &&
                CP <= other.CP &&
                Score >= other.Score &&
                RotationTime <= other.RotationTime;
        }

        public bool IsBetterThan(RotationInfo other)
        {
            return IsBetterThanOrEqual(other) && !Equals(other);
        }

        public override int GetHashCode()
        {
            int hash = 7;
            hash ^= MaxCraftsmanship;
            hash ^= MinCraftsmanship * 3;
            hash ^= MinControl * 13;
            hash ^= CP * 7;
            hash ^= Rotation.GetHashCode() * 29;
            return hash;
        }

        public bool Equals(RotationInfo other)
        {
            if (other is null)
                return false;
            return MaxCraftsmanship == other.MaxCraftsmanship &&
                MinCraftsmanship == other.MinCraftsmanship &&
                MinControl == other.MinControl &&
                CP == other.CP &&
                Score == other.Score &&
                Rotation.Equals(other.Rotation);
        }

        public static RotationInfo FromSim(CraftingSim sim)
        {
            if (sim == null ||
                sim.CurrentProgress < sim.CurrentRecipe.MaxProgress ||
                sim.CurrentQuality < sim.CurrentRecipe.MaxQuality)
                return null;
            RotationInfo result = new RotationInfo();
            CraftingSim s = sim.Clone();
            var actions = sim.GetCraftingActions();
            s.AddActions(actions);
            result.CP = s.MaxCP - s.CurrentCP;
            result.Score = s.Score;
            result.Rotation = actions.Select(x => (ushort)x.Id).ToArray();

            int recipeProgress = s.CurrentRecipe.MaxProgress;
            int recipeQuality = s.CurrentRecipe.MaxQuality;

            int oldCraftsmanshipBuff = s.CraftsmanshipBuff;
            int oldControlBuff = s.ControlBuff;
            while (s.CurrentProgress >= recipeProgress)
            {
                s.CraftsmanshipBuff--;
                s.ExecuteActions();
            }
            s.CraftsmanshipBuff++;
            s.RemoveActions();
            s.AddActions(actions);
            result.MinCraftsmanship = s.Craftsmanship;

            while (s.CurrentQuality >= recipeQuality)
            {
                s.ControlBuff--;
                s.ExecuteActions();
            }
            result.MinControl = s.Control + 1;
            s.CraftsmanshipBuff = oldCraftsmanshipBuff;
            s.ControlBuff = oldControlBuff;

            int oldActionsLength = actions.Length;
            int newActionsLength = oldActionsLength;
            s.RemoveActions();
            s.AddActions(actions);

            while(newActionsLength >= oldActionsLength)
            {

                s.CraftsmanshipBuff++;
                s.ExecuteActions();
                newActionsLength = s.CraftingActionsLength;
                if (s.Craftsmanship > 10000)
                    break;
            }
            result.MaxCraftsmanship = s.Craftsmanship - 1;
            return result;
        }
    }
}
