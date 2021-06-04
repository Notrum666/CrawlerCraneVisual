using CrawlerCraneVisual.CutsomElements;
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
    public partial class EditCapacityFunctionWindow : Window
    {
        public CapacityFunction capacityFunc { get; set; }
        public EditCapacityFunctionWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void OK_button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void AddPoint_button_Click(object sender, RoutedEventArgs e)
        {
            EditPointWindow window = new EditPointWindow();
            window.Owner = this;
            if (window.ShowDialog() == true)
            {
                int pos = capacityFunc.addPoint(window.point);
                if (pos == -1)
                    MessageBox.Show("Эта точка уже существует.", "Ошибка", MessageBoxButton.OK);
                else
                {
                    PointItem newPoint = new PointItem();
                    newPoint.point = window.point;
                    PointsList.Items.Insert(pos, newPoint);
                }
            }
        }

        private void DeletePoint_button_Click(object sender, RoutedEventArgs e)
        {
            if (PointsList.SelectedItem != null)
            {
                capacityFunc.removePoint((PointsList.SelectedItem as PointItem).point);
                PointsList.Items.Remove(PointsList.SelectedItem);
            }
        }
    }
}
