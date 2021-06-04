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
    public partial class PointItem : UserControl
    {
        private Vector _point;
        public Vector point { get { return _point; } set { _point = value; X_label.Content = value.X; Y_label.Content = value.Y; } }
        public PointItem()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
