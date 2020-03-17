using FFXIV_Crafting_Sim.Converters;
using FFXIV_Crafting_Sim.Stream;
using FFXIV_Crafting_Sim.Types.GameData;
using SaintCoinach;
using SaintCoinach.Xiv;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIV_Crafting_Sim
{
    public static class G
    {
        public static MainWindow MainWindow { get; private set; }

        public static ARealmReversed Game { get; private set; }

        public static List<RecipeInfo> Recipes { get; private set; }
        public static Dictionary<int, ItemInfo> Items { get; private set; }


        public static Task InitTask { get; private set; }
        public static Task ReloadTask { get; private set; }

        public static void Init(MainWindow window)
        {
            MainWindow = window;
            Game = new ARealmReversed(@"C:\Program Files (x86)\SquareEnix\FINAL FANTASY XIV - A Realm Reborn", SaintCoinach.Ex.Language.English);

            if (InitTask == null || InitTask.IsCompleted)
                InitTask = Task.Run(() =>
                {
                    ReadRecipes();
                    ReadItems();
                });
        }

        public static void ReloadDatabase()
        {
            if (ReloadTask == null || ReloadTask.IsCompleted)
                ReloadTask = Task.Run(() =>
                    {
                    if (!InitTask.IsCompleted)
                        InitTask.Wait();
                        ReadRecipes(true);
                        ReadItems(true);
                    });
        }

        private static void ReadRecipes(bool deleteCurrent = false)
        {
            if (deleteCurrent && File.Exists("Recipes.db"))
                File.Delete("Recipes.db");
            
            if (File.Exists("Recipes.db"))
            {
                SetStatus("Reading Recipes from Recipes.db...", 0);
                DataStream s = new DataStream(File.ReadAllBytes("Recipes.db"));
                ushort length = s.ReadUShort();
                Recipes = new List<RecipeInfo>(length);
                SetStatus(null, 0, 0, length);
                for (ushort i = 0; i < length; i++)
                {
                    RecipeInfo info = new RecipeInfo
                    {
                        Id = s.ReadInt(),
                        Name = s.ReadString(),
                        Level = s.ReadInt(),
                        RequiredCraftsmanship = s.ReadInt(),
                        RequiredControl = s.ReadInt(),
                        Durability = s.ReadInt(),
                        MaxProgress = s.ReadInt(),
                        MaxQuality = s.ReadInt()
                    };
                    Recipes.Add(info);
                    if (i % 100 == 0)
                        SetStatus(null, i);
                }
                SetStatus(null, length);
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
                            RequiredCraftsmanship = value.RequiredCraftsmanship,
                            RequiredControl = value.RequiredControl,
                            Durability = recipeLevelTable.Durability * value.DurabilityFactor / 100,
                            MaxProgress = recipeLevelTable.Difficulty * value.DifficultyFactor / 100,
                            MaxQuality = recipeLevelTable.Quality * value.QualityFactor / 100
                        };

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
            SetStatus("Writing Recipes to Items.db...", 0, 0, Recipes.Count);
            DataStream s = new DataStream();
            s.WriteUShort((ushort)Recipes.Count);
            for (int i = 0; i < Recipes.Count; i++)
            {
                var value = Recipes[i];
                s.WriteInt(value.Id);
                s.WriteString(value.Name);
                s.WriteInt(value.Level);
                s.WriteInt(value.RequiredCraftsmanship);
                s.WriteInt(value.RequiredControl);
                s.WriteInt(value.Durability);
                s.WriteInt(value.MaxProgress);
                s.WriteInt(value.MaxQuality);

                if (i % 100 == 0)
                    SetStatus(null, i);
            }
            SetStatus(null, Recipes.Count);

            File.WriteAllBytes("Recipes.db", s.GetBytes());
        }

            private static void ReadItems(bool deleteCurrent = false)
        {    
            if (deleteCurrent && File.Exists("Items.db"))
                File.Delete("Items.db");
            
            Items = new Dictionary<int, ItemInfo>();
            if (File.Exists("Items.db"))
            {
                SetStatus("Reading Items from Items.db...", 0);
                DataStream s = new DataStream(File.ReadAllBytes("Items.db"));
                ushort length = s.ReadUShort();
                SetStatus(null, 0, 0, length);
                for (ushort i = 0; i < length; i++)
                {
                    ItemInfo info = new ItemInfo
                    {
                        Id = s.ReadInt(),
                        Name = s.ReadString(),
                    };
                    Items[info.Id] = info;
                    if (i % 100 == 0)
                        SetStatus(null, i);
                }
                SetStatus(null, length);
            }
            else
            {
                var sheet = Game.GameData.GetSheet<Item>();
                SetStatus("Reading Items from sheets...", 0, 0, sheet.Count());
                int count = sheet.Count();
                int[] keys = sheet.Keys.ToArray();
                for (int i = 0; i < count; i++)
                {
                    var value = sheet[keys[i]];
                    if (!string.IsNullOrEmpty(value.Name))
                        Items[value.Key] = new ItemInfo
                        {
                            Id = value.Key,
                            Name = value.Name,
                        };
                    if (i % 100 == 0)
                        SetStatus(null, i);
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
            s.WriteUShort((ushort)values.Length);
            for (int i = 0; i < Items.Count; i++) { 
                var value = values[i];
                s.WriteInt(value.Id);
                s.WriteString(value.Name);

                if (i % 100 == 0) 
                SetStatus(null, i);
            }
            SetStatus(null, values.Length);

            File.WriteAllBytes("Items.db", s.GetBytes());
        }

        public static void SetStatus(string status = null, double? value = null, double? min = null, double? max = null)
        {
            MainWindow.SetStatus(status, value, min, max);
        }
    }
}
