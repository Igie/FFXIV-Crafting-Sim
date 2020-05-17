using FFXIVCraftingSim.Actions;
using FFXIVCraftingSim.GUI;
using FFXIVCraftingSim.GUI.Windows;
using FFXIVCraftingSim.Solving;
using FFXIVCraftingSim.Stream;
using FFXIVCraftingSim.Types;
using FFXIVCraftingSim.Types.GameData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        private CraftingSim Sim { get; set; }

        private CraftingActionContainer[] AvailableActions { get; set; }

        private bool FoodIsHQ { get; set; }
        private bool TeaIsHQ { get; set; }
        private ItemInfo SelectedFood { get; set; }
        private ItemInfo SelectedTea { get; set; }

        private GASolver Solver { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void TextChangedAllowNumericOnly(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (sender == null) return;
            string text = textBox.Text;
            textBox.Text = string.Concat(text.Where(x => char.IsDigit(x)));

            PlayerStatsFromTextToSim();
            UpdateCraftingText();
            if (Sim != null)
                Sim.ExecuteActions();
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.UnhandledException += new DispatcherUnhandledExceptionEventHandler((object s, DispatcherUnhandledExceptionEventArgs ee) =>
               {
                   Debugger.Log(0, "Error", ee.ToString());
               });

            Sim = new CraftingSim();
            Sim.FinishedStep += Sim_FinishedStep;
            Sim.FinishedExecution += Sim_FinishedExecution;



            G.Loaded += G_Loaded;
            G.Init(this);
        }



        private void G_Loaded()
        {
            try
            {
                if (File.Exists("Settings.db"))
                {
                    DataStream s = new DataStream(File.ReadAllBytes("Settings.db"));
                    int recipeId = s.ReadInt();


                    Dispatcher.Invoke(() =>
                    {
                        TextBoxCrafterLevel.Text = s.ReadInt().ToString();
                        TextBoxCrafterCraftsmanship.Text = s.ReadInt().ToString();
                        TextBoxCrafterControl.Text = s.ReadInt().ToString();
                        TextBoxCrafterMaxCP.Text = s.ReadInt().ToString();
                    });
                    Dispatcher.Invoke(() => {
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

                    PlayerStatsFromTextToSim();
                    LoadRecipe(G.Recipes.FirstOrDefault(x => x.Id == recipeId));

                    Sim.AddActions(actions);
                    s.Flush();
                    s.Close();
                }

                if (Solver == null)
                {
                    var actions = CraftingAction.CraftingActions.Select(x => (ushort)x.Key).ToList();
                    Solver = new GASolver(Sim, actions.ToArray());
                    Solver.GenerationRan += Solver_GenerationRan;
                }
            }
            catch (Exception ee)
            {
                Debugger.Break();
            }
        }



        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (File.Exists("Settings.db"))
                    File.Delete("Settings.db");

                G.WriteRecipeRotations();
                if (Sim == null || Sim.CurrentRecipe == null)
                    return;
                DataStream s = new DataStream();
                Sim.CraftsmanshipBuff = 0;
                Sim.ControlBuff = 0;
                Sim.MaxCPBuff = 0;
                s.WriteInt(Sim.CurrentRecipe.Id);
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
                UpdateCraftingText();
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
            UpdatePlayerStatsText();
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
            CraftingActionContainer container = new CraftingActionContainer(G.Actions[currentAction.Name].Images[sim.CurrentRecipe.ClassJob], Sim, currentAction);

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
            });
            for (int i = 0; i < CurrentActions.Count; i++)
            {
                CurrentActions[i].Update();
            }
            UpdateCraftingText();
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
                ProgressBarProgress.Minimum = 0;
                ProgressBarProgress.Maximum = recipeInfo.MaxProgress;
                ProgressBarProgress.Value = 0;

                ProgressBarQuality.Minimum = 0;
                ProgressBarQuality.Maximum = recipeInfo.MaxQuality;
                ProgressBarQuality.Value = 0;

                ListViewAvailableIncreasesProgress.ItemsSource = null;
                ListViewAvailableIncreasesQuality.ItemsSource = null;
                ListViewAvailableAddsBuff.ItemsSource = null;
                ListViewAvailableOther.ItemsSource = null;

                Sim.SetRecipe(recipeInfo);

                AvailableActions = CraftingAction.CraftingActions.Values.Select(x => new CraftingActionContainer(G.Actions[x.Name].Images[recipeInfo.ClassJob], Sim, x)).ToArray();

                ListViewAvailableIncreasesProgress.ItemsSource = AvailableActions.Where(x => x.Action.IncreasesProgress);
                ListViewAvailableIncreasesQuality.ItemsSource = AvailableActions.Where(x => x.Action.IncreasesQuality);
                ListViewAvailableAddsBuff.ItemsSource = AvailableActions.Where(x => x.Action.AddsBuff);
                ListViewAvailableOther.ItemsSource = AvailableActions.Where(x => !x.Action.IncreasesProgress && !x.Action.IncreasesQuality && !x.Action.AddsBuff);

                CollectionView view = null;
                view = (CollectionView)CollectionViewSource.GetDefaultView(ListViewAvailableIncreasesProgress.ItemsSource);
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new System.ComponentModel.SortDescription("ProgressIncrease", System.ComponentModel.ListSortDirection.Descending));
            });

            UpdateRotationsCount();

           
        }
        public void PlayerStatsFromTextToSim()
        {
            Dispatcher.Invoke(() =>
            {
                int val;
                if (int.TryParse(TextBoxCrafterLevel.Text, out val))
                {
                    Sim.Level = val;
                }
                if (int.TryParse(TextBoxCrafterCraftsmanship.Text, out val))
                    Sim.BaseCraftsmanship = val;

                if (int.TryParse(TextBoxCrafterControl.Text, out val))
                {
                    Sim.BaseControl = val;
                }

                if (int.TryParse(TextBoxCrafterMaxCP.Text, out val))
                {
                    Sim.BaseMaxCP = val;
                    Sim.CurrentCP = Sim.MaxCP;
                    LabelCP.Content = $"CP: {Sim.CurrentCP}/{Sim.MaxCP}";
                }
            });
        }

        public void UpdatePlayerStatsText()
        {
            Dispatcher.Invoke(() =>
            {
                TextBoxCrafterCraftsmanship.Text = Sim.BaseCraftsmanship.ToString();
                TextBoxCrafterControl.Text = Sim.BaseControl.ToString();
                TextBoxCrafterMaxCP.Text = Sim.BaseMaxCP.ToString();

                TextBoxCrafterCraftsmanshipBuff.Text = '+' + Sim.CraftsmanshipBuff.ToString();
                TextBoxCrafterControlBuff.Text = '+' + Sim.ControlBuff.ToString();
                TextBoxCrafterMaxCPBuff.Text = '+' + Sim.MaxCPBuff.ToString();
            });
        }

        public void UpdateCraftingText()
        {
            if (Sim.CurrentRecipe == null)
                return;
            Dispatcher.Invoke(() =>
            {
                LabelName.Content = $"{Sim.CurrentRecipe.Name}";
                LabelDurability.Content = $"Durability: {Sim.CurrentDurability}/{Sim.CurrentRecipe.Durability}";
                LabelCP.Content = $"CP: {Sim.CurrentCP}/{Sim.MaxCP}";
                LabelProgress.Content = $"{Sim.CurrentProgress}/{Sim.CurrentRecipe.MaxProgress} ({Sim.GetProgressIncrease(1)} at 100%)";
                ProgressBarProgress.Value = Sim.CurrentProgress;
                LabelQuality.Content = $"{Sim.CurrentQuality}/{Sim.CurrentRecipe.MaxQuality} ({Sim.GetQualityIncrease(1)} at 100%)";
                ProgressBarQuality.Value = Sim.CurrentQuality;
                LabelScore.Content = "Score: " + Sim.Score.ToString("#.##");
            });
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
                Sim.AddActions(item.Action);

        }

        private PopulationsWindow PopulationsWindow { get; set; }

        private void ButtonFindBest_Click(object sender, RoutedEventArgs e)
        {
            if (Solver == null) return;

            //StepSolver solver = new StepSolver();
            //solver.Solve(Sim);
            //Sim.RemoveActions();
            
            //Sim.AddActions(solver.Sim.GetCraftingActions());
            //return;
            Solver.Continue = !Solver.Continue;
            ButtonFindBest.Content = Solver.Continue ? "Stop" : "Find Best";

            int taskCount = 8;
            int chromosomeCount = 150;
            int val = 0;
            if (int.TryParse(TextBoxTaskCount.Text, out val))
                taskCount = val;
            if (int.TryParse(TextBoxChromosomeCount.Text, out val))
                chromosomeCount = val;

            if (Solver.Continue)
            {
                if (sender == ButtonFindBest)
                    Solver.Start(taskCount, chromosomeCount, false);
                else
                    Solver.Start(taskCount, chromosomeCount, true);
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

        private void Solver_GenerationRan(Solving.GeneticAlgorithm.Population obj)
        {
            Dispatcher.Invoke(() =>
            {
                LabelIterations.Content = "Iterations: " + Solver?.Iterations;
            });
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
            Dispatcher.Invoke(() => Clipboard.SetDataObject(text));

        }

        private void RotationsDatabaseClicked(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonAddRotationToDatabase_Click(object sender, RoutedEventArgs e)
        {
            if (Sim == null || Sim.CurrentRecipe == null)
                return;
            G.AddRotationFromSim(Sim);
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
                Sim.AddActions(window.RotationInfo.Rotation.Array.Select(x => CraftingAction.CraftingActions[x]));
            }
        }

        public void UpdateRotationsCount()
        {
            if (!G.RecipeRotations.ContainsKey(Sim.CurrentRecipe.GetAbstractData()))
                Debugger.Break();
            Dispatcher.Invoke(() => LabelRotationsInDatabase.Content = "Rotations in Database: " + G.RecipeRotations[Sim.CurrentRecipe.GetAbstractData()].Count);
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

        private void MenuItemOpenRotationManagerClicked(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonAddFromDatabase_Click(object sender, RoutedEventArgs e)
        {
            if (Sim == null || Sim.CurrentRecipe == null || Solver == null)
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
            if (Sim == null || Sim.CurrentRecipe == null || Solver == null)
                return;

            for (int i = 0; i < Solver.Populations.Length; i++)
            {
                for (int j = 0; j < Solver.Populations[i].Chromosomes.Length; j++)
                {
                    G.AddRotationFromSim(Solver.Populations[i].Chromosomes[j].Sim);
                }
            }
        }
    }
}
