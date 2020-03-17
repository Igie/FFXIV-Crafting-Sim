﻿using FFXIV_Crafting_Sim.Converters;
using SaintCoinach;
using SaintCoinach.Xiv;
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
            G.Init();
            var items = G.GameData.GameData.GetSheet<Item>();

            var a = items[1].Icon.GetImage();

            img.DataContext = a;

        }
    }
}
