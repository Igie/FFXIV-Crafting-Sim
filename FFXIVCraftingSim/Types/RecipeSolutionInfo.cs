using FFXIVCraftingSim.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Types
{
    public class RecipeSolutionInfo : IEquatable<RecipeSolutionInfo>
    {
        public int MinLevel { get; set; }
        public int MaxCraftsmanship { get; set; }
        public int MinCraftsmanship { get; set; }
        public int MinControl { get; set; }
        public int CP { get; set; }

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
            return base.Equals(obj as RecipeSolutionInfo);
        }

        public bool IsBetterThanOrEqual(RecipeSolutionInfo other)
        {
            return MinLevel <= other.MinLevel &&
                MaxCraftsmanship >= other.MaxCraftsmanship &&
                MinCraftsmanship <= other.MinCraftsmanship &&
                MinControl <= other.MinControl &&
                CP <= other.CP &&
                RotationTime <= other.RotationTime;
        }

        public bool IsBetterThan(RecipeSolutionInfo other)
        {
            return IsBetterThanOrEqual(other) && !Equals(other);
        }

        public override int GetHashCode()
        {
            int hash = 7;
            hash ^= MinLevel;
            hash ^= MaxCraftsmanship * 7;
            hash ^= MinCraftsmanship * 3;
            hash ^= MinControl * 13;
            hash ^= CP * 7;
            hash ^= Rotation.GetHashCode() * 29;
            return hash;
        }

        public bool Equals(RecipeSolutionInfo other)
        {
            if (other is null)
                return false;
            return MinLevel == other.MinLevel &&
                MaxCraftsmanship == other.MaxCraftsmanship &&
                MinCraftsmanship == other.MinCraftsmanship &&
                MinControl == other.MinControl &&
                CP == other.CP &&
                Rotation.Equals(other.Rotation);
        }

        public static RecipeSolutionInfo FromSim(CraftingSim sim, bool findMinLevel)
        {
            if (sim == null ||
                sim.CurrentProgress < sim.CurrentRecipe.MaxProgress ||
                sim.CurrentQuality < sim.CurrentRecipe.MaxQuality)
                return null;
            RecipeSolutionInfo result = new RecipeSolutionInfo();
            CraftingSim s = sim.Clone();
            var actions = sim.GetCraftingActions();
            s.AddActions(true, actions);
            result.CP = s.MaxCP - s.CurrentCP;

            result.Rotation = actions.Select(x => (ushort)x.Id).ToArray();
            s.Level = s.CurrentRecipe.ClassJobLevel;
            s.Level = sim.Level;
            if (findMinLevel)
            {
                int minLevelFromActionsLevel = actions.Max(x => x.Level);
                int minLevelFromSuccess = s.Level;

                bool craftFailed = false;

                while (!craftFailed)
                {
                    s.RemoveActions();
                    minLevelFromSuccess--;
                    s.Level = minLevelFromSuccess;
                    s.AddActions(true, actions);
                    craftFailed = s.CurrentProgress < s.CurrentRecipe.MaxProgress || s.CurrentQuality < s.CurrentRecipe.MaxQuality;
                }

                minLevelFromSuccess++;

                result.MinLevel = Math.Max(Math.Max(minLevelFromSuccess, minLevelFromActionsLevel), s.CurrentRecipe.ClassJobLevel);
            }
            else
                result.MinLevel = sim.Level;

            s.Level = result.MinLevel;

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
            s.AddActions(true, actions);
            result.MinCraftsmanship = Math.Max(s.Craftsmanship, sim.CurrentRecipe.RequiredCraftsmanship);


            while (s.CurrentQuality >= recipeQuality)
            {
                s.ControlBuff--;
                s.ExecuteActions();
            }
            result.MinControl = Math.Max(s.Control + 1, sim.CurrentRecipe.RequiredControl);
            s.CraftsmanshipBuff = oldCraftsmanshipBuff;
            s.ControlBuff = oldControlBuff;

            int oldActionsLength = actions.Length;
            int newActionsLength = oldActionsLength;
            s.RemoveActions();
            s.AddActions(true, actions);

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
