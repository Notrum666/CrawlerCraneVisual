using CrawlerCraneVisual.CutsomElements;
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
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

namespace CrawlerCraneVisual
{
    [Serializable]
    public class CapacityFunction
    {
        private List<Vector> Points = new List<Vector>();
        public CapacityFunction(params Vector[] points)
        {
            for (int i = 0; i < points.Length; i++)
                addPoint(points[i]);
        }
        public CapacityFunction(CapacityFunction func)
        {
            for (int i = 0; i < func.Points.Count; i++)
                Points.Add(new Vector(func.Points[i].X, func.Points[i].Y));
        }
        public double Calculate(double x)
        {
            if (Points.Count < 2)
                return double.NaN;
            if (x < Points[0].X)
                return Math.Max(Points[0].Y + (Points[1].Y - Points[0].Y) * (x - Points[0].X) / (Points[1].X - Points[0].X), 0.0);
            int c = Points.Count;
            if (x > Points[c - 1].X)
                return Math.Max(Points[c - 2].Y + (Points[c - 1].Y - Points[c - 2].Y) * (x - Points[c - 2].X) / (Points[c - 1].X - Points[c - 2].X), 0.0);
            for (int i = 0; i < c - 1; i++)
                if (x >= Points[i].X && x <= Points[i + 1].X)
                    return Math.Max(Points[i].Y + (Points[i + 1].Y - Points[i].Y) * (x - Points[i].X) / (Points[i + 1].X - Points[i].X), 0.0);
            return double.NaN;
        }
        public int addPoint(Vector p)
        {
            if (Points.Count == 0)
            {
                Points.Add(p);
                return 0;
            }
            for (int i = 0; i < Points.Count; i++)
                if (p.X == Points[i].X)
                    return -1;
            for (int i = 0; i < Points.Count; i++)
                if (p.X < Points[i].X)
                {
                    Points.Insert(i, p);
                    return i;
                }
            Points.Add(p);
            return Points.Count - 1;
        }
        public void removePoint(Vector p)
        {
            Points.Remove(p);
        }
        public Vector[] getPoints()
        {
            Vector[] arr = new Vector[Points.Count];
            Points.CopyTo(arr);
            return arr;
        }
    }
    [Serializable]
    public abstract class CraneConfiguration
    {
        public string name;
        public double l1, l2, height, distToL1, distToBack, totalWidth;
        public CapacityFunction mainCapacity;
    }
    [Serializable]
    public class CraneConfiguration_Default : CraneConfiguration
    {
        public double l1_l2_angle;
        public CapacityFunction subsidiaryCapacity;
        public CraneConfiguration_Default()
        {
            name = "";
            l1 = 0;
            l2 = 0;
            height = 0;
            totalWidth = 0;
            distToL1 = 0;
            distToBack = 0;
            l1_l2_angle = 0;
            mainCapacity = new CapacityFunction();
            subsidiaryCapacity = new CapacityFunction();
        }
        public CraneConfiguration_Default(CraneConfiguration_Default config)
        {
            name = config.name;
            l1 = config.l1;
            l2 = config.l2;
            height = config.height;
            totalWidth = config.totalWidth;
            distToL1 = config.distToL1;
            distToBack = config.distToBack;
            l1_l2_angle = config.l1_l2_angle;
            mainCapacity = config.mainCapacity;
            subsidiaryCapacity = config.subsidiaryCapacity;
        }
    }
    [Serializable]
    public class CraneConfiguration_Tower : CraneConfiguration
    {
        public double l2_minAngle, l1_angle;
        public CraneConfiguration_Tower()
        {
            name = "";
            l1 = 0;
            l2 = 0;
            height = 0;
            totalWidth = 0;
            distToL1 = 0;
            distToBack = 0;
            l1_angle = 0;
            l2_minAngle = 0;
            mainCapacity = new CapacityFunction();
        }
        public CraneConfiguration_Tower(CraneConfiguration_Tower config)
        {
            name = config.name;
            l1 = config.l1;
            l2 = config.l2;
            height = config.height;
            totalWidth = config.totalWidth;
            distToL1 = config.distToL1;
            distToBack = config.distToBack;
            l2_minAngle = config.l2_minAngle;
            l1_angle = config.l1_angle;
            mainCapacity = config.mainCapacity;
        }
    }
    [Serializable]
    public class BuildingConfiguration
    {
        public string name;
        public double height, width;
        public double distToSite, siteLength;
        public BuildingConfiguration()
        {
            name = "";
            height = 0;
            width = 0;
            distToSite = 0;
            siteLength = 0;
        }
        public BuildingConfiguration(BuildingConfiguration config)
        {
            name = config.name;
            height = config.height;
            width = config.width;
            distToSite = config.distToSite;
            siteLength = config.siteLength;
        }
        public override string ToString()
        {
            Stream dataStream = new MemoryStream();
            BinaryFormatter serializer = new BinaryFormatter();
            serializer.Serialize(dataStream, this);
            dataStream.Position = 0;
            return (new StreamReader(dataStream)).ReadToEnd();
        }
        public static BuildingConfiguration fromString(string str)
        {
            Stream dataStream = new MemoryStream(Encoding.Unicode.GetBytes(str));
            BinaryFormatter serializer = new BinaryFormatter();
            return (BuildingConfiguration)serializer.Deserialize(dataStream);
        }
    }
    public partial class MainWindow : Window
    {
        private string cranesConfigPath = "cranes.dat";
        private string buildingsConfigPath = "buildings.dat";
        private CraneConfiguration _craneConfig;
        public CraneConfiguration craneConfig 
        { 
            get 
            { 
                return _craneConfig; 
            } 
            set 
            { 
                _craneConfig = value; 
                CurCraneConfig_label.Content = value.name; 
                if (value is CraneConfiguration_Default)
                    VisualCapacity.updateFunction(value.mainCapacity, (value as CraneConfiguration_Default).subsidiaryCapacity);
                else
                    VisualCapacity.updateFunction(value.mainCapacity, null);
            } 
        }
        private BuildingConfiguration _buildingConfig;
        public BuildingConfiguration buildingConfig { get { return _buildingConfig; } set { _buildingConfig = value; CurBuildingConfig_label.Content = value.name; } }
        public delegate double function(double arg);

        public MainWindow()
        {
            InitializeComponent();
            readConfigs();
        }

        double getMaxPoint(function f_der, double epsilon)
        {
            if (buildingConfig.siteLength != 0 && buildingConfig.siteLength < craneConfig.totalWidth)
                return double.NaN;

            double x_prev = epsilon;
            double temp;
            double x;
            x = Math.Max(buildingConfig.distToSite, craneConfig.totalWidth - craneConfig.distToBack - craneConfig.distToL1) + epsilon * 10;

            while (Math.Abs(x - x_prev) > epsilon)
            {
                temp = x;
                if (Math.Sign(f_der(x)) == Math.Sign(f_der(x_prev)))
                    x += (x - x_prev) * 1.5;
                else
                    x = (x + x_prev) / 2.0;
                x_prev = temp;
                if (x <= craneConfig.totalWidth - craneConfig.distToL1 - craneConfig.distToBack)
                    return craneConfig.totalWidth - craneConfig.distToL1 - craneConfig.distToBack;
                if (buildingConfig.distToSite != 0 && x <= buildingConfig.distToSite - craneConfig.distToL1 + craneConfig.totalWidth - craneConfig.distToBack)
                    return buildingConfig.distToSite - craneConfig.distToL1 + craneConfig.totalWidth - craneConfig.distToBack;
                if (buildingConfig.siteLength != 0 && x >= buildingConfig.siteLength + buildingConfig.distToSite - craneConfig.distToBack - craneConfig.distToL1)
                    return buildingConfig.siteLength + buildingConfig.distToSite - craneConfig.distToBack - craneConfig.distToL1;
            }

            return x;
        }

        double default_main_func(double x)
        {
            double h = buildingConfig.height - craneConfig.height;
            double atan = Math.Atan(h / x);
            return craneConfig.l1 * Math.Cos(atan) - x;
        }

        double default_main_func_der(double x)
        {
            double h = buildingConfig.height - craneConfig.height;
            double denom = ((h * h) / (x * x) + 1);
            return craneConfig.l1 * h * h / (x * x * x * Math.Pow(denom, 1.5)) - 1;
        }

        double default_sub_func(double x)
        {
            double h = buildingConfig.height - craneConfig.height;
            double atan = Math.Atan(h / x);
            return craneConfig.l1 * Math.Cos(atan) + craneConfig.l2 * Math.Cos(atan + (craneConfig as CraneConfiguration_Default).l1_l2_angle - Math.PI) - x;
        }

        double default_sub_func_der(double x)
        {
            double h = buildingConfig.height - craneConfig.height;
            double denom = ((h * h) / (x * x) + 1);
            double func = craneConfig.l1 * h * h / (x * x * x * Math.Pow(denom, 1.5)) - craneConfig.l2 * h * Math.Sin((craneConfig as CraneConfiguration_Default).l1_l2_angle + Math.Atan(h / x)) / (x * x * denom) - 1;
            return func;
        }

        private void AddCraneConfig_button_Click(object sender, RoutedEventArgs e)
        {
            CraneConfigurationItem newConfig = new CraneConfigurationItem();
            if (newConfig.EditConfig())
            {
                CraneConfigList.Items.Add(newConfig);
                newConfig.MouseLeftButtonDown += CraneConfigurationDoubleclick;
                WriteCranesConfigs();
            }
        }

        private void AddBuildingConfig_button_Click(object sender, RoutedEventArgs e)
        {
            BuildingConfigurationItem newConfig = new BuildingConfigurationItem();
            if (newConfig.EditConfig())
            {
                BuildingConfigList.Items.Add(newConfig);
                newConfig.MouseLeftButtonDown += BuildingConfigurationDoubleclick;
                WriteBuildingsConfigs();
            }
        }

        private void CraneConfigurationDoubleclick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                craneConfig = (sender as CraneConfigurationItem).getConfig();
                if (buildingConfig != null)
                    Calculate();
            }
        }

        private void BuildingConfigurationDoubleclick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                buildingConfig = (sender as BuildingConfigurationItem).getConfig();
                if (craneConfig != null)
                    Calculate();
            }
        }

        public void WriteCranesConfigs()
        {
            Stream fileStream = File.OpenWrite(cranesConfigPath);
            BinaryFormatter serializer = new BinaryFormatter();
            List<CraneConfiguration> configs = new List<CraneConfiguration>();
            foreach (object configItem in CraneConfigList.Items)
                configs.Add((configItem as CraneConfigurationItem).getConfig());
            serializer.Serialize(fileStream, configs);
            fileStream.Close();
        }

        public void WriteBuildingsConfigs()
        {
            Stream fileStream = File.OpenWrite(buildingsConfigPath);
            BinaryFormatter serializer = new BinaryFormatter();
            List<BuildingConfiguration> configs = new List<BuildingConfiguration>();
            foreach (object configItem in BuildingConfigList.Items)
                configs.Add((configItem as BuildingConfigurationItem).getConfig());
            serializer.Serialize(fileStream, configs);
            fileStream.Close();
        }

        private void readConfigs()
        {
            try
            {
                if (File.Exists(cranesConfigPath))
                {
                    Stream fileStream = File.OpenRead(cranesConfigPath);
                    BinaryFormatter serializer = new BinaryFormatter();
                    List<CraneConfiguration> configs = (List<CraneConfiguration>)serializer.Deserialize(fileStream);
                    fileStream.Close();
                    foreach (CraneConfiguration config in configs)
                    {
                        CraneConfigurationItem item = new CraneConfigurationItem(config);
                        item.MouseLeftButtonDown += CraneConfigurationDoubleclick;
                        CraneConfigList.Items.Add(item);
                    }
                }
            }
            catch
            {
                MessageBox.Show("При загрузке одна или несколько конфигураций кранов были с ошибками.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            try
            {
                if (File.Exists(buildingsConfigPath))
                {
                    Stream fileStream = File.OpenRead(buildingsConfigPath);
                    BinaryFormatter serializer = new BinaryFormatter();
                    List<BuildingConfiguration> configs = (List<BuildingConfiguration>)serializer.Deserialize(fileStream);
                    fileStream.Close();
                    foreach (BuildingConfiguration config in configs)
                    {
                        BuildingConfigurationItem item = new BuildingConfigurationItem(config);
                        item.MouseLeftButtonDown += BuildingConfigurationDoubleclick;
                        BuildingConfigList.Items.Add(item);
                    }
                }
            }
            catch
            {
                MessageBox.Show("При загрузке одна или несколько конфигураций зданий были с ошибками.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Calculate()
        {
            if (buildingConfig.siteLength != 0 && buildingConfig.siteLength < craneConfig.totalWidth)
            {
                setFieldsNull();
                MessageBox.Show("Площадка не позволяет разместить кран физически.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (craneConfig is CraneConfiguration_Default)
            {
                double main_lenToBuilding = getMaxPoint(default_main_func_der, 1E-6);
                double main_lenAboveBuilding = default_main_func(main_lenToBuilding);
                double main_totalLen = main_lenToBuilding + main_lenAboveBuilding + craneConfig.distToL1;
                Main_DistanceToBuilding.Content = main_lenToBuilding.ToString("f2");
                Main_LengthAboveBuilding.Content = main_lenAboveBuilding.ToString("f2");
                Main_TotalLength.Content = main_totalLen.ToString("f2");
                if (main_lenAboveBuilding >= buildingConfig.width)
                    Main_Capacity_BuildingEnd.Content = craneConfig.mainCapacity.Calculate(main_lenToBuilding + buildingConfig.width + craneConfig.distToL1).ToString("f2");
                else
                    Main_Capacity_BuildingEnd.Content = "-";
                if (main_lenAboveBuilding >= buildingConfig.width / 2.0)
                    Main_Capacity_BuildingMiddle.Content = craneConfig.mainCapacity.Calculate(main_lenToBuilding + buildingConfig.width / 2.0 + craneConfig.distToL1).ToString("f2");
                else
                    Main_Capacity_BuildingMiddle.Content = "-";
                Main_Capacity_End.Content = craneConfig.mainCapacity.Calculate(main_totalLen).ToString("f2");

                double sub_lenToBuilding = getMaxPoint(default_sub_func_der, 1E-6);
                double sub_lenAboveBuilding = default_sub_func(sub_lenToBuilding);
                double sub_totalLen = sub_lenToBuilding + sub_lenAboveBuilding + craneConfig.distToL1;
                Subsidiary_DistanceToBuilding.Content = sub_lenToBuilding.ToString("f2");
                Subsidiary_LengthAboveBuilding.Content = sub_lenAboveBuilding.ToString("f2");
                Subsidiary_TotalLength.Content = sub_totalLen.ToString("f2");
                if (sub_lenAboveBuilding >= buildingConfig.width)
                    Subsidiary_Capacity_BuildingEnd.Content = (craneConfig as CraneConfiguration_Default).subsidiaryCapacity.Calculate(sub_lenToBuilding + buildingConfig.width + craneConfig.distToL1).ToString("f2");
                else
                    Subsidiary_Capacity_BuildingEnd.Content = "-";
                if (sub_lenAboveBuilding >= buildingConfig.width / 2.0)
                    Subsidiary_Capacity_BuildingMiddle.Content = (craneConfig as CraneConfiguration_Default).subsidiaryCapacity.Calculate(sub_lenToBuilding + buildingConfig.width / 2.0 + craneConfig.distToL1).ToString("f2");
                else
                    Subsidiary_Capacity_BuildingMiddle.Content = "-";
                Subsidiary_Capacity_End.Content = (craneConfig as CraneConfiguration_Default).subsidiaryCapacity.Calculate(sub_totalLen).ToString("f2");

                double l1_angle_main = Math.Atan((buildingConfig.height - craneConfig.height) / main_lenToBuilding);
                MainVisualCrane.updateCrane(buildingConfig.width, buildingConfig.height, buildingConfig.distToSite, buildingConfig.siteLength, 
                                            craneConfig.distToL1, craneConfig.height, craneConfig.l1, craneConfig.l2, craneConfig.distToBack, craneConfig.totalWidth,
                                            l1_angle_main, (craneConfig as CraneConfiguration_Default).l1_l2_angle + l1_angle_main - Math.PI, main_lenToBuilding);
                double l1_angle_sub = Math.Atan((buildingConfig.height - craneConfig.height) / sub_lenToBuilding);
                SubsidiaryVisualCrane.updateCrane(buildingConfig.width, buildingConfig.height, buildingConfig.distToSite, buildingConfig.siteLength, 
                                                  craneConfig.distToL1, craneConfig.height, craneConfig.l1, craneConfig.l2, craneConfig.distToBack, craneConfig.totalWidth,
                                                  l1_angle_sub, (craneConfig as CraneConfiguration_Default).l1_l2_angle + l1_angle_sub - Math.PI, sub_lenToBuilding);
            }
            else if (craneConfig is CraneConfiguration_Tower)
            {
                double lenToBuilding;
                if (buildingConfig.height - craneConfig.height > Math.Sin((craneConfig as CraneConfiguration_Tower).l1_angle) * craneConfig.l1)
                    lenToBuilding = Math.Cos((craneConfig as CraneConfiguration_Tower).l1_angle) * craneConfig.l1 +
                                    (buildingConfig.height - craneConfig.height - Math.Sin((craneConfig as CraneConfiguration_Tower).l1_angle) * craneConfig.l1) /
                                    Math.Tan((craneConfig as CraneConfiguration_Tower).l2_minAngle);
                else
                    lenToBuilding = (buildingConfig.height - craneConfig.height) / Math.Tan((craneConfig as CraneConfiguration_Tower).l1_angle);
                if (lenToBuilding < craneConfig.totalWidth - craneConfig.distToL1 - craneConfig.distToBack)
                    lenToBuilding = craneConfig.totalWidth - craneConfig.distToL1 - craneConfig.distToBack;
                if (buildingConfig.distToSite != 0 && lenToBuilding < buildingConfig.distToSite + craneConfig.totalWidth - craneConfig.distToBack - craneConfig.distToL1)
                    lenToBuilding = buildingConfig.distToSite + craneConfig.totalWidth - craneConfig.distToBack - craneConfig.distToL1;
                if (buildingConfig.siteLength != 0 && lenToBuilding > buildingConfig.distToSite + buildingConfig.siteLength - craneConfig.distToBack - craneConfig.distToL1)
                {
                    setFieldsNull();
                    MessageBox.Show("Кран в такой конфигурации невозможно поставить на площадку.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                double totalLen = craneConfig.l1 * Math.Cos((craneConfig as CraneConfiguration_Tower).l1_angle) + craneConfig.l2 * Math.Cos((craneConfig as CraneConfiguration_Tower).l2_minAngle);
                double lenAboveBuilding = totalLen - lenToBuilding;
                Main_DistanceToBuilding.Content = lenToBuilding.ToString("f2");
                Main_LengthAboveBuilding.Content = lenAboveBuilding.ToString("f2");
                Main_TotalLength.Content = totalLen.ToString("f2");
                if (lenAboveBuilding >= buildingConfig.width)
                    Main_Capacity_BuildingEnd.Content = craneConfig.mainCapacity.Calculate(lenToBuilding + craneConfig.distToL1 + buildingConfig.width).ToString("f2");
                else
                    Main_Capacity_BuildingEnd.Content = "-";
                if (lenAboveBuilding >= buildingConfig.width / 2.0)
                    Main_Capacity_BuildingMiddle.Content = craneConfig.mainCapacity.Calculate(lenToBuilding + craneConfig.distToL1 + buildingConfig.width / 2.0).ToString("f2");
                else
                    Main_Capacity_BuildingMiddle.Content = "-";
                Main_Capacity_End.Content = craneConfig.mainCapacity.Calculate(totalLen + craneConfig.distToL1).ToString("f2");

                Subsidiary_Capacity_BuildingEnd.Content = "-";
                Subsidiary_Capacity_BuildingMiddle.Content = "-";
                Subsidiary_Capacity_End.Content = "-";
                Subsidiary_DistanceToBuilding.Content = "-";
                Subsidiary_LengthAboveBuilding.Content = "-";
                Subsidiary_TotalLength.Content = "-";

                MainVisualCrane.updateCrane(buildingConfig.width, buildingConfig.height, buildingConfig.distToSite, buildingConfig.siteLength, 
                                            craneConfig.distToL1, craneConfig.height, craneConfig.l1, craneConfig.l2, craneConfig.distToBack, craneConfig.totalWidth,
                                            (craneConfig as CraneConfiguration_Tower).l1_angle, (craneConfig as CraneConfiguration_Tower).l2_minAngle, lenToBuilding);
                SubsidiaryVisualCrane.clearCrane();
            }
        }

        private void setFieldsNull()
        {
            Main_DistanceToBuilding.Content = "-";
            Main_LengthAboveBuilding.Content = "-";
            Main_TotalLength.Content = "-";
            Main_Capacity_End.Content = "-";
            Main_Capacity_BuildingEnd.Content = "-";
            Main_Capacity_BuildingMiddle.Content = "-";

            Subsidiary_DistanceToBuilding.Content = "-";
            Subsidiary_LengthAboveBuilding.Content = "-";
            Subsidiary_TotalLength.Content = "-";
            Subsidiary_Capacity_End.Content = "-";
            Subsidiary_Capacity_BuildingEnd.Content = "-";
            Subsidiary_Capacity_BuildingMiddle.Content = "-";

            MainVisualCrane.clearCrane();
            SubsidiaryVisualCrane.clearCrane();
        }
    }
}
