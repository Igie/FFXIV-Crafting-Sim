using FFXIVCraftingSim.Solving;
using FFXIVCraftingSim.Solving.GeneticAlgorithm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// Interaction logic for PopulationsWindow.xaml
    /// </summary>
    public partial class PopulationsWindow : Window
    {
        private ObservableCollection<PopulationContainer> Populations { get; set; }

        private string SortProperty { get; set; }
        private ListSortDirection OldSortDirection { get; set; }

        public PopulationsWindow()
        {
            InitializeComponent();
        }


        public void AddSolver(Solver solver)
        {
            DataContext = solver;
            Populations = new ObservableCollection<PopulationContainer>(solver.Populations.Select(x => new PopulationContainer(x)));

            for (int i = 0; i < Populations.Count; i++)
                Populations[i].PropertyChanged += PopulationsWindow_PropertyChanged;

            ListViewPopulations.ItemsSource = Populations;
            OldSortDirection = ListSortDirection.Ascending;
        }

        private void PopulationsWindow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //Dispatcher.Invoke(() =>
            //ListViewPopulations.Items.Refresh());
        }

        private void GridViewColumnHeaderClicked(object sender, RoutedEventArgs e)
        {
            string sortProperty = ((sender as GridViewColumnHeader).Column.DisplayMemberBinding as Binding).Path.Path;

            if (SortProperty == sortProperty)
                if (OldSortDirection == ListSortDirection.Ascending)
                    OldSortDirection = ListSortDirection.Descending;
                else
                    OldSortDirection = ListSortDirection.Ascending;
            else
                OldSortDirection = ListSortDirection.Descending;
            SortProperty = sortProperty;
            ListViewPopulations.Items.SortDescriptions.Clear();
            ListViewPopulations.Items.SortDescriptions.Add(new SortDescription(SortProperty, OldSortDirection));
            ListViewPopulations.Items.IsLiveSorting = true;
        }
    }

    public class PopulationContainer : INotifyPropertyChanged
    {
        public Population Population { get; private set; }

        public int Index => Population.Index;
        public double BestScore => Math.Round(Population.Best.Fitness, 3);
        public int CurrentGeneration => Population.CurrentGeneration;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private PropertyChangedEventArgs BestScoreChanged, CurrentGenChanged;

        public PopulationContainer(Population population)
        {
            Population = population;

            BestScoreChanged = new PropertyChangedEventArgs("BestScore");
            CurrentGenChanged = new PropertyChangedEventArgs("CurrentGeneration");

            Population.GenerationRan += Population_GenerationRan;
        }

        private void Population_GenerationRan(Population obj)
        {
            PropertyChanged(this, BestScoreChanged);
            PropertyChanged(this, CurrentGenChanged);
        }
    }
}
