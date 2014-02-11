using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HelixToolkit.Wpf;

namespace TestHelix
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ajouterLigne(new Point3D(0, 0, 0), new Point3D(1, 0, 0), Colors.Red, 3);
            ajouterLigne(new Point3D(0, 0, 0), new Point3D(0, 1, 0), Colors.Green, 3);
            ajouterLigne(new Point3D(0, 0, 0), new Point3D(0, 0, 1), Colors.Blue, 3);
        }

        private void ajouterLigne(Point3D p1, Point3D p2, Color color, float thickness)
        {
            LinesVisual3D ligne = new LinesVisual3D();
            ligne.Points.Add(p1);
            ligne.Points.Add(p2);
            ligne.Color = color;
            ligne.Thickness = thickness;

            ViewPort.Children.Add(ligne);
        }
    }
}
