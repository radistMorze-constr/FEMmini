using Prism.Commands;
using Prism.Mvvm;
using System.Security.Cryptography.Xml;
using System.Xml.Linq;
using System.Collections.Generic;
using Engine;
using System.Globalization;
using System.Windows.Data;
using System;
using OpenTK.Wpf;
using OpenTK.Mathematics;
using Common;
using System.Windows.Navigation;
using SWF = Microsoft.Win32;
using System.IO;
using MathNet.Numerics.LinearAlgebra.Factorization;
using System.Security.Policy;
using System.Linq;
using static System.Net.WebRequestMethods;
using System.Threading;
using ScottPlot.Drawing.Colormaps;
using FEMmini;
using System.Windows.Media;

namespace FEMmini
{
    public class MainWindowVM : BindableBase
    {
        public MainWindowVM() 
        {
            _solver = _manager.Fem;
            _geometry= _manager.Geometry;
            Renderer = _manager.Renderer;

            _solver.PropertyChanged += (s, e) => { RaisePropertyChanged(e.PropertyName); };

            UpdateDataCommand = new DelegateCommand(() =>
            {
                var o = new SWF.OpenFileDialog();
                o.Filter = "Text files (*.in)|*.in|All files (*.*)|*.*";
                if (o.ShowDialog() == true)
                {
                    Info.FileIn = o.FileName;
                    Info.FileOut = Path.GetDirectoryName(Info.FileIn) + "/result.out";
                }
                if (Info.FileIn == null || Info.FileIn.Length == 0)
                {
                    return;
                }
                _manager.ReadProblemFile(Info.FileIn);
                Info.ExampleName = _manager.Name;
                Info.UpdateContainers();

                var id = new SolutionID(0, 0);
                _manager.SetSolutionToRender(id, false);
                VisualMode.ModeElement = true;
                VisualMode.ModeConstraint = true;
                AnalysisMode.IsCalculated = false;
                VisualMode.ModeIsDeformed = false;
            });
            CalculateCommand = new DelegateCommand(() =>
            {
                _solver.Solve();

                _manager.WriteSolutionToFile(Info.FileOut);
                var id = new SolutionID(0, 0);
                _manager.SetSolutionToRender(id);
                AnalysisMode.IsCalculated = true;
            });
            DeleteDataCommand = new DelegateCommand(() =>
            {
                _manager = new SolverManager();
                _solver = _manager.Fem;
                _geometry = _manager.Geometry;
                Renderer = _manager.Renderer;
                //Info.ExampleName = "Новая задача";
            });

            Info = new InputInfoVM(_geometry, _solver);
            AnalysisMode = new AnalysisModeVM(_solver);
            VisualMode = new VisualModeVM(_manager, Renderer);
            CommonSign = new CommonSignVM(Renderer);
        }

        public InputInfoVM Info { get; private set; }
        public AnalysisModeVM AnalysisMode { get; set;  }
        public VisualModeVM VisualMode { get; set; }
        public CommonSignVM CommonSign { get; set; }
        public bool ModeIsCalculatedIsDeformed
        {
            get
            {
                if (AnalysisMode.IsCalculated)
                {
                    return !VisualMode.ModeIsDeformed;
                }
                else return false;
            }
        }
        public Renderer Renderer { get; private set; }

        private SolverManager _manager = new SolverManager();
        private FEM _solver;
        private DrawGeometry _geometry;

        public DelegateCommand UpdateDataCommand { get; }
        public DelegateCommand DeleteDataCommand { get; }
        public DelegateCommand CalculateCommand { get; }
    }
    /*
    public class PropertiesVM
    {
        public LinearityType LinearType { get; set; } = LinearityType.Linear;

        public string E { get; set; } = "20000";
        public string Poh { get; set; } = "0";
        public string Nu { get; set; } = "0,33";
        public string LineForce { get; set; } = "-10";
        public string G { get; set; } = "0"; //Пока что в решении не участвует, только плотность
        public string C { get; set; } = "8";
        public string Phi { get; set; } = "29";
    }
    */
    /*
    public class ResultInfoVM
    {
        public string E { get; set; } = "20000";
    }
    */
}
