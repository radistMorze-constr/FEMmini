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
                _manager.ReadProblemFile(Info.FileIn);
                /*
                _solver.AddMaterial(0,
                    Typematerial.Elastic,
                    double.Parse(MechanicalInfo.E),
                    double.Parse(MechanicalInfo.Nu),
                    double.Parse(MechanicalInfo.Poh),
                    double.Parse(MechanicalInfo.C),
                    double.Parse(MechanicalInfo.Phi));
                */
            });
            WriteSolutionCommand = new DelegateCommand(() =>
            {
                _manager.WriteSolutionToFile(Info.FileOut);
                var id = new SolutionID(0, 0);
                _manager.SetSolutionToRender(id);
            });

            AnalysisMode = new AnalysisModeVM(_solver);
            VisualMode = new VisualModeVM(_manager, Renderer);
            CommonSign = new CommonSignVM(Renderer);
        }

        public InputInfoVM Info { get; } = new InputInfoVM();
        public PropertiesVM MechanicalInfo { get; } = new PropertiesVM();
        public AnalysisModeVM AnalysisMode { get; set;  }
        public VisualModeVM VisualMode { get; set; }
        public ResultInfoVM ResultInfo { get; } = new ResultInfoVM();
        public CommonSignVM CommonSign { get; set; }
        public Renderer Renderer { get; }

        private SolverManager _manager = new SolverManager();
        private FEM _solver;
        private DrawGeometry _geometry;
        private GeometryDataConverter _geometryContext;

        public DelegateCommand UpdateDataCommand { get; }
        public DelegateCommand WriteSolutionCommand { get; }
    }

    public class InputInfoVM : BindableBase
    {
        public InputInfoVM()
        {
            DownloadCommand = new DelegateCommand(() =>
            {
                var o = new SWF.OpenFileDialog();
                o.Filter = "Text files (*.in)|*.in|All files (*.*)|*.*";
                if (o.ShowDialog() == true)
                {
                    FileIn = o.FileName;
                    FileOut = Path.GetDirectoryName(FileIn) + "/result.out";
                }
                RaisePropertyChanged(nameof(FileIn));
                RaisePropertyChanged(nameof(FileOut));
            });
        }
        public string FileIn { get; set; }
        public string FileOut { get; set; }

        public DelegateCommand DownloadCommand { get; }
    }

    public class CommonSignVM : BindableBase
    {
        private Renderer _renderer;
        public Vector2 MouseCoordinates 
        { 
            get => _renderer.MouseCoordinates;
            set => _renderer.MouseCoordinates = value;
        }
        public CommonSignVM(Renderer renderer)
        {
            _renderer = renderer;
            _renderer.PropertyChanged += (s, e) => { RaisePropertyChanged(e.PropertyName); };
        }
    }

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

    public class AnalysisModeVM : BindableBase
    {
        private readonly FEM _solver;
        public AnalysisModeVM(FEM solver)
        {
            _solver = solver;
            CalculateCommand = new DelegateCommand(() =>
            {
                solver.Solve();
            });
        }
        public LinearityType LinearityType { get => _solver.LinearityType; set => _solver.LinearityType = value; }
        public ProblemType ProblemType { get => _solver.ProblemType; set => _solver.ProblemType = value; }

        public DelegateCommand CalculateCommand { get; }
    }

    public class VisualModeVM : BindableBase
    {
        private SolverManager _manager;
        private Renderer _renderer;

        public bool ModeIsDeformed
        {
            get => _renderer.Mode.IsDeformed;
            set => _renderer.Mode.IsDeformed = value;
        }
        public List<int> DeformationMultiple { get; set; } = Enumerable.Range(1, 100).ToList();
        public int DeformationMultipleChoice {
            get => _manager.MultipleDeform;
            set => _manager.MultipleDeform = value;
        }
        public bool ModeElement
        {
            get => _renderer.Mode.Element;
            set => _renderer.Mode.Element = value;
        }
        public bool ModeNode
        {
            get => _renderer.Mode.Node;
            set => _renderer.Mode.Node = value;
        }
        public bool ModeConstraint
        {
            get => _renderer.Mode.Constraint;
            set => _renderer.Mode.Constraint = value;
        }
        public bool ModeLoad
        {
            get => _renderer.Mode.LoadLine;
            set => _renderer.Mode.LoadLine = value;
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

    public class ResultInfoVM
    {
        public string E { get; set; } = "20000";
    }

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
}
