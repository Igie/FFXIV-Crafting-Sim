using FFXIVCraftingSim.Converters;
using FFXIVCraftingSim.Solving;
using FFXIVCraftingSim.Stream;
using FFXIVCraftingSim.Types;
using FFXIVCraftingSim.Types.GameData;
using SaintCoinach;
using SaintCoinach.Xiv;
using SaintCoinach.Xiv.ItemActions;
using SaintCoinach.Xiv.Items;
using SaintCoinach.Xiv.Sheets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim
{
    public static class G
    {
        public static MainWindow MainWindow { get; private set; }

        public static ARealmReversed Game { get; private set; }

        public static List<RecipeInfo> Recipes { get; private set; }

        public static Dictionary<AbstractRecipeInfo, List<RecipeSolutionInfo>> RecipeRotations { get; private set; }
        public static Dictionary<int, ItemInfo> Items { get; private set; }
        public static Dictionary<string, ActionInfo> Actions { get; private set; }

        public static List<ItemInfo> CrafterFood { get; private set; }

        public static List<LevelDifferenceInfo> LevelDifferences { get; private set; }
        public static Task InitTask { get; private set; }
        public static Task ReloadTask { get; private set; }

        public static event System.Action Loaded = delegate { };

        public static Dictionary<ExtendedArray<ushort>, CraftingSim> CraftingStates { get; private set; } = new Dictionary<ExtendedArray<ushort>, CraftingSim>();

        public static void Init(MainWindow window)
        {
            DataStream s = new DataStream();
            MainWindow = window;
            Game = new ARealmReversed(@"C:\Program Files (x86)\SquareEnix\FINAL FANTASY XIV - A Realm Reborn", SaintCoinach.Ex.Language.English);

            if (InitTask == null || InitTask.IsCompleted)
                InitTask = Task.Run(() =>
                {
                    ReadItems();
                    ReadRecipes();
                    
                    ReadActions();
                    ReadLevelDifferences();
                    ReadRecipeRotations();
                    Loaded();
                });
        }

        public static void ReloadDatabase()
        {
            if (ReloadTask == null || ReloadTask.IsCompleted)
            {
                ReloadTask = Task.Run(() =>
                    {
                        if (!InitTask.IsCompleted)
                            InitTask.Wait();
                        ReadItems(true);
                        ReadRecipes(true);
                        
                        ReadActions(true);
                        ReadLevelDifferences(true);
                        ReadRecipeRotations(true);
                        Loaded();
                    });
            }
        }

        private static void ReadRecipes(bool deleteCurrent = false)
        {
            if (deleteCurrent && File.Exists("Recipes.db"))
                File.Delete("Recipes.db");

            if (File.Exists("Recipes.db"))
            {
                SetStatus("Reading Recipes from Recipes.db...", 0);
                DataStream s = new DataStream(File.ReadAllBytes("Recipes.db"));
                int length = s.ReadS32();
                Recipes = new List<RecipeInfo>(length);
                SetStatus(null, 0, 0, length);
                for (ushort i = 0; i < length; i++)
                {
                    RecipeInfo info = new RecipeInfo
                    {
                        Id = s.ReadS32(),
                        Name = s.ReadString(),
                        Level = s.ReadS32(),
                        ClassJobLevel = s.ReadS32(),
                        ClassJob = (ClassJobInfo)s.ReadS32(),
                        RequiredCraftsmanship = s.ReadS32(),
                        RequiredControl = s.ReadS32(),
                        Durability = s.ReadS32(),
                        MaxProgress = s.ReadS32(),
                        MaxQuality = s.ReadS32()
                    };

                    int c = s.ReadByte();
                    for (int j = 0; j < c; j++)
                        info.Ingredients.Add(Items[s.ReadS32()]);

                    Recipes.Add(info);

                    if (i % 100 == 0)
                        SetStatus(null, i);
                }
                SetStatus(null, length);
                s.Flush();
                s.Close();
            }
            else
            {
                var sheet = Game.GameData.GetSheet<Recipe>();
                SetStatus("Reading Recipes from sheets...", 0, 0, sheet.Count());
                int count = sheet.Count();
                int[] keys = sheet.Keys.ToArray();
                Recipes = new List<RecipeInfo>(count);
                for (int i = 0; i < count; i++)
                {
                    var value = sheet[keys[i]];
                    if (!string.IsNullOrEmpty(value.ResultItem.Name))
                    {
                        RecipeLevelTable recipeLevelTable = value.RecipeLevelTable;
                        RecipeInfo info = new RecipeInfo
                        {
                            Id = value.Key,
                            Name = value.ResultItem.Name,
                            Level = recipeLevelTable.Key,
                            ClassJobLevel = recipeLevelTable.ClassJobLevel,
                            ClassJob = (ClassJobInfo)value.ClassJob.Key,
                            RequiredCraftsmanship = recipeLevelTable.SuggestedCraftsmanship,
                            RequiredControl = recipeLevelTable.SuggestedControl,
                            Durability = recipeLevelTable.Durability * value.DurabilityFactor / 100,
                            MaxProgress = recipeLevelTable.Difficulty * value.DifficultyFactor / 100,
                            MaxQuality = recipeLevelTable.Quality * value.QualityFactor / 100
                        };

                        try
                        {
                            info.Ingredients.AddRange(value.Ingredients.Select(x => Items[x.Item.Key]));
                        }
                        catch
                        {
                            Debugger.Break();
                        }

                        Recipes.Add(info);
                    }

                    if (i % 100 == 0)
                        SetStatus(null, i);
                }

                SetStatus(null, count);

                WriteRecipes();
            }
        }



        private static void WriteRecipes()
        {
            SetStatus("Writing Recipes to Recipes.db...", 0, 0, Recipes.Count);
            DataStream s = new DataStream();
            s.WriteS32(Recipes.Count);
            for (int i = 0; i < Recipes.Count; i++)
            {
                var value = Recipes[i];
                s.WriteS32(value.Id);
                s.WriteString(value.Name);
                s.WriteS32(value.Level);
                s.WriteS32(value.ClassJobLevel);
                s.WriteS32((int)value.ClassJob);
                s.WriteS32(value.RequiredCraftsmanship);
                s.WriteS32(value.RequiredControl);
                s.WriteS32(value.Durability);
                s.WriteS32(value.MaxProgress);
                s.WriteS32(value.MaxQuality);
                s.WriteByte((byte)value.Ingredients.Count);
                for (int j = 0; j < value.Ingredients.Count; j++)
                    s.WriteS32(value.Ingredients[j].Id);
                if (i % 100 == 0)
                    SetStatus(null, i);
            }
            SetStatus(null, Recipes.Count);

            File.WriteAllBytes("Recipes.db", s.GetBytes());
            s.Flush();
            s.Close();
        }

        private static void ReadItems(bool deleteCurrent = false)
        {
            if (deleteCurrent && File.Exists("Items.db"))
                File.Delete("Items.db");

            Items = new Dictionary<int, ItemInfo>();
            CrafterFood = new List<ItemInfo>();
            if (File.Exists("Items.db"))
            {
                SetStatus("Reading Items from Items.db...", 0);
                DataStream s = new DataStream(File.ReadAllBytes("Items.db"));
                int length = s.ReadS32();
                SetStatus(null, 0, 0, length);
                for (ushort i = 0; i < length; i++)
                {
                    ItemInfo info = new ItemInfo
                    {
                        Id = s.ReadS32(),
                        Name = s.ReadString(),
                    };

                    bool hasCrafterFoodInfo = s.ReadByte() == 1;
                    if (hasCrafterFoodInfo)
                    {
                        var f = new CrafterFoodInfo(s.ReadByte());
                        f.FoodType = (FoodType)s.ReadByte();

                        for (int j = 0; j < f.StatTypes.Length; j++)
                        {
                            f.StatTypes[j] = (StatType)s.ReadByte();
                            f.PercentageIncrease[j] = s.ReadS32();
                            f.MaxIncrease[j] = s.ReadS32();
                            f.PercentageIncreaseHQ[j] = s.ReadS32();
                            f.MaxIncreaseHQ[j] = s.ReadS32();
                        }

                        info.FoodInfo = f;
                        CrafterFood.Add(info);
                    }
                    Items[info.Id] = info;
                    if (i % 100 == 0)
                        SetStatus(null, i);
                }
                SetStatus(null, length);
                s.Flush();
                s.Close();
            }
            else
            {
                var sheet = Game.GameData.GetSheet<Item>();

                int count = sheet.Count();
                int[] keys = sheet.Keys.ToArray();
                for (int i = 0; i < count; i++)
                {
                    var value = sheet[keys[i]];
                    if (!string.IsNullOrEmpty(value.Name))
                    {
                        var item = Items[value.Key] = new ItemInfo
                        {
                            Id = value.Key,
                            Name = value.Name,
                        };
                        if (i % 100 == 0)
                            SetStatus(null, i);
                        Enhancement food = null; 

                        int type = 0;

                        if (value.ItemAction is Food)
                        {
                            //food
                            food = value.ItemAction as Enhancement;
                            type = 1;
                        } else if (value.ItemAction is Enhancement)
                        {
                            //enhancement
                            food = value.ItemAction as Enhancement;
                            type = 2;
                        }

                        if (type != 0)
                        {
                           
                            var args = food.ItemFood.Parameters.ToArray();

                            
                            
                            bool isCrafterFood = args.Any(x => x.BaseParam.Name == "Craftsmanship" || x.BaseParam.Name == "Control" || x.BaseParam.Name == "CP");
                            if (isCrafterFood)
                            {
                                args = args.Where(x => x.BaseParam.Name == "Craftsmanship" || x.BaseParam.Name == "Control" || x.BaseParam.Name == "CP").ToArray();
                                CrafterFoodInfo f = new CrafterFoodInfo(args.Length);
                                f.FoodType = (FoodType)type;

                                for (int j = 0; j < args.Length; j++)
                                {
                                    ParameterValueRelativeLimited[] values = args[j].Cast<ParameterValueRelativeLimited>().ToArray();
                                    
                                    f.StatTypes[j] = (StatType)Enum.Parse(typeof(StatType), args[j].BaseParam.Name);
                                    f.PercentageIncrease[j] = (int)(values[0].Amount * 100);
                                    f.MaxIncrease[j] = values[0].Maximum;
                                    f.PercentageIncreaseHQ[j] = (int)(values[1].Amount * 100);
                                    f.MaxIncreaseHQ[j] = values[1].Maximum;
                                }

                                item.FoodInfo = f;

                                CrafterFood.Add(item);
                            }
                        }
                    }
                }

                SetStatus(null, count);

                WriteItems();
            }
        }

        private static void WriteItems()
        {
            SetStatus("Writing Items to Items.db...", 0, 0, Items.Count);
            DataStream s = new DataStream();
            var values = Items.Values.ToArray();
            s.WriteS32((ushort)values.Length);
            for (int i = 0; i < Items.Count; i++)
            {
                var value = values[i];
                s.WriteS32(value.Id);
                s.WriteString(value.Name);

                s.WriteByte(value.FoodInfo == null ? (byte)0 : (byte)1);

                if (value.FoodInfo != null)
                {
                    var f = value.FoodInfo;
                    s.WriteByte((byte)f.StatTypes.Length);
                    s.WriteByte((byte)f.FoodType);
                    for (int j = 0; j < f.StatTypes.Length; j++)
                    {
                        s.WriteByte((byte)f.StatTypes[j]);
                        s.WriteS32(f.PercentageIncrease[j]);
                        s.WriteS32(f.MaxIncrease[j]);
                        s.WriteS32(f.PercentageIncreaseHQ[j]);
                        s.WriteS32(f.MaxIncreaseHQ[j]);
                    }
                }

                if (i % 100 == 0)
                    SetStatus(null, i);
            }
            SetStatus(null, values.Length);

            File.WriteAllBytes("Items.db", s.GetBytes());
            s.Flush();
            s.Close();
        }

        private static void ReadActions(bool deleteCurrent = false)
        {
            if (deleteCurrent && File.Exists("Actions.db"))
                File.Delete("Actions.db");

            Actions = new Dictionary<string, ActionInfo>();
            if (File.Exists("Actions.db"))
            {
                SetStatus("Reading Actions from Actions.db...", 0);
                DataStream s = new DataStream(File.ReadAllBytes("Actions.db"));
                int length = s.ReadS32();
                SetStatus(null, 0, 0, length);
                for (int i = 0; i < length; i++)
                {
                    ActionInfo info = new ActionInfo
                    {
                        Name = s.ReadString(),
                        Level = s.ReadS32()
                    };

                    info.Images[ClassJobInfo.GSM] = s.ReadBitmapSource();
                    info.Images[ClassJobInfo.CRP] = s.ReadBitmapSource();
                    info.Images[ClassJobInfo.BLM] = s.ReadBitmapSource();
                    info.Images[ClassJobInfo.ARM] = s.ReadBitmapSource();
                    info.Images[ClassJobInfo.LTW] = s.ReadBitmapSource();
                    info.Images[ClassJobInfo.WVR] = s.ReadBitmapSource();
                    info.Images[ClassJobInfo.ALC] = s.ReadBitmapSource();
                    info.Images[ClassJobInfo.CUL] = s.ReadBitmapSource();
                    Actions[info.Name] = info;
                    if (i % 100 == 0)
                        SetStatus(null, i);
                }
                SetStatus(null, length);
                s.Flush();
                s.Close();
            }
            else
            {
                var sheet = Game.GameData.GetSheet<SaintCoinach.Xiv.Action>();
                SetStatus("Reading Actions from sheets...", 0, 0, sheet.Count());
                int count = sheet.Count();
                int[] keys = sheet.Keys.ToArray();
                for (int i = 0; i < count; i++)
                {
                    var value = sheet[keys[i]];
                    if (value.ActionCategory.Key == 7 && value.ClassJob != null)
                    {
                        string name = value.Name;
                        if (!Actions.ContainsKey(name))
                            Actions[name] = new ActionInfo { Name = name, Level = value.ClassJobLevel };
                        Actions[name].Images[(ClassJobInfo)value.ClassJob.Key] = value.Icon.GetBitmapSource();
                        Actions[name].Images[(ClassJobInfo)value.ClassJob.Key].Freeze();




                    }
                    if (i % 100 == 0)
                        SetStatus(null, i);
                }

                var otherSheet = Game.GameData.GetSheet<CraftAction>();
                SetStatus("Reading Actions from sheets...", 0, 0, sheet.Count());
                count = otherSheet.Count();
                keys = otherSheet.Keys.ToArray();
                for (int i = 0; i < count; i++)
                {
                    var value = otherSheet[keys[i]];

                    string name = value.Name;
                    if (value.ClassJob != null && !string.IsNullOrEmpty(name))
                    {

                        if (!Actions.ContainsKey(value.Name))
                            Actions[name] = new ActionInfo { Name = name, Level = value.ClassJobLevel };
                        Actions[name].Images[(ClassJobInfo)value.ClassJob.Key] = value.Icon.GetBitmapSource();
                        Actions[name].Images[(ClassJobInfo)value.ClassJob.Key].Freeze();
                    }
                    if (i % 100 == 0)
                        SetStatus(null, i);
                }

                SetStatus(null, count);

                WriteActions();
            }
        }

        private static void WriteActions()
        {
            SetStatus("Writing Actions to Actions.db...", 0, 0, Actions.Count);
            DataStream s = new DataStream();
            var values = Actions.Values.ToArray();
            s.WriteS32(values.Length);
            for (int i = 0; i < Actions.Count; i++)
            {
                var value = values[i];
                s.WriteString(value.Name);
                s.WriteS32(value.Level);

                s.WriteBitmapSource(value.Images[ClassJobInfo.GSM]);
                s.WriteBitmapSource(value.Images[ClassJobInfo.CRP]);
                s.WriteBitmapSource(value.Images[ClassJobInfo.BLM]);
                s.WriteBitmapSource(value.Images[ClassJobInfo.ARM]);
                s.WriteBitmapSource(value.Images[ClassJobInfo.LTW]);
                s.WriteBitmapSource(value.Images[ClassJobInfo.WVR]);
                s.WriteBitmapSource(value.Images[ClassJobInfo.ALC]);
                s.WriteBitmapSource(value.Images[ClassJobInfo.CUL]);

                if (i % 100 == 0)
                    SetStatus(null, i);
            }
            SetStatus(null, values.Length);

            File.WriteAllBytes("Actions.db", s.GetBytes());
            s.Flush();
            s.Close();
        }

        private static void ReadLevelDifferences(bool deleteCurrent = false)
        {
            if (deleteCurrent && File.Exists("LevelDifferences.db"))
                File.Delete("LevelDifferences.db");

            if (File.Exists("LevelDifferences.db"))
            {
                SetStatus("Reading LevelDifferences from LevelDifferences.db...", 0);
                DataStream s = new DataStream(File.ReadAllBytes("LevelDifferences.db"));
                int length = s.ReadS32();
                LevelDifferences = new List<LevelDifferenceInfo>(length);
                SetStatus(null, 0, 0, length);
                for (ushort i = 0; i < length; i++)
                {
                    LevelDifferenceInfo info = new LevelDifferenceInfo
                    {
                        Difference = s.ReadS32(),
                        ProgressFactor = s.ReadS32(),
                        QualityFactor = s.ReadS32()
                    };
                    LevelDifferences.Add(info);
                    if (i % 100 == 0)
                        SetStatus(null, i);
                }
                SetStatus(null, length);
                s.Flush();
                s.Close();
            }
            else
            {
                var sheet = Game.GameData.GetSheet<CraftLevelDifference>();
                SetStatus("Reading LevelDifferences from sheets...", 0, 0, sheet.Count());
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


                    if (i % 100 == 0)
                        SetStatus(null, i);
                }

                SetStatus(null, count);

                WriteLevelDifferences();
            }
        }

        private static void WriteLevelDifferences()
        {
            SetStatus("Writing LevelDifferences to LevelDifferences.db...", 0, 0, LevelDifferences.Count);
            DataStream s = new DataStream();
            s.WriteS32(LevelDifferences.Count);
            for (int i = 0; i < LevelDifferences.Count; i++)
            {
                var value = LevelDifferences[i];
                s.WriteS32(value.Difference);
                s.WriteS32(value.ProgressFactor);
                s.WriteS32(value.QualityFactor);

                if (i % 100 == 0)
                    SetStatus(null, i);
            }
            SetStatus(null, LevelDifferences.Count);

            File.WriteAllBytes("LevelDifferences.db", s.GetBytes());
            s.Flush();
            s.Close();
        }

        private static void ReadRecipeRotations(bool deleteCurrent = false)
        {
            if (deleteCurrent && File.Exists("RecipeRotations.db"))
                File.Delete("RecipeRotations.db");

            if (File.Exists("RecipeRotations.db"))
            {
                SetStatus("Reading RecipeRotations from RecipeRotations.db...", 0);
                DataStream s = new DataStream(File.ReadAllBytes("RecipeRotations.db"));
                int length = s.ReadS32();
                RecipeRotations = new Dictionary<AbstractRecipeInfo, List<RecipeSolutionInfo>>(length);
                SetStatus(null, 0, 0, length);
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

                    if (i % 100 == 0)
                        SetStatus(null, i);
                }
                SetStatus(null, length);
                s.Flush();
                s.Close();
            }
            else
            {

                SetStatus("Creating RecipeRotations from Recipes...", 0, 0, Recipes.Count);

                RecipeRotations = new Dictionary<AbstractRecipeInfo, List<RecipeSolutionInfo>>();
                for (int i = 0; i < Recipes.Count; i++)
                {

                    AbstractRecipeInfo abstractInfo = Recipes[i].GetAbstractData();
                    if (!RecipeRotations.ContainsKey(abstractInfo))

                        RecipeRotations[abstractInfo] = new List<RecipeSolutionInfo>();

                    if (i % 100 == 0)
                        SetStatus(null, i);
                }

                SetStatus(null, Recipes.Count);

                WriteRecipeRotations();
            }
        }



        public static void WriteRecipeRotations()
        {
            SetStatus("Writing RecipeRotations to RecipeRotations.db...", 0, 0, RecipeRotations.Count);
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
                if (i % 100 == 0)
                    SetStatus(null, i);
            }
            SetStatus(null, RecipeRotations.Count);

            File.WriteAllBytes("RecipeRotations.db", s.GetBytes());
            s.Flush();
            s.Close();
        }

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
            if (levelDifference <= LevelDifferences[0].Difference)
                return LevelDifferences[0];
            if (levelDifference >= LevelDifferences[LevelDifferences.Count - 1].Difference)
                return LevelDifferences[LevelDifferences.Count - 1];
            for (int i = 1; i < LevelDifferences.Count - 2; i++)
                if (LevelDifferences[i].Difference == levelDifference)
                    return LevelDifferences[i];

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
            if (!G.RecipeRotations.ContainsKey(abstractData))
            {
                Debugger.Break();
                return;
            }

            

            RecipeSolutionInfo infoWithMinLevel = RecipeSolutionInfo.FromSim(s, true);
            RecipeSolutionInfo infoWithoutMinLevel = RecipeSolutionInfo.FromSim(s, false);

            var list = G.RecipeRotations[abstractData];

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

            MainWindow.UpdateRotationsCount();
        }

        public static RecipeSolutionInfo[] GetAllRotationsForRecipe(RecipeInfo recipe)
        {
            return G.RecipeRotations[recipe.GetAbstractData()].ToArray();
        }

        public static void RemoveRotation(AbstractRecipeInfo abstractRecipeInfo, RecipeSolutionInfo rotationInfo)
        {
            G.RecipeRotations[abstractRecipeInfo].Remove(rotationInfo);
            MainWindow.UpdateRotationsCount();
        }

        public static void SetStatus(string status = null, double? value = null, double? min = null, double? max = null)
        {
            MainWindow.SetStatus(status, value, min, max);
        }
    }
}
