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
    /// Interaction logic for FoodSelectionWindow.xaml
    /// </summary>
    public partial class FoodSelectionWindow : Window
    {
        public bool IsHQ { get; private set; } = false;
        public ItemInfo SelectedItem { get; private set; }
        public FoodSelectionWindow()
        {
            InitializeComponent();
        }

        private void MouseDoubleClickFood(object sender, MouseButtonEventArgs e)
        {
            ListView parent = (sender as ListView);
            if (parent.SelectedItem != null)
            {
                DialogResult = true;
                SelectedItem = (parent.SelectedItem as ItemInfo);
                Close();
            }
        }


        private void SearchTextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(ListViewFood.ItemsSource).Refresh();
        }

        public void LoadList(IEnumerable<ItemInfo> list)
        {
            ListViewFood.ItemsSource = list;
            CollectionViewSource.GetDefaultView(ListViewFood.ItemsSource).Filter = UserFilter;
        }

        private bool UserFilter(object item)
        {
            ItemInfo info = (item as ItemInfo);

            string[] args = TextBoxSearchString.Text.ToLower().Split(' ');
            if (String.IsNullOrEmpty(TextBoxSearchString.Text))
                return true;
            string s = info.SearchString.ToLower();
            foreach (var arg in args)
            {
                if (s.Contains(arg))
                    return true;
            }

            return false;
        }

        private void CheckBoxHQChanged(object sender, RoutedEventArgs e)
        {
            IsHQ = CheckBoxHQ.IsChecked == true;
        }

        private void ButtonRemoveFood_Click(object sender, RoutedEventArgs e)
        {
            SelectedItem = null;
            DialogResult = true;
            Close();
        }
    }
}
