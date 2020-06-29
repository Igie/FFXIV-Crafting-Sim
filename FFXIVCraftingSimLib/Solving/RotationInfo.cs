using FFXIVCraftingSimLib.Actions;
using FFXIVCraftingSimLib.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSimLib.Solving
{
    public class RotationInfo
    {
        public ExtendedArray<ushort> Rotation { get; private set; }

        public RotationInfo(params CraftingAction[] actions)
        {
            Rotation = actions.Select(x => x.Id).ToArray();
        }

        public RotationStats GetStats(CraftingSim sim)
        {
            CraftingSim copy = sim.Clone();
            sim.CopyTo(copy, true);
            int progress = sim.CurrentProgress;
            int quality = sim.CurrentQuality;
            int cp = sim.CurrentCP;
            int durability = sim.CurrentDurability;
            var actions = sim.GetCraftingActions().ToList();

            actions.AddRange(Rotation.Array.Select(x => CraftingAction.CraftingActions[x]));
            copy.AddActions(true, actions);

            return new RotationStats(
                Rotation.Array.Length,
                copy.CurrentProgress - progress,
                copy.CurrentQuality - quality,
                cp -copy.CurrentCP,
                durability - copy.CurrentDurability
                );
        }
    }

    public class RotationStats
    {
        public int Steps { get; private set; }

        public int ProgressIncrease { get; private set; }
        public int QualityIncrease { get; private set; }
        public int CPCost { get; private set; }
        public int DurabilityLoss { get; private set; }

        public double ProgressIncreasePerStep { get; private set; }
        public double QualityIncreasePerStep { get; private set; }
        public double CPCostPerStep { get; private set; }
        public double DurabilityLossPerStep { get; private set; }

        public RotationStats(int steps, int progressIncrease, int qualityIncrease, int cPCost, int durabilityLoss)
        {
            Steps = steps;
            ProgressIncrease = progressIncrease;
            QualityIncrease = qualityIncrease;
            CPCost = cPCost;
            DurabilityLoss = durabilityLoss;

            ProgressIncreasePerStep = progressIncrease / (double)steps;
            QualityIncreasePerStep = qualityIncrease / (double)steps;
            CPCostPerStep = cPCost / (double)steps;
            DurabilityLossPerStep = durabilityLoss / (double)steps;
        }
    }
}
