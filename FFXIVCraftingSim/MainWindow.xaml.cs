using FFXIVCraftingSim;
using FFXIVCraftingSim.GUI;
using FFXIVCraftingSim.GUI.Windows;
using FFXIVCraftingSim.Stream;
using FFXIVCraftingSim.Types.GameData;
using FFXIVCraftingSimLib;
using FFXIVCraftingSimLib.Actions;
using FFXIVCraftingSimLib.Solving;
using FFXIVCraftingSimLib.Solving.GeneticAlgorithm;
using FFXIVCraftingSimLib.Types;
using FFXIVCraftingSimLib.Types.GameData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace FFXIVCraftingSim
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public CraftingSimEx Sim { get; set; }

        private CraftingActionContainer[] AvailableActions { get; set; }

        private bool FoodIsHQ { get; set; }
        private bool TeaIsHQ { get; set; }
        private ItemInfo SelectedFood { get; set; }
        private ItemInfo SelectedTea { get; set; }

        private GASolver Solver { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            Sim = new CraftingSimEx();
            Sim.FinishedStep += Sim_FinishedStep;
            Sim.FinishedExecution += Sim_FinishedExecution;
            Sim.PropertyChanged += Sim_PropertyChanged;
        }

        private void Sim_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Level")
            {
                if (Sim.CurrentRecipe != null)
                {
                    UpdateAvailableActions(Sim.CurrentRecipe);
                    var actions = Sim.GetCraftingActions();
                    Sim.RemoveActions();
                    Sim.AddActions(true, actions);
                        
                }
            }
        }

        private void TextChangedAllowNumericOnly(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (sender == null) return;
            string text = textBox.Text;
            textBox.Text = string.Concat(text.Where(x => char.IsDigit(x)));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.UnhandledException += new DispatcherUnhandledExceptionEventHandler((object s, DispatcherUnhandledExceptionEventArgs ee) =>
               {
                   Debugger.Log(0, "Error", ee.ToString());
               });

            G.Initialized += GInitialized;
            G.Reloaded += GInitialized;
            G.Init(this);
        }

        private void GInitialized()
        {
            if (File.Exists("Settings.db"))
            {
                DataStreamEx s = new DataStreamEx(File.ReadAllBytes("Settings.db"));
                int recipeId = s.ReadInt();

                Sim.Level = s.ReadInt();
                Sim.BaseCraftsmanship = s.ReadInt();
                Sim.BaseControl = s.ReadInt();
                Sim.BaseMaxCP = s.ReadInt();

                Dispatcher.Invoke(() =>
                {
                    CheckBoxIsSpecialist.IsChecked = s.ReadByte() == 1;
                });

                int id = s.ReadInt();
                if (id > 0)
                {
                    SelectedFood = G.Items[id];
                    FoodIsHQ = s.ReadByte() == 1;
                }
                id = s.ReadInt();
                if (id > 0)
                {
                    SelectedTea = G.Items[id];
                    TeaIsHQ = s.ReadByte() == 1;
                }
                Dispatcher.Invoke(() =>
                {
                    ApplyFoodBuffs();
                });
                List<CraftingAction> actions = new List<CraftingAction>();
                while (s.Position < s.Length)
                {
                    try
                    {
                        actions.Add(CraftingAction.CraftingActions[s.ReadInt()]);
                    }
                    catch { }
                }

                if (recipeId > 0)
                    LoadRecipe(G.Recipes.FirstOrDefault(x => x.Id == recipeId));

                Sim.AddActions(true, actions);
                s.Flush();
                s.Close();
            }

            if (Solver == null)
            {
                Solver = new GASolver(Sim);
                Solver.GenerationRan += Solver_GenerationRan;
                Solver.FoundBetterRotation += Solver_FoundBetterRotation;
                Solver.Stopped += Solver_Stopped;
            }
        }

        private void Solver_Stopped()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    if (PopulationsWindow != null)
                        PopulationsWindow.Close();
                    ButtonFindBest.Content = "Simulate";
                });
            }
            catch (Exception e)
            { Debugger.Break(); }
        }

        private void Solver_FoundBetterRotation(CraftingSim obj)
        {
            UpdateRotationsCount();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                GameData.WriteRecipeRotations();
                TextBlockCP.Text.ToString();
                if (Sim == null)
                    return;
                DataStreamEx s = new DataStreamEx();
                Sim.CraftsmanshipBuff = 0;
                Sim.ControlBuff = 0;
                Sim.MaxCPBuff = 0;
                if (Sim.CurrentRecipe != null)
                    s.WriteInt(Sim.CurrentRecipe.Id);
                else
                    s.WriteInt(0);
                s.WriteInt(Sim.Level);
                s.WriteInt(Sim.Craftsmanship);
                s.WriteInt(Sim.Control);
                s.WriteInt(Sim.MaxCP);
                s.WriteByte(CheckBoxIsSpecialist.IsChecked == true ? (byte)1 : (byte)0);
                s.WriteInt(SelectedFood == null ? 0 : SelectedFood.Id);
                if (SelectedFood != null)
                    s.WriteByte(FoodIsHQ ? (byte)1 : (byte)0);
                s.WriteInt(SelectedTea == null ? 0 : SelectedTea.Id);
                if (SelectedTea != null)
                    s.WriteByte(TeaIsHQ ? (byte)1 : (byte)0);
                var actions = Sim.GetCraftingActions();
                foreach (var action in actions)
                    s.WriteInt(action.Id);
                if (File.Exists("Settings.db"))
                    File.Delete("Settings.db");
                File.WriteAllBytes("Settings.db", s.GetBytes());
                s.Flush();
                s.Close();
            }
            catch(Exception ee) {
                Debugger.Break();
            }
        }

        public void SetStatus(string status = null, double? value = null, double? min = null, double? max = null)
        {
            Dispatcher.Invoke(() =>
            {
                if (status != null)
                    LabelStatus.Content = status;
                if (min.HasValue)
                    ProgressBarStatus.Minimum = min.Value;

                if (max.HasValue)
                    ProgressBarStatus.Maximum = max.Value;

                if (value.HasValue)
                {
                    if (value.Value < 0)
                        ProgressBarStatus.Value -= value.Value;
                    else
                        ProgressBarStatus.Value = value.Value;
                }
            });
        }

        private void ReloadDatabaseClicked(object sender, RoutedEventArgs e)
        {
            G.ReloadDatabase();
        }

        private void ButtonSelectRecipe_Click(object sender, RoutedEventArgs e)
        {
            RecipeSelectionWindow window = new RecipeSelectionWindow();

            if (window.ShowDialog() == true)
            {
                LoadRecipe(window.SelectedRecipe);
            }

            Debugger.Log(0, "", "Selected Recipe: " + (window.SelectedRecipe != null ? window.SelectedRecipe.Name : "None") + "\r\n");
        }

        private void ButtonFood_Click(object sender, RoutedEventArgs e)
        {
            FoodSelectionWindow window = new FoodSelectionWindow();
            window.LoadList(G.CrafterFood.Where(x => x.FoodInfo.FoodType == FoodType.Food));
            if (window.ShowDialog() == true)
            {
                SelectedFood = window.SelectedItem;
                if (SelectedFood != null)
                    FoodIsHQ = window.IsHQ;


                ApplyFoodBuffs();
            }
        }

        private void ButtonTea_Click(object sender, RoutedEventArgs e)
        {
            FoodSelectionWindow window = new FoodSelectionWindow();
            window.LoadList(G.CrafterFood.Where(x => x.FoodInfo.FoodType == FoodType.Tea));
            if (window.ShowDialog() == true)
            {
                SelectedTea = window.SelectedItem;
                if (SelectedTea != null)
                    TeaIsHQ = window.IsHQ;


                ApplyFoodBuffs();
            }
        }

        private void SpecialistCheckChanged(object sender, RoutedEventArgs e)
        {
            ApplyFoodBuffs();
        }

        private void ApplyFoodBuffs()
        {
            Sim.CraftsmanshipBuff = 0;
            Sim.ControlBuff = 0;
            Sim.MaxCPBuff = 0;

            int currentCraftsmanship = Sim.Craftsmanship;
            int currentControl = Sim.Control;
            int currentMaxCP = Sim.MaxCP;

            if (CheckBoxIsSpecialist.IsChecked == true)
            {
                Sim.CraftsmanshipBuff += 20;
                Sim.ControlBuff += 20;
                Sim.MaxCPBuff += 15;
            }

            if (SelectedFood != null)
            {
                Dispatcher.Invoke(() => ButtonFood.Content = (FoodIsHQ ? "HQ" : "NQ") + ' ' + SelectedFood.Name);
                Sim.CraftsmanshipBuff += SelectedFood.FoodInfo.GetCraftsmanshipBuff(currentCraftsmanship, FoodIsHQ);
                Sim.ControlBuff += SelectedFood.FoodInfo.GetControlBuff(currentControl, FoodIsHQ);
                Sim.MaxCPBuff += SelectedFood.FoodInfo.GetMaxCPBuff(currentMaxCP, FoodIsHQ);
            }
            else
                Dispatcher.Invoke(() => ButtonFood.Content = "None");

            if (SelectedTea != null)
            {
                Dispatcher.Invoke(() => ButtonTea.Content = (TeaIsHQ ? "HQ" : "NQ") + ' ' + SelectedTea.Name);
                Sim.CraftsmanshipBuff += SelectedTea.FoodInfo.GetCraftsmanshipBuff(currentCraftsmanship, TeaIsHQ);
                Sim.ControlBuff += SelectedTea.FoodInfo.GetControlBuff(currentControl, TeaIsHQ);
                Sim.MaxCPBuff += SelectedTea.FoodInfo.GetMaxCPBuff(currentMaxCP, TeaIsHQ);
            }
            else
                Dispatcher.Invoke(() => ButtonTea.Content = "None");
            Sim.ExecuteActions();
        }

        private List<CraftingActionContainer> CurrentActions { get; set; } = new List<CraftingActionContainer>();
        private int OldProgress { get; set; }
        private int OldQuality { get; set; }

        private void Sim_FinishedStep(CraftingSim sim, int index)
        {
            if (index == 0)
            {
                if (CurrentActions != null)
                {
                    CurrentActions.ForEach(x => x.Dispose());
                    CurrentActions.Clear();
                }
                CurrentActions = new List<CraftingActionContainer>();
                OldProgress = 0;
                OldQuality = 0;
            }
            var currentAction = sim.CraftingActions[index];
            CraftingActionContainer container = new CraftingActionContainer(Sim, currentAction);

            container.ProgressIncreased = sim.CurrentProgress - OldProgress;
            container.QualityIncreased = sim.CurrentQuality - OldQuality;
            OldProgress = sim.CurrentProgress;
            OldQuality = sim.CurrentQuality;
            CurrentActions.Add(container);

          
        }

        private void Sim_FinishedExecution(CraftingSim sim)
        {
            Dispatcher.Invoke(() =>
            {

                if (ListViewActions.ItemsSource != null)
                {
                    var list = (ListViewActions.ItemsSource as List<CraftingActionContainer>);
                    for (int i = 0; i < list.Count; i++)
                        list[i].Dispose();
                }
                if (Sim.CraftingActionsLength > 0)

                    for (int i = 0; i < CurrentActions.Count; i++)
                    {
                        CurrentActions[i].Update();
                    }
                else CurrentActions.Clear();


                ListViewActions.ItemsSource = CurrentActions.ToList();

                if (ListViewCraftingBuffs.ItemsSource != null)
                {
                    var list = (ListViewCraftingBuffs.ItemsSource as List<CraftingBuffContainer>);
                    for (int i = 0; i < list.Count; i++)
                        list[i].Source = null;
                }
                ListViewCraftingBuffs.ItemsSource = sim.CraftingBuffs.Select(x => new CraftingBuffContainer(G.Actions[x.Name].Images[sim.CurrentRecipe.ClassJob], x)).ToList();

                if (AvailableActions != null)
                for (int i = 0; i < AvailableActions.Length; i++)
                {
                    AvailableActions[i].Update();
                }

                LabelScore.Content = "Score: " + Sim.Score.ToString("#.###");
            });
           
        }

        public void LoadRecipe(RecipeInfo recipeInfo)
        {
            Dispatcher.Invoke(() =>
            {
                ButtonSelectRecipe.Content = recipeInfo.Name;
                TextBoxRecipeLevel.Text = recipeInfo.Level.ToString();
                TextBoxSuggestedCraftsmanship.Text = recipeInfo.RequiredCraftsmanship.ToString();
                TextBoxSuggestedControl.Text = recipeInfo.RequiredControl.ToString();
                TextBoxDurability.Text = recipeInfo.Durability.ToString();
                TextBoxMaxProgress.Text = recipeInfo.MaxProgress.ToString();
                TextBoxMaxQuality.Text = recipeInfo.MaxQuality.ToString();
            });

            Dispatcher.Invoke(() =>
            {
                ListViewAvailableIncreasesProgress.ItemsSource = null;
                ListViewAvailableIncreasesQuality.ItemsSource = null;
                ListViewAvailableAddsBuff.ItemsSource = null;
                ListViewAvailableOther.ItemsSource = null;

                Sim.SetRecipe(recipeInfo);

                UpdateAvailableActions(recipeInfo);

                CollectionView view = null;
                view = (CollectionView)CollectionViewSource.GetDefaultView(ListViewAvailableIncreasesProgress.ItemsSource);
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription("ProgressIncrease", ListSortDirection.Descending));
            });

            UpdateRotationsCount();
        }

        private void UpdateAvailableActions(RecipeInfo recipeInfo)
        {
            AvailableActions = CraftingAction.CraftingActions.Values.Where(xx => xx.Level <= Sim.Level).Select(x => new CraftingActionContainer(Sim, x)).ToArray();

            ListViewAvailableIncreasesProgress.ItemsSource = AvailableActions.Where(x => x.Action.IncreasesProgress);
            ListViewAvailableIncreasesQuality.ItemsSource = AvailableActions.Where(x => x.Action.IncreasesQuality);
            ListViewAvailableAddsBuff.ItemsSource = AvailableActions.Where(x => x.Action.AddsBuff);
            ListViewAvailableOther.ItemsSource = AvailableActions.Where(x => !x.Action.IncreasesProgress && !x.Action.IncreasesQuality && !x.Action.AddsBuff);
        }

        private void ListViewActions_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ListViewActions.SelectedIndex >= 0)
            {
                CurrentActions?.Clear();
                Sim.RemoveActionAt(ListViewActions.SelectedIndex);
            }
        }

        private void ListViewAvailable_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            CraftingActionContainer item = (sender as ListView)?.SelectedItem as CraftingActionContainer;
            if (item != null)
                Sim.AddActions(true, item.Action);

        }

        private PopulationsWindow PopulationsWindow { get; set; }

        private void ButtonFindBest_Click(object sender, RoutedEventArgs e)
        {
            if (Solver == null) return;

            var actions = Sim.GetCraftingActions();
            Solver.Continue = !Solver.Continue;
            ButtonFindBest.Content = Solver.Continue ? "Stop" : "Simulate";

            int taskCount = 8;
            int chromosomeCount = 150;
            int val = 0;
            if (int.TryParse(TextBoxTaskCount.Text, out val))
                taskCount = val;
            if (int.TryParse(TextBoxChromosomeCount.Text, out val))
                chromosomeCount = val;

            if (Solver.Continue)
            {
                int timeLimit = 0;
                int iterationLimit = 0;

                int.TryParse(TextBoxTimeLimit.Text, out timeLimit);
                int.TryParse(TextBoxIterationLimit.Text, out iterationLimit);

                Solver.Start(taskCount, chromosomeCount, CheckBoxLeaveActions.IsChecked == true, timeLimit, iterationLimit);
                PopulationsWindow = new PopulationsWindow();
                PopulationsWindow.AddSolver(Solver);
                PopulationsWindow.Closed += (x, y) =>
                {
                    PopulationsWindow = null;
                };
                PopulationsWindow.Show();
            }
            else
            {
                if (PopulationsWindow != null && PopulationsWindow.ShowActivated)
                {
                    PopulationsWindow.Close();
                    PopulationsWindow = null;
                }
            }

        }

        private Task SetIterationsCount;
        private void Solver_GenerationRan(Population obj)
        {

                if (SetIterationsCount == null || SetIterationsCount.IsCompleted)
                    SetIterationsCount = Task.Factory.StartNew(() => Dispatcher.Invoke(() => LabelIterations.Content = "Iterations: " + Solver?.Iterations),
                    CancellationToken.None, TaskCreationOptions.None, PriorityScheduler.BelowNormal);
        }

        private void CopyMacroClicked(object sender, RoutedEventArgs e)
        {
            if (Sim == null) return;

            string text = "";
            var actions = Sim.GetCraftingActions();
            for (int i = 0; i < actions.Length; i++)
            {
                CraftingAction ac = actions[i];
                text += $"/ac \"{ac.Name}\" <wait.{(ac.IsBuff ? 2 : 3)}>\r\n";
            }
            if (!string.IsNullOrEmpty(text)) 
                Dispatcher.Invoke(() => Clipboard.SetDataObject(text));

        }

        private void RotationsDatabaseClicked(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonAddRotationToDatabase_Click(object sender, RoutedEventArgs e)
        {
            if (Sim == null || Sim.CurrentRecipe == null)
                return;
            Utils.AddRotationFromSim(Sim);
            UpdateRotationsCount();
        }

        private void ButtonChooseRotation_Click(object sender, RoutedEventArgs e)
        {
            if (Sim == null || Sim.CurrentRecipe == null)
                return;
            RotationsDatabaseWindow window = new RotationsDatabaseWindow();
            window.AddRotations(Sim.CurrentRecipe);
            if (window.ShowDialog() == true)
            {
                Sim.RemoveActions();
                Sim.AddActions(true, window.RotationInfo.Rotation.Array.Select(x => CraftingAction.CraftingActions[x]));
            }
        }

        public void UpdateRotationsCount()
        {
            var data = Sim.CurrentRecipe.GetAbstractData();
            if (!GameData.RecipeRotations.ContainsKey(data))
                GameData.RecipeRotations[data] = new List<RecipeSolutionInfo>();
            Dispatcher.Invoke(() => LabelRotationsInDatabase.Content = "Rotations in Database: " + GameData.RecipeRotations[data].Count);
        }


        private void ButtonEditStepSettings_Click(object sender, RoutedEventArgs e)
        {
            RecipeSettingsWindow window = new RecipeSettingsWindow();
            window.SetCraftingSim(Sim);
            window.ShowDialog();
        }

        private void ClearAllClicked(object sender, RoutedEventArgs e)
        {
            Sim.RemoveActions();
        }

        private void ButtonAddFromDatabase_Click(object sender, RoutedEventArgs e)
        {
            if (Sim == null || Sim.CurrentRecipe == null || Solver == null || Solver.Populations == null)
                return;
            var rotations = G.GetAllRotationsForRecipe(Sim.CurrentRecipe);
            ushort[][] actions = rotations.Select(x => x.Rotation.Array.ToArray()).ToArray();
            for (int i = 0; i < Solver.Populations.Length; i++)
            {
                    Solver.Populations[i].AddChromosomes(actions, true);
            }
        }

        private void ButtonAddToDatabase_Click(object sender, RoutedEventArgs e)
        {
            if (Sim == null || Sim.CurrentRecipe == null || Solver == null || Solver.Populations == null)
                return;

            List<Chromosome> uniqueValidChromosomes = new List<Chromosome>(Solver.Populations.Length * Solver.Populations[0].MaxSize);
            for (int i = 0; i < Solver.Populations.Length; i++)
            {
                for (int j = 0; j < Solver.Populations[i].Chromosomes.Length; j++)
                {
                    var current = Solver.Populations[i].Chromosomes[j];
                    if (!uniqueValidChromosomes.Contains(current))
                        uniqueValidChromosomes.Add(current);
                }
            }

            foreach (var u in uniqueValidChromosomes)
                Utils.AddRotationFromSim(u.Sim);
            UpdateRotationsCount();
        }

        private ConditionalSolvingWindow ConditionalSolvingWindow { get; set; }
        private void MenuItemOpenConditionalSolvingInterface(object sender, RoutedEventArgs e)
        {
            ConditionalSolvingWindow = new ConditionalSolvingWindow();
            ConditionalSolvingWindow.Initialize(Sim);
            ConditionalSolvingWindow.FinishedConditionExecution += ConditionalSolvingWindow_FinishedConditionExecution;
            ConditionalSolvingWindow.Closed += ConditionalSolvingWindow_Closed;
            ConditionalSolvingWindow.Show();
        }

        private void ConditionalSolvingWindow_Closed(object sender, EventArgs e)
        {
            ConditionalSolvingWindow.Closed -= ConditionalSolvingWindow_Closed;
            ConditionalSolvingWindow.FinishedConditionExecution -= ConditionalSolvingWindow_FinishedConditionExecution;
        }

        private void ConditionalSolvingWindow_FinishedConditionExecution(CraftingSimEx obj)
        {
            Sim.RemoveActions();
            for (int i = 0; i < obj.StepSettings.Length; i++)
                Sim.StepSettings[i].RecipeCondition = obj.StepSettings[i].RecipeCondition;
            Sim.AddActions(true, obj.GetCraftingActions());
        }
    }
}
