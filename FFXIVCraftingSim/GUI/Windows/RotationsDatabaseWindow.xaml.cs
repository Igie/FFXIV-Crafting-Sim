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
                var rotations = G.RecipeRotations[AbstractRecipeInfo].Select(x => new RotationInfoContainer(x, ClassJobInfo));
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
                var rotations = G.RecipeRotations[AbstractRecipeInfo].Select(x => new RotationInfoContainer(x, ClassJobInfo));
                DataGridRotations.ItemsSource = rotations;
            });
        }

        private void FilterForCurrentStatsClicked(object sender, RoutedEventArgs e)
        {

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
