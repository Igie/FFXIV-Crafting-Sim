using FFXIVCraftingSim.Actions;
using FFXIVCraftingSim.Types;
using FFXIVCraftingSim.Types.GameData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Threading;

namespace FFXIVCraftingSim.GUI.Windows
{
    /// <summary>
    /// Interaction logic for RotationsDatabaseWindow.xaml
    /// </summary>
    public partial class RotationsDatabaseWindow : Window
    {
        public RotationInfo RotationInfo { get; private set; }
        private AbstractRecipeInfo AbstractRecipeInfo { get; set; }

        public ClassJobInfo ClassJobInfo { get; private set; }

        public RotationsDatabaseWindow()
        {
            InitializeComponent();
        }

        public void AddRotations(RecipeInfo recipeInfo)
        {
           
            ClassJobInfo = recipeInfo.ClassJob;
            AbstractRecipeInfo = recipeInfo.GetAbstractData();
            
             if (!Dispatcher.CheckAccess())
                Debugger.Break();
            Dispatcher.Invoke(() => {
                var rotations = G.RecipeRotations[AbstractRecipeInfo].Select(x => new RotationInfoContainer(x, ClassJobInfo));
                ListViewRotations.ItemsSource = rotations;
            });
        }

        private void ListViewRotations_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListViewRotations.SelectedValue == null)
                return;
            RotationInfo = (ListViewRotations.SelectedItem as RotationInfoContainer).RotationInfo;
            if (RotationInfo == null)
                return;

            DialogResult = true;
            Close();
        }

        private void ContextMenuItemRemoveClicked(object sender, RoutedEventArgs e)
        {
            if (ListViewRotations.SelectedItem == null)
                return;
            G.RemoveRotation(AbstractRecipeInfo, (ListViewRotations.SelectedItem as RotationInfoContainer).RotationInfo);
            var rotations = G.RecipeRotations[AbstractRecipeInfo];
            ListViewRotations.ItemsSource = null;
            ListViewRotations.ItemsSource = rotations;
            ListViewRotations.Items.Refresh();
        }

        private void ListBoxLoaded(object sender, RoutedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox == null)
                return;
            var images = (listBox.DataContext as RotationInfoContainer).Images;
            if (!Dispatcher.CheckAccess())
                Debugger.Break();
            listBox.Dispatcher.Invoke(() =>
            {

                //listBox.ItemsSource = (listBox.DataContext as RotationInfoContainer).Images;
            });

        }
    }

    public class RotationInfoContainer
    {
        public RotationInfo RotationInfo { get; private set; }

        public BitmapSourceContainer[] Images { get; set; }

        public RotationInfoContainer(RotationInfo rotationInfo, ClassJobInfo classJobInfo)
        {
            RotationInfo = rotationInfo;
            Images = RotationInfo.Rotation.Array.Select(x=> new BitmapSourceContainer(G.Actions[CraftingAction.CraftingActions[x].Name].Images[classJobInfo])).ToArray();
        }
    }

    public class BitmapSourceContainer
    {
        public BitmapSource BitmapSource { get; private set; }

        public BitmapSourceContainer(BitmapSource bitmapSource)
        {
            BitmapSource = bitmapSource;
        }
    }
}
