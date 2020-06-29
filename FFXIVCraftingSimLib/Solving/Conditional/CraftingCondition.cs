using FFXIVCraftingSimLib.Actions;
using FFXIVCraftingSimLib.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSimLib.Solving.Conditional
{
    public enum PropertyInfo
    {
        TimesUsed,
        PlayerLevel,
        Step,
        RecipeCondition,
        CP,
        CurrentDurability,
        MaxDurability,
        CurrentProgress,
        MaxProgress,
        CurrentQuality,
        CurrentQualityPercentage,
        Craftsmanship,
        CurrentControl,
        LastSkillUsed,
        IQStack,
        WasteNotStack,
        ManipulationStack,
        InnovationStack,
        VenerationStack,
    }

    public enum ConditionInfo
    {
        MoreThan = 1,
        MoreThanOrEqual = 2,
        LessThan = 4,
        LessThanOrEqual = 8,
        NotEqual = 16,
        Equal = 32,
    }

    public enum PropertyTypeInfo
    {
        Int,
        Bool,
        RecipeConditionEnum,
        Action
    }

    public class PropertyComparisonInfo
    {
        public PropertyInfo PropertyInfo { get; set; }
        
        public CraftingCondition Parent { get; private set; }

        public PropertyTypeInfo PropertyTypeInfo { get; set; }
        public ConditionInfo ConditionInfo { get; set; }
        

       public ConditionInfo[] PossibleConditions
        {
            get
            {
                return CraftingCondition.PropertyConditions[PropertyInfo];
            }
        }

        public Object Value { get; set; }

        public PropertyComparisonInfo(PropertyInfo propertyInfo, CraftingCondition parent)
        {
            PropertyInfo = propertyInfo;
            Parent = parent;
        }

        public RecipeCondition[] AllRecipeConditions
        {
            get
            {
                return new RecipeCondition[]
                {
                    RecipeCondition.Normal,
                    RecipeCondition.Good,
                    RecipeCondition.Excellent,
                    RecipeCondition.Poor,
                    RecipeCondition.Centered,
                    RecipeCondition.Pliant,
                    RecipeCondition.Sturdy
                };
            }
        }

        
    }

    public class CraftingCondition : INotifyPropertyChanged, INotifyCollectionChanged
    {
        public static Dictionary<PropertyInfo, string> PropertyNames = new Dictionary<PropertyInfo, string>
        {
            [PropertyInfo.TimesUsed] = "Times Used",
            [PropertyInfo.PlayerLevel] = "Player Level",
            [PropertyInfo.Step] = "Step",
            [PropertyInfo.RecipeCondition] = "Recipe Condition",
            [PropertyInfo.CP] = "Current CP",
            [PropertyInfo.CurrentDurability] = "Durability",
            [PropertyInfo.MaxDurability] = "Max Durability",
            [PropertyInfo.CurrentProgress] = "Progress",
            [PropertyInfo.MaxProgress] = "Max Progress",
            [PropertyInfo.CurrentQuality] = "Quality",
            [PropertyInfo.CurrentQualityPercentage] = "Quality %",
            [PropertyInfo.Craftsmanship] = "Craftsmanship",
            [PropertyInfo.CurrentControl] = "Current Control",
            [PropertyInfo.LastSkillUsed] = "Last Skill Used",
            [PropertyInfo.IQStack] = "IQ Stack Count",
            [PropertyInfo.WasteNotStack] = "Waste Not Stack Count",
            [PropertyInfo.ManipulationStack] = "Manipulation Stack Sount",
            [PropertyInfo.InnovationStack] = "Inovation Stack Count",
            [PropertyInfo.VenerationStack] = "Veneration Stack Count",
        };

        public static Dictionary<PropertyInfo, PropertyTypeInfo> PropertyTypes = new Dictionary<PropertyInfo, PropertyTypeInfo>
        {
            [PropertyInfo.TimesUsed] = PropertyTypeInfo.Int,
            [PropertyInfo.PlayerLevel] = PropertyTypeInfo.Int,
            [PropertyInfo.Step] = PropertyTypeInfo.Int,
            [PropertyInfo.RecipeCondition] = PropertyTypeInfo.RecipeConditionEnum,
            [PropertyInfo.CP] = PropertyTypeInfo.Int,
            [PropertyInfo.CurrentDurability] = PropertyTypeInfo.Int,
            [PropertyInfo.MaxDurability] = PropertyTypeInfo.Int,
            [PropertyInfo.CurrentProgress] = PropertyTypeInfo.Int,
            [PropertyInfo.MaxProgress] = PropertyTypeInfo.Int,
            [PropertyInfo.CurrentQuality] = PropertyTypeInfo.Int,
            [PropertyInfo.CurrentQualityPercentage] = PropertyTypeInfo.Int,
            [PropertyInfo.Craftsmanship] = PropertyTypeInfo.Int,
            [PropertyInfo.CurrentControl] = PropertyTypeInfo.Int,
            [PropertyInfo.LastSkillUsed] = PropertyTypeInfo.Action,
            [PropertyInfo.IQStack] = PropertyTypeInfo.Int,
            [PropertyInfo.WasteNotStack] = PropertyTypeInfo.Int,
            [PropertyInfo.ManipulationStack] = PropertyTypeInfo.Int,
            [PropertyInfo.InnovationStack] = PropertyTypeInfo.Int,
            [PropertyInfo.VenerationStack] = PropertyTypeInfo.Int,
        };

        public static Dictionary<ConditionInfo, string> ConditionNames = new Dictionary<ConditionInfo, string>
        {
            [ConditionInfo.MoreThan] = ">",
            [ConditionInfo.MoreThanOrEqual] = ">=",
            [ConditionInfo.LessThan] = "<",
            [ConditionInfo.LessThanOrEqual] = "<=",
            [ConditionInfo.NotEqual] = "!=",
            [ConditionInfo.Equal] = "=",
        };


        public static Dictionary<PropertyInfo, ConditionInfo[]> PropertyConditions = new Dictionary<PropertyInfo, ConditionInfo[]>
        {
            [PropertyInfo.TimesUsed] = new ConditionInfo[] { ConditionInfo.Equal },
            [PropertyInfo.PlayerLevel] = GetAll(),
            [PropertyInfo.Step] = GetAll(),
            [PropertyInfo.RecipeCondition] = new ConditionInfo[] { ConditionInfo.Equal, ConditionInfo.NotEqual },
            [PropertyInfo.CP] = GetAll(),
            [PropertyInfo.CurrentDurability] = GetAll(),
            [PropertyInfo.MaxDurability] = GetAll(),
            [PropertyInfo.CurrentProgress] = GetAll(),
            [PropertyInfo.MaxProgress] = GetAll(),
            [PropertyInfo.CurrentQuality] = GetAll(),
            [PropertyInfo.CurrentQualityPercentage] = GetAll(),
            [PropertyInfo.Craftsmanship] = GetAll(),
            [PropertyInfo.CurrentControl] = GetAll(),
            [PropertyInfo.LastSkillUsed] = GetAll(),
            [PropertyInfo.IQStack] = GetAll(),
            [PropertyInfo.WasteNotStack] = GetAll(),
            [PropertyInfo.ManipulationStack] = GetAll(),
            [PropertyInfo.InnovationStack] = GetAll(),
            [PropertyInfo.VenerationStack] = GetAll(),
        };

        public static ConditionInfo[] GetAll()
        {
            ConditionInfo[] result = new[]
            {
                ConditionInfo.MoreThan,
                ConditionInfo.MoreThanOrEqual,
                ConditionInfo.LessThan,
                ConditionInfo.LessThanOrEqual,
                ConditionInfo.NotEqual,
                ConditionInfo.Equal,
            };

            return result;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public event NotifyCollectionChangedEventHandler CollectionChanged = delegate { };

        private CraftingSim Sim { get; set; }

        public CraftingAction CraftingAction { get; private set; }

        public ObservableCollection<PropertyComparisonInfo> Conditions { get; private set; }

        public string ConditionsText
        {
            get
            {
                if (Conditions.Count == 0)
                    return "None";
                return string.Join("\r\n", Conditions.Select(x => $"{PropertyNames[x.PropertyInfo]} {ConditionNames[x.ConditionInfo]} {x.Value}"));
            }
        }

        public ActionSettings ActionSettings { get; set; }

        public CraftingCondition(CraftingSim sim, CraftingAction craftingAction)
        {
            Sim = sim;
            CraftingAction = craftingAction;
            ActionSettings = new ActionSettings();
            Conditions = new ObservableCollection<PropertyComparisonInfo>();
            Conditions.CollectionChanged += Conditions_CollectionChanged;
        }

        private void Conditions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            PropertyChanged(this, new PropertyChangedEventArgs("ConditionsText"));
            CollectionChanged(this, e);
        }

        public void AddCondition(PropertyInfo propertyInfo)
        {
            var newComparison = new PropertyComparisonInfo(propertyInfo, this);
            Conditions.Add(newComparison);
        }

        public void AddCondition(PropertyInfo propertyInfo, ConditionInfo conditionInfo, object value)
        {
            var newComparison = new PropertyComparisonInfo(propertyInfo, this);
            newComparison.ConditionInfo = conditionInfo;
            newComparison.Value = value;
            Conditions.Add(newComparison);
        }

        public void RemoveCondition(PropertyComparisonInfo property)
        {
            if (Conditions.Contains(property))
            {
                Conditions.Remove(property);
            }
            
        }

        public void RemoveConditionAt(int i)
        {
            var condition = Conditions[i];
            Conditions.RemoveAt(i);
        }

        private void NewComparison_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged(this, new PropertyChangedEventArgs("ConditionsText"));
        }

        public bool AreConditionsSatisfied(CraftingSim sim)
        {
            return Conditions.All(x => IsConditionSatisfied(x, sim));
        }

        private bool IsConditionSatisfied(PropertyComparisonInfo variable, CraftingSim sim)
        {
            object propertyValue = GetPropertyValue(variable, sim);
            object comparisonValue = variable.Value;

            switch(variable.PropertyTypeInfo)
            {
                case PropertyTypeInfo.Int:
                    return Compare(propertyValue, comparisonValue, variable.ConditionInfo);


                case PropertyTypeInfo.Bool:
                    return Compare(propertyValue, comparisonValue, variable.ConditionInfo);


                case PropertyTypeInfo.RecipeConditionEnum:
                    return Compare(propertyValue, comparisonValue, variable.ConditionInfo);
            }

            throw new Exception();
        }

        private bool Compare(object propertyValue, object comparisonValue, ConditionInfo condition)
        {
            if (propertyValue is bool)
                Debugger.Break();
            int propIntValue = Convert.ToInt32(propertyValue);
            int compIntValue = Convert.ToInt32(comparisonValue);
            switch (condition)
            {
                case ConditionInfo.MoreThan:
                    return propIntValue > compIntValue;

                case ConditionInfo.MoreThanOrEqual:
                    return propIntValue >= compIntValue;

                case ConditionInfo.LessThan:
                    return propIntValue < compIntValue;

                case ConditionInfo.LessThanOrEqual:
                    return propIntValue <= compIntValue;

                case ConditionInfo.NotEqual:
                    return propIntValue != compIntValue;

                case ConditionInfo.Equal:
                    return propIntValue == compIntValue;
            }

            throw new Exception();
        }

        private Object GetPropertyValue(PropertyComparisonInfo variable, CraftingSim sim)
        {
            switch (variable.PropertyInfo)
            {
                case PropertyInfo.TimesUsed:
                    return ActionSettings.TimesUsed;

                case PropertyInfo.PlayerLevel:
                    return sim.Level;

                case PropertyInfo.Step:
                    return sim.Step;

                case PropertyInfo.RecipeCondition:
                    return sim.StepSettings[sim.Step].RecipeCondition;

                case PropertyInfo.CP:
                    return sim.CurrentCP;

                case PropertyInfo.CurrentDurability:
                    return sim.CurrentDurability;

                case PropertyInfo.MaxDurability:
                    return sim.CurrentRecipe.Durability;

                case PropertyInfo.CurrentProgress:
                    return sim.CurrentProgress;

                case PropertyInfo.MaxProgress:
                    return sim.CurrentRecipe.MaxProgress;

                case PropertyInfo.CurrentQuality:
                    return sim.CurrentQuality;

                case PropertyInfo.CurrentQualityPercentage:
                    return (int)(sim.CurrentQuality * 100d / sim.CurrentRecipe.MaxQuality);

                case PropertyInfo.Craftsmanship:
                    return sim.Craftsmanship;

                case PropertyInfo.CurrentControl:
                    return (int)sim.ActualControl;

                case PropertyInfo.LastSkillUsed:
                    return null;

                case PropertyInfo.IQStack:
                    if (sim.InnerQuietBuff != null)
                        return sim.InnerQuietBuff.Stack;
                    return 0;

                case PropertyInfo.WasteNotStack:
                    if (sim.WasteNotBuff != null)
                        return sim.WasteNotBuff.Stack;
                    return 0;

                case PropertyInfo.ManipulationStack:
                    if (sim.ManipulationBuff != null)
                        return sim.ManipulationBuff.Stack;
                    return 0;

                case PropertyInfo.InnovationStack:
                    if (sim.InnovationBuff != null)
                        return sim.InnovationBuff.Stack;
                    return 0;

                case PropertyInfo.VenerationStack:
                    if (sim.InnovationBuff != null)
                        return sim.InnovationBuff.Stack;
                    return 0;
            }

            throw new Exception();
        }

    }

    public class ActionSettings
    {
        public int TimesUsed { get; set; }

        public ActionSettings()
        {

        }

        public void Reset()
        {
            TimesUsed = 0;
        }
    }
}
