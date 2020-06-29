using FFXIVCraftingSimLib.Types;
using System.Windows;
using System.Windows.Controls;

namespace FFXIVCraftingSim.GUI.Windows
{
    /// <summary>
    /// Interaction logic for RecipeSettingsWindow.xaml
    /// </summary>
    public partial class RecipeSettingsWindow : Window
    {
        private CraftingSimEx Sim { get; set; }
        private CraftingSimStepSettings[] Settings { get; set; }

        private string[] Names = new string[] { "Condition" };

        public RecipeSettingsWindow()
        {
            InitializeComponent();
        }

        public void SetCraftingSim(CraftingSimEx sim)
        {
            Sim = sim;
            Settings = sim.StepSettings;
            var actions = Sim.GetCraftingActions();

            StepSettingsContainer[] source = new StepSettingsContainer[Settings.Length];
            for (int i = 0; i < Settings.Length; i++)
            {
                source[i] = new StepSettingsContainer {
                    Step = i,
                    Setting = Settings[i],
                    ActionName = "None"
                };
            }

            for (int i = 0; i < actions.Length; i++)
                source[i].ActionName = actions[i].Name;

            ListViewSettings.ItemsSource = source;
        }

        private void ApplyClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            //Sim.SetStepSetting(Settings.CurrentStep, Settings);
            Close();

            Sim.ExecuteActions();
        }

        private void RemoveClicked(object sender, RoutedEventArgs e)
        {
            //Sim.SetStepSetting(Settings.CurrentStep, null);
            DialogResult = false;
            Close();

            Sim.ExecuteActions();
        }

        private void ConditionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }
    }

    public class StepSettingsContainer
    {
        public int Step { get; set; }
        public string ActionName { get; set; }

        public RecipeCondition Property
        {
            get
            {
                return Setting.RecipeCondition;
            }

            set
            {
                Setting.RecipeCondition = value;
            }
        }
        public CraftingSimStepSettings Setting { get; set; }
    }
}
