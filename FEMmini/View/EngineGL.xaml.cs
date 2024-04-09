using Engine;
using OpenTK.Mathematics;
using OpenTK.Wpf;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using OpenTK.Platform.Windows;

namespace FEMmini
{
    /// <summary>
    /// Логика взаимодействия для EngineGL.xaml
    /// </summary>
    public partial class EngineGL : UserControl
    {
        private Renderer _renderer;

        public EngineGL()
        {
            InitializeComponent();
            
            //_renderer = new Renderer();

            var settings = new GLWpfControlSettings
            {
                MajorVersion = 1,
                MinorVersion = 1
            };
            OpenTkControl.Start(settings);
        }
        
        private void OpenTkControl_OnRender(TimeSpan delta)
        {
            //var vm = (MainWindowVM)this.DataContext;
            //_renderer = vm.Renderer;
            //_renderer.VisaulMode = vm.VisualMode.Mode;
            /*
            var geometryData = vm.GeometryData;
            if (geometryData.NeedToUpdateRender)
            {
                _renderer.VertNodes = VertNodes;
                _renderer.IndicesNodes = IndicesNodes;
                _renderer.IndicesElements   = IndicesElements;
                _renderer.IndicesConstraints = IndicesConstraints;
                _renderer.UpdateVBO();
            }
            */
            _renderer.Render();
            
        }

        private void OpenTkControl_LeftUp(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(OpenTkControl);
            _renderer.LeftButtonDown = false;
            _renderer.LastMousePosition = new Vector2((float)position.X, (float)position.Y);
        }
        private void OpenTkControl_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(OpenTkControl);
            _renderer.MouseMove(position);
        }

        private void OpenTkControl_LeftDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(OpenTkControl);
            _renderer.LeftButtonDown = true;
            _renderer.LastMousePosition = new Vector2((float)position.X, (float)position.Y);
        }

        private void OpenTkControl_Wheel(object sender, MouseWheelEventArgs e)
        {
            var mousePosition = e.GetPosition(OpenTkControl);
            _renderer.MouseWheel(e.Delta, mousePosition);
        }

        private void OpenTkControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var widgetWidth = (float)OpenTkControl.ActualWidth;
            var widgetHeight = (float)OpenTkControl.ActualHeight;
            _renderer.SizeChange(widgetWidth, widgetHeight);
        }

        private void OpenTkControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = (MainWindowVM)this.DataContext;
            _renderer = vm.Renderer;
            var widgetWidth = (float)OpenTkControl.ActualWidth;
            var widgetHeight = (float)OpenTkControl.ActualHeight;
            _renderer.Ready(widgetWidth, widgetHeight);
        }

        private void OpenTkControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                _renderer.MouseWheelPressed();
            }
        }
    }
}
