using FFXIVCraftingSimLib;
using FFXIVCraftingSimLib.Actions;
using FFXIVCraftingSimLib.Types;
using FFXIVCraftingSimLib.Types.GameData;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace FFXIVCraftingSim.GUI.Windows
{
    /// <summary>
    /// Interaction logic for RotationsDatabaseWindow.xaml
    /// </summary>
    public partial class RotationsDatabaseWindow : Window
    {
        public RecipeSolutionInfo RotationInfo { get; private set; }
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
            
            Dispatcher.Invoke(() => {
                var rotations = GameData.RecipeRotations[AbstractRecipeInfo].Select(x => new RotationInfoContainer(x, ClassJobInfo));
                DataGridRotations.ItemsSource = rotations;
            });
        }

        private void DataGridRotations_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataGridRotations.SelectedValue == null || e.LeftButton != MouseButtonState.Pressed)
                return;
            RotationInfo = (DataGridRotations.SelectedItem as RotationInfoContainer).RotationInfo;
            if (RotationInfo == null)
                return;

            DialogResult = true;
            Close();
        }

        private void ContextMenuItemRemoveClicked(object sender, RoutedEventArgs e)
        {
            if (DataGridRotations.SelectedItem == null)
                return;
            G.RemoveRotation(AbstractRecipeInfo, (DataGridRotations.SelectedItem as RotationInfoContainer).RotationInfo);
            Dispatcher.Invoke(() => {
                var rotations = GameData.RecipeRotations[AbstractRecipeInfo].Select(x => new RotationInfoContainer(x, ClassJobInfo));
                DataGridRotations.ItemsSource = rotations;
            });
        }

        private void CheckBoxFilterStatsClicked(object sender, RoutedEventArgs e)
        {
            if (DataGridRotations.ItemsSource == null) return;
            CollectionViewSource.GetDefaultView(DataGridRotations.ItemsSource).Filter = UserFilter;
        }

        private bool UserFilter(object item)
        {
            if (CheckBoxFilterForStats.IsChecked == false)
                return true;

            int lvl = G.MainWindow.Sim.Level;
            int cft = G.MainWindow.Sim.Craftsmanship;
            int ctrl = G.MainWindow.Sim.Control;

            var cont = (item as RotationInfoContainer).RotationInfo;
            if (lvl == cont.MinLevel && cft <= cont.MaxCraftsmanship && cft >= cont.MinCraftsmanship && ctrl >= cont.MinControl)
                return true;
            return false;
        }
    }

    public class RotationInfoContainer
    {
        public RecipeSolutionInfo RotationInfo { get; private set; }

        public BitmapSourceContainer[] Images { get; set; }

        public RotationInfoContainer(RecipeSolutionInfo rotationInfo, ClassJobInfo classJobInfo)
        {
            RotationInfo = rotationInfo;
            try
            {
                Images = RotationInfo.Rotation.Array.Select(x => new BitmapSourceContainer(G.Actions[CraftingAction.CraftingActions[x].Name].Images[classJobInfo])).ToArray();
            } catch(Exception e)
            {
                Debugger.Break();
            }
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
