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
            _geometryContext = _manager._geometryConverter;
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
                _manager.ReadProblemFile(Info.FileIn);
                Info.ExampleName = _manager.Name;
                Info.UpdateContainers();

                var id = new SolutionID(0, 0);
                _manager.SetSolutionToRender(id, false);
                VisualMode.ModeElement = true;
                VisualMode.ModeConstraint = true;
            });
            CalculateCommand = new DelegateCommand(() =>
            {
                _solver.Solve();

                _manager.WriteSolutionToFile(Info.FileOut);
                var id = new SolutionID(0, 0);
                _manager.SetSolutionToRender(id);
                AnalysisMode.IsCalculated = true;
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
        public Renderer Renderer { get; }

        private SolverManager _manager = new SolverManager();
        private FEM _solver;
        private DrawGeometry _geometry;
        private GeometryDataConverter _geometryContext;

        public DelegateCommand UpdateDataCommand { get; }
        public DelegateCommand CalculateCommand { get; }
    }
    public class NodeVM
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public string Constraint { get; set; }
        public NodeVM(int id, double x, double y, string constraint = "нет")
        {
            Id = id;
            X = x;
            Y = y;
            Constraint = constraint;
        }
    }
    public class ElementVM
    {
        public int Id { get; set; }
        public string Nodes { get; set; }
        public int IdModule { get; set; }
        public ElementVM(int id, List<int> nodes, int idModule)
        {
            Id = id;
            Nodes = string.Format("{0}; {1}; {2}", nodes[0], nodes[1], nodes[2]);
            IdModule = idModule;
        }
    }
    public class StiffnessVM
    {
        public int Id { get; set; }
        public double E { get; set; }
        public double Nu { get; set; }
        public double Rhof { get; set; }
        public StiffnessVM(double id, double e, double nu, double rhof)
        {
            Id = (int)id;
            E = e;
            Nu = nu;
            Rhof = rhof;
        }
    }
    public class LoadsVM
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int Count { get; set; }
        public LoadsVM(int id, string type, double x, double y, int count)
        {
            Id = id;
            Type = type;
            X = x;
            Y = y;
            Count = count;
        }
    }
    public class InputInfoVM : BindableBase
    {
        private FEM _solver;
        private DrawGeometry _geometry;

        private string _fileIn;
        private string _fileOut;
        private string _exampleName;
    public InputInfoVM(DrawGeometry geometry, FEM solver)
        {
            _geometry = geometry;
            _solver = solver;
            NodeVM = new List<NodeVM>();
            ElementVM = new List<ElementVM>();
            StiffnessVM = new List<StiffnessVM>();
            LoadsVM = new List<LoadsVM>();
        }
        public string FileIn
        {
            get => _fileIn;
            set
            {
                _fileIn = value;
                RaisePropertyChanged();
            }
        }
        public List<NodeVM> NodeVM { get; set; }
        public List<ElementVM> ElementVM { get; set; }
        public List<StiffnessVM> StiffnessVM { get; set; }
        public List<LoadsVM> LoadsVM { get; set; }
        public string FileOut
        {
            get => _fileOut;
            set
            {
                _fileOut = value;
                RaisePropertyChanged();
            }
        }
        public string ExampleName
        {
            get => _exampleName;
            set
            {
                _exampleName = value;
                RaisePropertyChanged();
            }
        }
        private string ConstraintTypeTostring(ConstraintType type)
        {
            if (type == ConstraintType.X)
            {
                return "X";
            }
            else if (type == ConstraintType.Y)
            {
                return "Y";
            }
            else
            {
                return "XY";
            }
        }
        public void UpdateContainers()
        {
            var nodes = _geometry.Nodes;
            var elements = _geometry.Elements;
            var constraints = _solver.Constraints;
            var materials = _solver.Properties;
            var nodesLoads = _solver.LoadsNoad;
            var lineLoads = _solver.LoadsLine;
            var surfaceLoads = _solver.LoadsSurface;
            foreach (var node in nodes.Values)
            {
                NodeVM.Add(new NodeVM(node.Id, node.X, node.Y));
            }
            foreach (var constraint in constraints.Values)
            {
                var str = ConstraintTypeTostring(constraint.Type);
                foreach (var nodeId in constraint.Nodes)
                {
                    NodeVM[nodeId - 1].Constraint = str;
                }
            }
            foreach (var elem in elements.Values)
            {
                ElementVM.Add(new ElementVM(elem.Id, elem.Nodes, elem.Properties));
            }
            foreach (var material in materials.Values)
            {
                StiffnessVM.Add(new StiffnessVM(material.Id, material.E, material.Nu, material.Rhof));
            }
            foreach (var load in nodesLoads.Values)
            {
                LoadsVM.Add(new LoadsVM(load.Id, "Узловая, тс", load.ForceX, load.ForceY, load.Nodes.Count));
            }
            foreach (var load in lineLoads.Values)
            {
                LoadsVM.Add(new LoadsVM(load.Id, "Линейная, тс/м", load.ForceX, load.ForceY, load.Nodes.Count));
            }
            foreach (var load in surfaceLoads.Values)
            {
                LoadsVM.Add(new LoadsVM(load.Id, "Площадная, тс/м2", load.ForceX, load.ForceY, load.Elements.Count));
            }
        }
    }
        
    public class CommonSignVM : BindableBase
    {
        private Renderer _renderer;
        public Vector2 MouseCoordinates 
        { 
            get => _renderer.MouseCoordinates;
        }
        public CommonSignVM(Renderer renderer)
        {
            _renderer = renderer;
            _renderer.PropertyChanged += (s, e) => { RaisePropertyChanged(e.PropertyName); };
        }
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
    public class AnalysisModeVM : BindableBase
    {
        private readonly FEM _solver;
        private bool _isCalculated;
        public AnalysisModeVM(FEM solver)
        {
            _solver = solver;
        }
        public LinearityType LinearityType { get => _solver.LinearityType; set => _solver.LinearityType = value; }
        public ProblemType ProblemType { get => _solver.ProblemType; set => _solver.ProblemType = value; }
        public bool IsCalculated
        {
            get => _isCalculated;
            set
            {
                _isCalculated = value;
                RaisePropertyChanged();
            }
        }
    }

    public class VisualModeVM : BindableBase
    {
        private SolverManager _manager;
        private Renderer _renderer;

        public bool ModeIsDeformed
        {
            get => _renderer.Mode.IsDeformed;
            set
            {
                _renderer.Mode.IsDeformed = value;
                RaisePropertyChanged();
            }
        }
        public List<int> DeformationMultiple { get; set; } = Enumerable.Range(1, 100).ToList();
        public int DeformationMultipleChoice {
            get => _manager.MultipleDeform;
            set => _manager.MultipleDeform = value;
        }
        public bool ModeElement
        {
            get => _renderer.Mode.Element;
            set
            {
                _renderer.Mode.Element = value;
                RaisePropertyChanged();
            }
        }
        public bool ModeNode
        {
            get => _renderer.Mode.Node;
            set
            {
                _renderer.Mode.Node = value;
                RaisePropertyChanged();
            }
        }
        public bool ModeConstraint
        {
            get => _renderer.Mode.Constraint;
            set
            {
                _renderer.Mode.Constraint = value;
                RaisePropertyChanged();
            }
        }
        public bool ModeLoad
        {
            get => _renderer.Mode.LoadLine;
            set
            {
                _renderer.Mode.LoadNode = value;
                _renderer.Mode.LoadLine = value;
                _renderer.Mode.LoadSurface = value;
                RaisePropertyChanged();
            }
        }
        public SolutionID SolutionID
        {
            get => _manager.SolutionID;
            set => _manager.SolutionID = value;
        }
        public VisualNodeText NodeResult
        {
            get => _manager.NodeResult;
            set => _manager.NodeResult = value;
        }
        public VisualElementText ElementResult
        {
            get => _manager.ElementResult;
            set => _manager.ElementResult = value;
        }
        public VisualLineCenterText LineCenterText
        {
            get => _manager.LineCenterText;
            set => _manager.LineCenterText = value;
        }
        public VisualModeVM(SolverManager manager, Renderer renderer)
        {
            _manager = manager;
            _renderer = renderer;
            _renderer.PropertyChanged += (s, e) => { RaisePropertyChanged(e.PropertyName); };
        }
    }
    /*
    public class ResultInfoVM
    {
        public string E { get; set; } = "20000";
    }
    */
    /// <summary>WPF конвертер для получения списка значений перечисления</summary>
    public class EnumsIEnumerableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value?.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value?.Equals(true) == true ? parameter : Binding.DoNothing;
        }
    }
    public class BoolToValueConverter<T> : IValueConverter
    {
        public T FalseValue { get; set; }
        public T TrueValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return FalseValue;
            else
                return (bool)value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value != null ? value.Equals(TrueValue) : false;
        }
    }
    public class BoolToStringConverter : BoolToValueConverter<String> { }
    public class BoolToBoolConverter : BoolToValueConverter<bool> { }
}
