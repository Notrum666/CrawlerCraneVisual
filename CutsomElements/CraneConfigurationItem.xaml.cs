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
    public partial class CraneConfigurationItem : UserControl
    {
        private CraneConfiguration _config;
        private CraneConfiguration config { get { return _config; } set { _config = value; ConfigName_label.Content = value.name; } }
        private bool configSuccess;
        public CraneConfigurationItem()
        {
            InitializeComponent();
            DataContext = this;
        }

        public CraneConfigurationItem(CraneConfiguration config)
        {
            InitializeComponent();
            DataContext = this;

            this.config = config;
            configSuccess = true;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            (Parent as ListBox).Items.Remove(this);
            (Application.Current.MainWindow as MainWindow).WriteCranesConfigs();
        }

        public bool EditConfig()
        {
            EditButton_Click(EditButton, null);
            return configSuccess;
        }

        public CraneConfiguration getConfig()
        {
            return config;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow w = (Application.Current.MainWindow as MainWindow);
            if (config == null)
                config = new CraneConfiguration_Default();
            EditCraneConfigWindow configWindow = new EditCraneConfigWindow();
            configWindow.Owner = Application.Current.MainWindow;
            if (config is CraneConfiguration_Default)
                configWindow.config = new CraneConfiguration_Default(config as CraneConfiguration_Default);
            else if (config is CraneConfiguration_Tower)
                configWindow.config = new CraneConfiguration_Tower(config as CraneConfiguration_Tower);
            configWindow.updateEditWindow();
            if (configWindow.ShowDialog() == true)
            {
                if (config == w.craneConfig)
                {
                    config = configWindow.config;
                    w.craneConfig = config;
                    configSuccess = true;
                    w.WriteCranesConfigs();
                    w.Calculate();
                }
                else
                {
                    config = configWindow.config;
                    configSuccess = true;
                    w.WriteCranesConfigs();
                }
                return;
            }
            configSuccess = false;
        }
    }
}
