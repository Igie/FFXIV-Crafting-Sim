using FFXIVCraftingSim.Actions;
using FFXIVCraftingSim.Solving.Conditional;
using FFXIVCraftingSim.Types;
using SaintCoinach.Xiv;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FFXIVCraftingSim.GUI.Windows
{
    /// <summary>
    /// Interaction logic for ConditionalSolvingWindow.xaml
    /// </summary>
    public partial class ConditionalSolvingWindow : Window, INotifyPropertyChanged
    {
        public ImageAndActionContainer[] AllActions { get; private set; }
        public PropertyInfo[] AllProperties { get; private set; }
        public ConditionInfo[] AllConditions { get; private set; }

        private CraftingSim Sim { get; set; }
        private CraftingSim OriginalSim { get; set; }
        private CraftingCondition _SelectedCondition { get; set; }
        public CraftingCondition SelectedCondition
        {
            get { return _SelectedCondition; }
            set
            {
                PropertyChanging(this, new PropertyChangingEventArgs("SelectedCondition"));
                _SelectedCondition = value;
                PropertyChanged(this, new PropertyChangedEventArgs("SelectedCondition"));
            }
        }

        public ObservableCollection<CraftingCondition> CraftingConditions { get; set; }

        public event Action<CraftingSim> FinishedConditionExecution = delegate { };

        public ConditionalSolvingWindow()
        {
            InitializeComponent();
        }
        public event PropertyChangingEventHandler PropertyChanging = delegate { };
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void Initialize(CraftingSim sim)
        {
            Sim = sim.Clone();
            OriginalSim = sim;
            AllActions = CraftingAction.CraftingActions.Values.Select(x => new ImageAndActionContainer(Sim, x)).ToArray();
            AllProperties = Enum.GetValues(typeof(PropertyInfo)).OfType<PropertyInfo>().ToArray();
            AllConditions = Enum.GetValues(typeof(ConditionInfo)).OfType<ConditionInfo>().ToArray();

            CraftingConditions = new ObservableCollection<CraftingCondition>();

            CraftingConditions.CollectionChanged += OnChanged;
        }

       

        private void RemoveClicked(object sender, RoutedEventArgs e)
        {
            if (ListViewConditions.SelectedIndex < 0) return;
            CraftingConditions.RemoveAt(ListViewConditions.SelectedIndex);
        }

        private void AddClicked(object sender, RoutedEventArgs e)
        {
            var a = (ComboBoxNewAction.SelectedValue as ImageAndActionContainer);
                if (a == null) return;

            CraftingCondition newCondition = new CraftingCondition(Sim, a.CraftingAction);
            CraftingConditions.Add(newCondition);
            ListViewConditions.SelectedIndex = CraftingConditions.Count - 1;
            newCondition.PropertyChanged += OnChanged;
            newCondition.CollectionChanged += OnChanged;
        }

        private void AddNewProperty(object sender, RoutedEventArgs e)
        {
            if (SelectedCondition == null) return;
            SelectedCondition.AddCondition(PropertyInfo.Step);
        }

        private void OnChanged(object sender, EventArgs e)
        {
            SimulateConditions();
        }
        private void SimulateConditions()
        {
            if (CheckBoxSimulateOnChanges.IsChecked != true) return;

            Sim.RemoveActions();

            foreach (var c in CraftingConditions)
                c.ActionSettings.Reset();

            bool shouldContinue = true;
            while (shouldContinue)
            {
                var f = CraftingConditions.Where(x => x.AreConditionsSatisfied(Sim)).ToArray();
                if (f.Length == 0 || Sim.CraftingActionsLength >= CraftingSim.MaxActions || Sim.CurrentDurability <= 0)
                    shouldContinue = false;
                else
                {
                    int simLength = Sim.CraftingActionsLength;
                    for (int i = 0; i < f.Length; i++)
                    { 
                        Sim.AddActions(true, f[i].ActionContainer.CraftingAction);
                        if (Sim.CraftingActionsLength > simLength)
                        {
                            f[i].ActionSettings.TimesUsed++;
                            break;
                        }
                        else if (i == f.Length - 1)
                            shouldContinue = false;
                    }
                }
            }
            FinishedConditionExecution(Sim);
        }

        private void ShuffleSimRecipeConditionsClicked(object sender, RoutedEventArgs e)
        {
            bool isExpert = Sim.CurrentRecipe.Level == 481;

            Dictionary<double, RecipeCondition> Chances = new Dictionary<double, RecipeCondition>();

            if (isExpert)
            {
                Chances[0.12] = RecipeCondition.Good;
                Chances[0.12 + 0.15] = RecipeCondition.Centered;
                Chances[0.12 + 0.15 + 0.12] = RecipeCondition.Pliant;
                Chances[0.12 + 0.15 + 0.12 + 0.15] = RecipeCondition.Sturdy;
                Chances[1] = RecipeCondition.Normal;
            } else
            {
                Chances[0.2] = RecipeCondition.Good;
                Chances[0.2 + 0.04] = RecipeCondition.Excellent;
                Chances[1] = RecipeCondition.Normal;
            }

            Random r = new Random();

            bool nextShouldBePoor = false;

            for (int i = 0; i < Sim.StepSettings.Length; i++)
            {
                if (nextShouldBePoor)
                {
                    Sim.StepSettings[i].RecipeCondition = RecipeCondition.Poor;
                    nextShouldBePoor = false;
                    continue;
                }
                var chance = r.NextDouble();
                double key = Chances.Keys.FirstOrDefault(x => chance <= x);
                RecipeCondition condition = Chances[key];
                Sim.StepSettings[i].RecipeCondition = condition;
                if (condition == RecipeCondition.Excellent)
                    nextShouldBePoor = true;
            }

            SimulateConditions();
        }

        private void MoveActionUp(object sender, RoutedEventArgs e)
        {
            var button = (sender as Button);
            if (button == null || button.Tag == null) return;

            var condition = button.Tag as CraftingCondition;
            int index = CraftingConditions.IndexOf(condition);
            if (index > 0)
            {
                var tmp = CraftingConditions[index];
                CraftingConditions[index] = CraftingConditions[index - 1];
                CraftingConditions[index - 1] = tmp;
            }
        }

        private void MoveActionDown(object sender, RoutedEventArgs e)
        {
            var button = (sender as Button);
            if (button == null || button.Tag == null) return;

            var condition = button.Tag as CraftingCondition;
            int index = CraftingConditions.IndexOf(condition);

            if (index < CraftingConditions.Count - 1)
            {
                var tmp = CraftingConditions[index];
                CraftingConditions[index] = CraftingConditions[index + 1];
                CraftingConditions[index + 1] = tmp;
            }
        }

        private void RemoveAction(object sender, RoutedEventArgs e)
        {
            var button = (sender as Button);
            if (button == null || button.Tag == null) return;

            var condition = button.Tag as CraftingCondition;

            if (CraftingConditions.Contains(condition))
                CraftingConditions.Remove(condition);
        }

        private void RemoveActionCondition(object sender, RoutedEventArgs e)
        {
            var button = (sender as Button);
            if (button == null || button.Tag == null) return;

            var condition = button.Tag as PropertyComparisonInfo;
            if (condition == null) return;

            condition.Parent.RemoveCondition(condition);
            
        }
    }
}
