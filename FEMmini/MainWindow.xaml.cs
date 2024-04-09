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

        private void CalcSolution(object sender, EventArgs e)
        {
            /*Чтения данных из Инпута
            _geom = new ClassGeometry(Nodes.Text, Elements.Text, NodesDirihle.Text, NodesNeyman.Text);
            var rhof = double.Parse(input_rhof.Text) * double.Parse(input_g.Text);
            var phi = double.Parse(input_phi.Text) * Math.PI / 180;
            var fem = new FEM(_geom,
                _problemType,
                _linearityType,
                double.Parse(input_E.Text),
                double.Parse(input_nu.Text),
                double.Parse(input_q.Text),
                rhof,
                double.Parse(input_c.Text),
                phi);
            fem.SolveFEM();
            _nodesResult = fem.NodesResult;
            _elementResult = fem.ElementResult;
            _isCalculated = true;

            _indices = _geom.GLIndices();
            _vertices= _geom.GLVertices();
            _constraints= _geom.GLConstraints();
            _forces = _geom.GLForces();
            _properties = fem.GetProperties();
            */

            /*Заполненеие графиков НДС грунта
            FillTables();
            for (int i = 0; i < _elementResult.Count(); i++)
            {
                ElemComboBox.Items.Add(i);
            }
            for (int i = 0; i < fem.IterationCount; i++)
            {
                IterationComboBox.Items.Add(i);
            }
            */
        }
    }
}