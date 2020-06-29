using FFXIVCraftingSimLib.Actions;
using FFXIVCraftingSimLib.Types;
using FFXIVCraftingSimLib.Types.GameData;
using Microsoft.Win32;
using SaintCoinach.Xiv;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSimLib
{
    public static class Utils
    {
        public static Dictionary<ExtendedArray<ushort>, CraftingSim> CraftingStates { get; private set; } = new Dictionary<ExtendedArray<ushort>, CraftingSim>();

        public static int GetPlayerLevel(int level)
        {
            switch (level)
            {
                case 51: return 120;
                case 52: return 125;
                case 53: return 130;
                case 54: return 133;
                case 55: return 136;
                case 56: return 139;
                case 57: return 142;
                case 58: return 145;
                case 59: return 148;
                case 60: return 150;
                case 61: return 260;
                case 62: return 265;
                case 63: return 270;
                case 64: return 273;
                case 65: return 276;
                case 66: return 279;
                case 67: return 282;
                case 68: return 285;
                case 69: return 288;
                case 70: return 290;
                case 71: return 390;
                case 72: return 395;
                case 73: return 400;
                case 74: return 403;
                case 75: return 406;
                case 76: return 409;
                case 77: return 412;
                case 78: return 415;
                case 79: return 418;
                case 80: return 420;
            }

            return level;
        }

        public static LevelDifferenceInfo GetCraftingLevelDifference(int levelDifference)
        {
            if (levelDifference <= GameData.LevelDifferences[0].Difference)
                return GameData.LevelDifferences[0];
            if (levelDifference >= GameData.LevelDifferences[GameData.LevelDifferences.Count - 1].Difference)
                return GameData.LevelDifferences[GameData.LevelDifferences.Count - 1];
            for (int i = 1; i < GameData.LevelDifferences.Count - 2; i++)
                if (GameData.LevelDifferences[i].Difference == levelDifference)
                    return GameData.LevelDifferences[i];

            throw new Exception();
        }

        public static void AddRotationFromSim(CraftingSim sim)
        {
            if (sim == null || sim.CurrentRecipe == null || sim.CustomRecipe)
                return;
            if (sim.CurrentProgress < sim.CurrentRecipe.MaxProgress || sim.CurrentQuality < sim.CurrentRecipe.MaxQuality)
                return;
            CraftingSim s = sim.Clone();
            s.AddActions(true, sim.GetCraftingActions());

            var abstractData = s.CurrentRecipe.GetAbstractData();
            if (!GameData.RecipeRotations.ContainsKey(abstractData))
            {
                Debugger.Break();
                return;
            }

            RecipeSolutionInfo infoWithMinLevel = RecipeSolutionInfo.FromSim(s, true);
            RecipeSolutionInfo infoWithoutMinLevel = RecipeSolutionInfo.FromSim(s, false);

            var list = GameData.RecipeRotations[abstractData];

            if (!list.Contains(infoWithMinLevel) && !list.Any(x => x.IsBetterThan(infoWithMinLevel)))
                list.Add(infoWithMinLevel);

            for (int i = 0; i < list.Count; i++)
            {
                if (infoWithMinLevel.IsBetterThan(list[i]))
                {
                    list.RemoveAt(i);
                    i--;
                }
            }

            if (!list.Contains(infoWithoutMinLevel) && !list.Any(x => x.IsBetterThan(infoWithoutMinLevel)))
                list.Add(infoWithoutMinLevel);

            for (int i = 0; i < list.Count; i++)
            {
                if (infoWithoutMinLevel.IsBetterThan(list[i]))
                {
                    list.RemoveAt(i);
                    i--;
                }
            }
        }

        public static int GetClassActionId(int classJobKey, int id)
        {
            return CraftingAction.CraftingActionIds[id][classJobKey];
        }
    }
}
