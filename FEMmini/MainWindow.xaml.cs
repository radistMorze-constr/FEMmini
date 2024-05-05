using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using OpenTK.Wpf;
using OpenTK.Mathematics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Input =  System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Reflection;
using Engine;
using ScottPlot;
using System.Data;
using System.Windows.Media.Media3D;
using MathNet.Numerics;

namespace FEMmini
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow(MainWindowVM windowVM)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            DataContext = windowVM;
            InitializeComponent();
        }
    }
}