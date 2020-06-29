using FFXIVCraftingSim.Stream;
using FFXIVCraftingSimLib.Actions;
using FFXIVCraftingSimLib.Types;
using FFXIVCraftingSimLib.Types.GameData;
using SaintCoinach;
using SaintCoinach.Xiv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSimLib
{
    public static class GameData
    {
        public static ARealmReversed Game { get; private set; }

        public static List<LevelDifferenceInfo> LevelDifferences { get; set; }
        public static Dictionary<AbstractRecipeInfo, List<RecipeSolutionInfo>> RecipeRotations { get; private set; }

        private static Task InitTask { get;  set; }
        private static Task ReloadTask { get; set; }

        public static event System.Action Initialized = delegate { };
        public static event System.Action Reloaded = delegate { };

        public static void Init(string gameFilesPath)
        {
            Game = new ARealmReversed(gameFilesPath, SaintCoinach.Ex.Language.English);
            if (InitTask == null || InitTask.IsCompleted)
                InitTask = Task.Run(() =>
                {
                    ReadLevelDifferences();
                    ReadRecipeRotations();
                    ReadCraftingActionData();
                    Initialized();
                });
        }

        public static void Reload()
        {
            if (ReloadTask == null || ReloadTask.IsCompleted)
            {
                ReloadTask = Task.Run(() =>
                {
                    if (!InitTask.IsCompleted)
                        InitTask.Wait();
                    ReadLevelDifferences(true);
                    ReadRecipeRotations(true);
                    ReadCraftingActionData();
                    Reloaded();
                });
            }
        }
    

        public static void ReadLevelDifferences(bool deleteCurrent = false)
        {
            if (deleteCurrent && File.Exists("LevelDifferences.db"))
                File.Delete("LevelDifferences.db");

            if (File.Exists("LevelDifferences.db"))
            {
                DataStream s = new DataStream(File.ReadAllBytes("LevelDifferences.db"));
                int length = s.ReadS32();
                LevelDifferences = new List<LevelDifferenceInfo>(length);
                for (ushort i = 0; i < length; i++)
                {
                    LevelDifferenceInfo info = new LevelDifferenceInfo
                    {
                        Difference = s.ReadS32(),
                        ProgressFactor = s.ReadS32(),
                        QualityFactor = s.ReadS32()
                    };
                    LevelDifferences.Add(info);
                }
                s.Flush();
                s.Close();
            }
            else
            {
                var sheet = Game.GameData.GetSheet<CraftLevelDifference>();
                int count = sheet.Count();
                int[] keys = sheet.Keys.ToArray();
                LevelDifferences = new List<LevelDifferenceInfo>(count);
                for (int i = 0; i < count; i++)
                {
                    var value = sheet[keys[i]];
                    LevelDifferenceInfo info = new LevelDifferenceInfo
                    {
                        Difference = (short)value.Difference,
                        ProgressFactor = (short)value.ProgressFactor,
                        QualityFactor = (short)value.QualityFactor
                    };

                    LevelDifferences.Add(info);
                }
                WriteLevelDifferences();
            }
        }

        private static void WriteLevelDifferences()
        {
            DataStream s = new DataStream();
            s.WriteS32(LevelDifferences.Count);
            for (int i = 0; i < LevelDifferences.Count; i++)
            {
                var value = LevelDifferences[i];
                s.WriteS32(value.Difference);
                s.WriteS32(value.ProgressFactor);
                s.WriteS32(value.QualityFactor);
            }

            File.WriteAllBytes("LevelDifferences.db", s.GetBytes());
            s.Flush();
            s.Close();
        }

        public static void ReadRecipeRotations(bool deleteCurrent = false)
        {
            if (deleteCurrent && File.Exists("RecipeRotations.db"))
                File.Delete("RecipeRotations.db");

            if (File.Exists("RecipeRotations.db"))
            {
                DataStream s = new DataStream(File.ReadAllBytes("RecipeRotations.db"));
                int length = s.ReadS32();
                RecipeRotations = new Dictionary<AbstractRecipeInfo, List<RecipeSolutionInfo>>(length);
                for (ushort i = 0; i < length; i++)
                {
                    AbstractRecipeInfo info = new AbstractRecipeInfo
                    {
                        Level = s.ReadS32(),
                        RequiredCraftsmanship = s.ReadS32(),
                        RequiredControl = s.ReadS32(),
                        Durability = s.ReadS32(),
                        MaxProgress = s.ReadS32(),
                        MaxQuality = s.ReadS32()
                    };
                    var ll = s.ReadS32();
                    RecipeRotations[info] = new List<RecipeSolutionInfo>(ll);
                    for (int j = 0; j < ll; j++)
                    {
                        RecipeSolutionInfo rotation = new RecipeSolutionInfo();
                        rotation.MinLevel = s.ReadS32();
                        rotation.MaxCraftsmanship = s.ReadS32();
                        rotation.MinCraftsmanship = s.ReadS32();
                        rotation.MinControl = s.ReadS32();
                        rotation.CP = s.ReadS32();
                        int l = s.ReadS32();
                        ushort[] array = new ushort[l];
                        for (int k = 0; k < l; k++)
                            array[k] = (ushort)s.ReadU30();
                        rotation.Rotation = array;
                        RecipeRotations[info].Add(rotation);
                    }
                }
                s.Flush();
                s.Close();
            }
            else
            {
                RecipeRotations = new Dictionary<AbstractRecipeInfo, List<RecipeSolutionInfo>>();
                var sheet = Game.GameData.GetSheet<Recipe>();
                int count = sheet.Count;
                int[] keys = sheet.Keys.ToArray();
                for (int i = 0; i < count; i++)
                {
                    var value = sheet[keys[i]];


                    AbstractRecipeInfo abstractInfo = AbstractRecipeInfo.GetAbstractData(value);
                    if (!RecipeRotations.ContainsKey(abstractInfo))

                        RecipeRotations[abstractInfo] = new List<RecipeSolutionInfo>();
                }
                WriteRecipeRotations();
            }
        }



        public static void WriteRecipeRotations()
        {
            DataStream s = new DataStream();

            var keys = RecipeRotations.Keys.ToArray();
            s.WriteS32(keys.Length);
            for (int i = 0; i < keys.Length; i++)
            {
                var value = keys[i];
                s.WriteS32(value.Level);
                s.WriteS32(value.RequiredCraftsmanship);
                s.WriteS32(value.RequiredControl);
                s.WriteS32(value.Durability);
                s.WriteS32(value.MaxProgress);
                s.WriteS32(value.MaxQuality);

                var rotations = RecipeRotations[value];
                s.WriteS32(rotations.Count);
                for (int j = 0; j < rotations.Count; j++)
                {
                    s.WriteS32(rotations[j].MinLevel);
                    s.WriteS32(rotations[j].MaxCraftsmanship);
                    s.WriteS32(rotations[j].MinCraftsmanship);
                    s.WriteS32(rotations[j].MinControl);
                    s.WriteS32(rotations[j].CP);
                    s.WriteS32(rotations[j].Rotation.Array.Length);
                    for (int k = 0; k < rotations[j].Rotation.Array.Length; k++)
                        s.WriteU30(rotations[j].Rotation.Array[k]);
                }
            }

            File.WriteAllBytes("RecipeRotations.db", s.GetBytes());
            s.Flush();
            s.Close();
        }

        public static void ReadCraftingActionData()
        {
            var sheet = Game.GameData.GetSheet<SaintCoinach.Xiv.Action>();
            int count = sheet.Count();
            int[] keys = sheet.Keys.ToArray();

            var actions = CraftingAction.CraftingActions.Values.ToArray();

            for (int i = 0; i < count; i++)
            {
                int key = keys[i];
                var value = sheet[key];
                string name = value.Name;
                
                if (value.ActionCategory.Key == 7 && value.ClassJob != null)
                {
                    var action = actions.FirstOrDefault(x => x.Name == name);
                    if (action != null && !CraftingAction.CraftingActionIds[action.Id].ContainsKey(value.ClassJob.Key))
                    {
                        CraftingAction.CraftingActionIds[action.Id][value.ClassJob.Key] = value.Key;
                    }
                }
            }

            var otherSheet = Game.GameData.GetSheet<CraftAction>();
           
            count = otherSheet.Count();
            keys = otherSheet.Keys.ToArray();
            for (int i = 0; i < count; i++)
            {
                int key = keys[i];
                var value = otherSheet[key];
                string name = value.Name;
                if (value.ClassJob != null && !string.IsNullOrEmpty(name))
                {
                    var action = actions.FirstOrDefault(x => x.Name == name);
                    if (action != null && !CraftingAction.CraftingActionIds[action.Id].ContainsKey(value.ClassJob.Key))
                    {
                        CraftingAction.CraftingActionIds[action.Id][value.ClassJob.Key] = value.Key;
                    }
                }
            }
        }
    }
}
