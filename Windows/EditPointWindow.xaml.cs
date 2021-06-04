using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CrawlerCraneVisual.Windows
{
    public partial class EditPointWindow : Window
    {
        public Vector point;
        public double X { get; set; }
        public double Y { get; set; }
        public EditPointWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void OK_button_Click(object sender, RoutedEventArgs e)
        {
            point.X = X;
            point.Y = Y;
            DialogResult = true;
        }
    }
}
