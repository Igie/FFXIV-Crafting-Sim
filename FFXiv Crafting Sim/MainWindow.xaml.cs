using FFXIV_Crafting_Sim.Actions;
using FFXIV_Crafting_Sim.Converters;
using FFXIV_Crafting_Sim.GUI;
using FFXIV_Crafting_Sim.GUI.Windows;
using FFXIV_Crafting_Sim.Solving;
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FFXIV_Crafting_Sim
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CraftingSim Sim { get; set; }

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
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Sim = new CraftingSim();
            Sim.FinishedStep += Sim_FinishedStep;
            Sim.FinishedExecution += Sim_FinishedExecution;

            G.Loaded += G_Loaded;
            G.Init(this);
        }

        

        private void G_Loaded()
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
                List<CraftingAction> actions = new List<CraftingAction>();
                while (s.Position < s.Length)
                {
                    actions.Add(CraftingAction.CraftingActions[s.ReadInt()]);
                }

                PlayerStatsFromTextToSim();
                LoadRecipe(G.Recipes.FirstOrDefault(x => x.Id == recipeId));

                Sim.AddActions(actions);
                s.Flush();
                s.Close();
            }


            //G.Loaded -= G_Loaded;
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (File.Exists("Settings.db"))
                File.Delete("Settings.db");
            if (Sim == null || Sim.CurrentRecipe == null)
                return;
            DataStream s = new DataStream();
            s.WriteInt(Sim.CurrentRecipe.Id);
            s.WriteInt(Sim.Level);
            s.WriteInt(Sim.Craftsmanship);
            s.WriteInt(Sim.Control);
            s.WriteInt(Sim.MaxCP);
            var actions = Sim.GetCraftingActions();
            foreach (var action in actions)
                s.WriteInt(action.Id);
            File.WriteAllBytes("Settings.db", s.GetBytes());
            s.Flush();
            s.Close();
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
            CraftingActionContainer container = new CraftingActionContainer(G.Actions[currentAction.Name].Images[sim.CurrentRecipe.ClassJob], currentAction);

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
                    (ListViewActions.ItemsSource as List<CraftingActionContainer>).ForEach(x => x.Dispose());
                ListViewActions.ItemsSource = CurrentActions.ToList();

                if (ListViewCraftingBuffs.ItemsSource != null)
                    (ListViewCraftingBuffs.ItemsSource as List<CraftingBuffContainer>).ForEach(x => x.Source = null);
                ListViewCraftingBuffs.ItemsSource = sim.CraftingBuffs.Select(x => new CraftingBuffContainer(G.Actions[x.Name].Images[sim.CurrentRecipe.ClassJob], x)).ToList();
            }); 
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
                ListViewActions.ItemsSource = null;
                Sim.SetRecipe(recipeInfo);

                var availableActions = CraftingAction.CraftingActions.Values.Select(x => new CraftingActionContainer(G.Actions[x.Name].Images[recipeInfo.ClassJob], x)).ToArray();

                ListViewAvailable.ItemsSource = availableActions;
            });
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
                    Sim.Craftsmanship = val;

                if (int.TryParse(TextBoxCrafterControl.Text, out val))
                {
                    Sim.Control = val;
                }

                if (int.TryParse(TextBoxCrafterMaxCP.Text, out val))
                {
                    Sim.MaxCP = val;
                    Sim.CurrentCP = val;
                    LabelCP.Content = $"CP: {Sim.CurrentCP}/{Sim.MaxCP}";
                }
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
                LabelScore.Content = "Score: " + Sim.Score;
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

            CraftingActionContainer item = ListViewAvailable.SelectedItem as CraftingActionContainer;
            if (item != null)
                Sim.AddActions(item.Action);

        }

        private void ButtonFindBest_Click(object sender, RoutedEventArgs e)
        {
            CraftingActionSequence seq = new CraftingActionSequence();

            PossibleCraftingAction first = CraftingAction.CraftingActions.Where(x => x.Value.AsFirstActionOnly).Select(x => x.Value.Id).ToArray();
            PossibleCraftingAction nonFirst = CraftingAction.CraftingActions.Where(x => !x.Value.AsFirstActionOnly).Select(x => x.Value.Id).ToArray();


            seq.AddPossibleAction(first);
            for (int i = 0; i < 10; i++)
                seq.AddPossibleAction(nonFirst);

            int possibilities = seq.GetPossibilities();

            int[][] actions = seq.PossibleIds.Select(x => x.Ids.ToArray()).ToArray();
            CraftingAction[][] ac = seq.PossibleIds.Select(x => x.Ids.Select(xx => CraftingAction.CraftingActions[xx]).ToArray()).ToArray();
            ArrayCounter<CraftingAction> counter = new ArrayCounter<CraftingAction>(ac);
            Sim.RemoveActions();
            Sim.AddActions(counter.Current);
            double bestScore = Sim.Score;
            var currentActions = counter.Current;
            Task.Run(() =>
            {
                do
                {
                    Sim.RemoveActions();
                    Sim.AddActions(counter.Current);
                    double score = Sim.Score;
                    if (bestScore < score)
                    {
                        bestScore = score;
                        currentActions = counter.Current;
                    }
                } while (counter.Increase());
            });

        }
    }
}
