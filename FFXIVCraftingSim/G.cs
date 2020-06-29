using FFXIVCraftingSim.Converters;
using FFXIVCraftingSim.Stream;
using FFXIVCraftingSim.Types;
using FFXIVCraftingSim.Types.GameData;
using FFXIVCraftingSimLib;
using FFXIVCraftingSimLib.Types;
using FFXIVCraftingSimLib.Types.GameData;
using Microsoft.Win32;
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

        

        public static List<RecipeInfo> Recipes { get; private set; }
        public static Dictionary<RecipeInfo, List<ItemInfo>> Ingredients { get; private set; }


        public static Dictionary<int, ItemInfo> Items { get; private set; }
        public static Dictionary<string, ActionInfo> Actions { get; private set; }

        public static List<ItemInfo> CrafterFood { get; private set; }

        public static Task InitTask { get; private set; }
        public static Task ReloadTask { get; private set; }

        public static event System.Action Initialized = delegate { };
        public static event System.Action Reloaded = delegate { };
        static G()
        {
            GameData.Initialized += GameDataInitialized;
            GameData.Reloaded += GameDataReloaded;
        }

        private static void GameDataInitialized()
        {
            if (InitTask == null || InitTask.IsCompleted)
                InitTask = Task.Run(() =>
                {
                    ReadItems();
                    ReadRecipes();
                    ReadActions();
                    Initialized();
                });
        }

        private static void GameDataReloaded()
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
                    Reloaded();
                });
            }
        }

        public static void Init(MainWindow window)
        {
            DataStreamEx s = new DataStreamEx();
            MainWindow = window;

            string directory1 = @"C:\Program Files (x86)\SquareEnix\FINAL FANTASY XIV - A Realm Reborn";
            string directory2 = @"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XIV Online";

            string dir = "";

            if (Directory.Exists(directory1))
                dir = directory1;
            else if (Directory.Exists(directory2))
                dir = directory2;
            else
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Title = "Select FFXIV Location";
                if (dialog.ShowDialog() == false)
                    App.Current.Shutdown(0);
                string name = Directory.GetParent(Path.GetDirectoryName(dialog.FileName)).FullName;
                dir = name;
            }
            GameData.Init(dir);
        }

        public static void ReloadDatabase()
        {
            GameData.Reload();
        }

        private static void ReadRecipes(bool deleteCurrent = false)
        {
            if (deleteCurrent && File.Exists("Recipes.db"))
                File.Delete("Recipes.db");

            if (File.Exists("Recipes.db"))
            {
                SetStatus("Reading Recipes from Recipes.db...", 0);
                DataStreamEx s = new DataStreamEx(File.ReadAllBytes("Recipes.db"));
                int length = s.ReadS32();
                Recipes = new List<RecipeInfo>(length);
                Ingredients = new Dictionary<RecipeInfo, List<ItemInfo>>();
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
                    List<ItemInfo> ingredients = new List<ItemInfo>(c);
                    for (int j = 0; j < c; j++)
                        ingredients.Add(Items[s.ReadS32()]);
                    Ingredients[info] = ingredients;
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
                var sheet = GameData.Game.GameData.GetSheet<Recipe>();
                SetStatus("Reading Recipes from sheets...", 0, 0, sheet.Count());
                int count = sheet.Count();
                int[] keys = sheet.Keys.ToArray();
                Recipes = new List<RecipeInfo>(count);
                Ingredients = new Dictionary<RecipeInfo, List<ItemInfo>>();
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
                            List<ItemInfo> ingredients = new List<ItemInfo>(value.Ingredients.Count());
                            ingredients.AddRange(value.Ingredients.Select(x => Items[x.Item.Key]));
                            Ingredients[info] = ingredients;
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
            DataStreamEx s = new DataStreamEx();
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

                var ingredients = Ingredients[value];

                s.WriteByte((byte)ingredients.Count);
                for (int j = 0; j < ingredients.Count; j++)
                    s.WriteS32(ingredients[j].Id);
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
                DataStreamEx s = new DataStreamEx(File.ReadAllBytes("Items.db"));
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
                var sheet = GameData.Game.GameData.GetSheet<Item>();

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
            DataStreamEx s = new DataStreamEx();
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
                DataStreamEx s = new DataStreamEx(File.ReadAllBytes("Actions.db"));
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
                var sheet = GameData.Game.GameData.GetSheet<SaintCoinach.Xiv.Action>();
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

                var otherSheet = GameData.Game.GameData.GetSheet<CraftAction>();
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
            DataStreamEx s = new DataStreamEx();
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

      

        

        public static RecipeSolutionInfo[] GetAllRotationsForRecipe(RecipeInfo recipe)
        {
            return GameData.RecipeRotations[recipe.GetAbstractData()].ToArray();
        }

        public static void RemoveRotation(AbstractRecipeInfo abstractRecipeInfo, RecipeSolutionInfo rotationInfo)
        {
            GameData.RecipeRotations[abstractRecipeInfo].Remove(rotationInfo);
            MainWindow.UpdateRotationsCount();
        }

        public static void SetStatus(string status = null, double? value = null, double? min = null, double? max = null)
        {
            MainWindow.SetStatus(status, value, min, max);
        }
    }
}
