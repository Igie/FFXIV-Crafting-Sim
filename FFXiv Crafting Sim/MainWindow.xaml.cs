using FFXIV_Crafting_Sim.Converters;
using FFXIV_Crafting_Sim.GUI.Windows;
using FFXIV_Crafting_Sim.Types.GameData;
using SaintCoinach;
using SaintCoinach.Xiv;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FFXIV_Crafting_Sim
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            G.Init(this);
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

        public void LoadRecipe(RecipeInfo recipeInfo)
        {
            ButtonSelectRecipe.Content = recipeInfo.Name;
            TextBoxRecipeLevel.Text = recipeInfo.Level.ToString();
            TextBoxSuggestedCraftsmanship.Text = recipeInfo.RequiredCraftsmanship.ToString();
            TextBoxSuggestedControl.Text = recipeInfo.RequiredControl.ToString();
            TextBoxDurability.Text = recipeInfo.Durability.ToString();
            TextBoxMaxProgress.Text = recipeInfo.MaxProgress.ToString();
            TextBoxMaxQuality.Text = recipeInfo.MaxQuality.ToString();
        }
    }
}
