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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CrawlerCraneVisual.CutsomElements
{
    public partial class BuildingConfigurationItem : UserControl
    {
        private BuildingConfiguration _config;
        private BuildingConfiguration config { get { return _config; } set { _config = value; ConfigName_label.Content = value.name; } }
        private bool configSuccess;
        public BuildingConfigurationItem()
        {
            InitializeComponent();
            DataContext = this;
        }
        public BuildingConfigurationItem(BuildingConfiguration config)
        {
            InitializeComponent();
            DataContext = this;

            this.config = config;
            configSuccess = true;
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            (Parent as ListBox).Items.Remove(this);
            (Application.Current.MainWindow as MainWindow).WriteBuildingsConfigs();
        }

        public bool EditConfig()
        {
            EditButton_Click(EditButton, null);
            return configSuccess;
        }

        public BuildingConfiguration getConfig()
        {
            return config;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow w = (Application.Current.MainWindow as MainWindow);
            if (config == null)
                config = new BuildingConfiguration();
            EditBuildingConfigWindow configWindow = new EditBuildingConfigWindow();
            configWindow.Owner = Application.Current.MainWindow;
            configWindow.config = new BuildingConfiguration(config);
            if (configWindow.ShowDialog() == true)
            {
                if (config == w.buildingConfig)
                {
                    config = configWindow.config;
                    w.buildingConfig = config;
                    configSuccess = true;
                    w.WriteBuildingsConfigs();
                    w.Calculate();
                }
                else
                {
                    config = configWindow.config;
                    configSuccess = true;
                    w.WriteBuildingsConfigs();
                }
                return;
            }
            configSuccess = false;
        }
    }
}
