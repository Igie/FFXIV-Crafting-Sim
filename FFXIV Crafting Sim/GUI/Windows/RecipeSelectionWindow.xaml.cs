using FFXIV_Crafting_Sim.Types.GameData;
using System;
using System.Collections.Generic;
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

namespace FFXIV_Crafting_Sim.GUI.Windows
{
    /// <summary>
    /// Interaction logic for RecipeSelectionWindow.xaml
    /// </summary>
    public partial class RecipeSelectionWindow : Window
    {
        public RecipeSelectionWindow()
        {
            InitializeComponent();
        }

        public RecipeInfo SelectedRecipe { get; private set; }

        private void MouseDoubleClickRecipe(object sender, MouseButtonEventArgs e)
        {
            ListView parent = (sender as ListView);
            if (parent.SelectedItem != null)
            {
                DialogResult = true;
                SelectedRecipe = (parent.SelectedItem as RecipeInfo);
                Close();
            }
        }

        private void SearchTextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(ListViewRecipes.ItemsSource).Refresh();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ListViewRecipes.ItemsSource = G.Recipes;
            CollectionViewSource.GetDefaultView(ListViewRecipes.ItemsSource).Filter = UserFilter;
        }

        private bool UserFilter(object item)
        {
            RecipeInfo info = (item as RecipeInfo);

            string[] args = TextBoxSearchString.Text.Split(' ');
            if (String.IsNullOrEmpty(TextBoxSearchString.Text))
                return true;
            string s = info.SearchString;
           foreach(var arg in args)
            {
                if (s.Contains(arg))
                    return true;
            }

            return false;
        }
    }
}
