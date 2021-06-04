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
    public partial class EditBuildingConfigWindow : Window
    {
        private BuildingConfiguration _config;
        public BuildingConfiguration config 
        { 
            get 
            {
                _config.name = name;
                _config.height = buildingHeight;
                _config.width = buildingWidth;
                _config.distToSite = distToSite;
                _config.siteLength = siteLength;
                return _config; 
            } 
            set 
            {
                name = value.name;
                buildingHeight = value.height;
                buildingWidth = value.width;
                distToSite = value.distToSite;
                siteLength = value.siteLength;
                _config = value; 
            } 
        }
        public string name { get; set; }
        public double buildingHeight { get; set; }
        public double buildingWidth { get; set; }
        private double _distToSite;
        public double distToSite { get { return _distToSite; } set { _distToSite = Math.Max(0, value); } }
        private double _siteLength;
        public double siteLength { get { return _siteLength; } set { _siteLength = Math.Max(0, value); } }

        public EditBuildingConfigWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void OK_button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void OnFocusEvent(object sender, EventArgs e)
        {
            (sender as TextBox).SelectAll();
        }
    }
}
