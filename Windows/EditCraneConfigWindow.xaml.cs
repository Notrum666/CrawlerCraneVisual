using CrawlerCraneVisual.CutsomElements;
using CrawlerCraneVisual.Windows;
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

namespace CrawlerCraneVisual
{
    public partial class EditCraneConfigWindow : Window
    {
        private CraneConfiguration _config;
        public CraneConfiguration config 
        { 
            get 
            {
                _config.name = name;
                _config.l1 = length1;
                _config.l2 = length2;
                _config.height = height;
                _config.distToL1 = distToL1;
                _config.distToBack = distToBack;
                _config.totalWidth = totalWidth;
                _config.mainCapacity = mainCapacity;
                if (_config is CraneConfiguration_Default)
                {
                    (_config as CraneConfiguration_Default).subsidiaryCapacity = subsidiaryCapacity;
                    (_config as CraneConfiguration_Default).l1_l2_angle = l1_l2_angle * Math.PI / 180.0;
                }
                else if (_config is CraneConfiguration_Tower)
                {
                    (_config as CraneConfiguration_Tower).l2_minAngle = l2_minAngle * Math.PI / 180.0;
                    (_config as CraneConfiguration_Tower).l1_angle = l1_angle * Math.PI / 180.0;
                }
                return _config; 
            } 
            set 
            {
                _config = value;
                name = value.name;
                length1 = value.l1;
                length2 = value.l2;
                height = value.height;
                distToL1 = value.distToL1;
                distToBack = value.distToBack;
                totalWidth = value.totalWidth;
                mainCapacity = value.mainCapacity;
                if (value is CraneConfiguration_Default)
                {
                    subsidiaryCapacity = (value as CraneConfiguration_Default).subsidiaryCapacity;
                    l1_l2_angle = (value as CraneConfiguration_Default).l1_l2_angle / Math.PI * 180.0;
                }
                else if (value is CraneConfiguration_Tower)
                {
                    l2_minAngle = (value as CraneConfiguration_Tower).l2_minAngle / Math.PI * 180.0;
                    l1_angle = (value as CraneConfiguration_Tower).l1_angle / Math.PI * 180.0;
                }
            } 
        }
        public string name { get; set; }
        public double length1 { get; set; }
        public double length2 { get; set; }
        public double l1_angle { get; set; }
        public double l1_l2_angle { get; set; }
        public double l2_minAngle { get; set; }
        public double height { get; set; }
        public double distToL1 { get; set; }
        public double distToBack { get; set; }
        public double totalWidth { get; set; }
        public CapacityFunction mainCapacity { get; set; }
        public CapacityFunction subsidiaryCapacity { get; set; }
        public EditCraneConfigWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void OK_button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public void updateEditWindow()
        {
            if (config is CraneConfiguration_Default)
            {
                Grid_Tower.IsEnabled = false;
                Grid_Tower.Visibility = Visibility.Hidden;
                Grid_Default.IsEnabled = true;
                Grid_Default.Visibility = Visibility.Visible;
                if (CraneTypeSelection.SelectedIndex != 0)
                    CraneTypeSelection.SelectedIndex = 0;
                TextBox_Default_height.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                TextBox_Default_distToL1.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                TextBox_Default_distToBack.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                TextBox_Default_totalWidth.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                TextBox_Default_length1.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                TextBox_Default_length2.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                TextBox_Default_l1_l2_angle.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                TextBox_Default_name.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            }
            else if (config is CraneConfiguration_Tower)
            {
                Grid_Tower.IsEnabled = true;
                Grid_Tower.Visibility = Visibility.Visible;
                Grid_Default.IsEnabled = false;
                Grid_Default.Visibility = Visibility.Hidden;
                if (CraneTypeSelection.SelectedIndex != 1)
                    CraneTypeSelection.SelectedIndex = 1;
                TextBox_Tower_height.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                TextBox_Tower_distToL1.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                TextBox_Tower_distToBack.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                TextBox_Tower_totalWidth.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                TextBox_Tower_l1_angle.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                TextBox_Tower_l2_minAngle.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                TextBox_Tower_length1.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                TextBox_Tower_length2.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                TextBox_Tower_name.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            }
        }

        private void CraneTypeSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((Grid_Default.IsEnabled && CraneTypeSelection.SelectedIndex == 0) || (Grid_Tower.IsEnabled && CraneTypeSelection.SelectedIndex == 1))
                return;
            switch ((CraneTypeSelection.SelectedItem as ComboBoxItem).Name)
            {
                case "Default":
                    config = new CraneConfiguration_Default();
                    break;
                case "Tower":
                    config = new CraneConfiguration_Tower();
                    break;
            }
            updateEditWindow();
        }

        private void EditMainFunction_button_Click(object sender, RoutedEventArgs e)
        {
            EditCapacityFunctionWindow editWindow = new EditCapacityFunctionWindow();
            editWindow.Owner = this;
            editWindow.capacityFunc = new CapacityFunction(mainCapacity);

            Vector[] points = editWindow.capacityFunc.getPoints();
            if (points.Length > 0)
                for (int i = 0; i < points.Length; i++)
                {
                    PointItem newPoint = new PointItem();
                    newPoint.point = points[i];
                    editWindow.PointsList.Items.Add(newPoint);
                }

            if (editWindow.ShowDialog() == true)
                mainCapacity = editWindow.capacityFunc;
        }

        private void EditSubsidiaryFunction_button_Click(object sender, RoutedEventArgs e)
        {
            EditCapacityFunctionWindow editWindow = new EditCapacityFunctionWindow();
            editWindow.Owner = this;
            editWindow.capacityFunc = new CapacityFunction(subsidiaryCapacity);

            Vector[] points = editWindow.capacityFunc.getPoints();
            if (points.Length > 0)
                for (int i = 0; i < points.Length; i++)
                {
                    PointItem newPoint = new PointItem();
                    newPoint.point = points[i];
                    editWindow.PointsList.Items.Add(newPoint);
                }

            if (editWindow.ShowDialog() == true)
                subsidiaryCapacity = editWindow.capacityFunc;
        }

        private void OnFocusEvent(object sender, EventArgs e)
        {
            (sender as TextBox).SelectAll();
        }
    }
}
