using FFXIVCraftingSim.Types.GameData;
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

namespace FFXIVCraftingSim.GUI.Windows
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


            DataGrid parent = (sender as DataGrid);
            if (parent != null && parent.SelectedValue != null)
            {
                DialogResult = true;
                SelectedRecipe = (parent.SelectedValue as RecipeInfo);
                Close();
            }
        }

        private void SearchTextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(DataGridRecipes.ItemsSource).Refresh();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            DataGridRecipes.ItemsSource = G.Recipes;
            CollectionViewSource.GetDefaultView(DataGridRecipes.ItemsSource).Filter = UserFilter;
        }

        private bool UserFilter(object item)
        {
            RecipeInfo info = (item as RecipeInfo);

            string[] args = TextBoxSearchStringName.Text.ToLower().Split(' ');

            bool e1 = String.IsNullOrEmpty(TextBoxSearchStringName.Text);
            bool e2 = String.IsNullOrEmpty(TextBoxSearchIngredient.Text);

            if (e1 && e2)
                return true;
            if (!e1)
            {
                string s = info.SearchString.ToLower();
                bool contains = args.All(x => s.Contains(x));
                if (contains)
                    return true;

            }
            if (!e2)
            {
                string ingredient = TextBoxSearchIngredient.Text.ToLower();
                if (info.Ingredients.Any(x => x.Name.ToLower() == ingredient))
                    return true;
            }

            return false;
        }
    }
}
