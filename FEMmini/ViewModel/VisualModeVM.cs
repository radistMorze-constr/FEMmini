using Common;
using Engine;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEMmini
{
    public class VisualModeVM : BindableBase
    {
        private SolverManager _manager;
        private Renderer _renderer;
        private bool _modeIdNode;
        private bool _modeIdElement;
        private bool _modeDeflectionX;
        private bool _modeDeflectionY;
        private bool _modeStressX;
        private bool _modeStressY;
        private bool _modeStressXY;
        private bool _modeStress1;
        private bool _modeStress3;

        private void DisableAllText()
        {
            _modeIdNode = false;
            _modeIdElement = false;
            _modeDeflectionX = false;
            _modeDeflectionY = false;
            _modeStressX = false;
            _modeStressY = false;
            _modeStressXY = false;
            _modeStress1 = false;
            _modeStress3 = false;
            RaisePropertyChanged(nameof(ModeIdNode));
            RaisePropertyChanged(nameof(ModeIdElement));
            RaisePropertyChanged(nameof(ModeDeflectionX));
            RaisePropertyChanged(nameof(ModeDeflectionY));
            RaisePropertyChanged(nameof(ModeStressX));
            RaisePropertyChanged(nameof(ModeStressY));
            RaisePropertyChanged(nameof(ModeStressXY));
            RaisePropertyChanged(nameof(ModeStress1));
            RaisePropertyChanged(nameof(ModeStress3));
        }
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
        public int DeformationMultipleChoice
        {
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
        public bool ModeIdNode
        {
            get => _modeIdNode;
            set
            {
                DisableAllText();
                _modeIdNode = value;
                _renderer.Mode.Text = value;
                _renderer.Mode.TextType = TextType.NodeId;
                _renderer.TextScreenUpdate();
                RaisePropertyChanged();
            }
        }
        public bool ModeIdElement
        {
            get => _modeIdElement;
            set
            {
                DisableAllText();
                _modeIdElement = value;
                _renderer.Mode.Text = value;
                _renderer.Mode.TextType = TextType.ElementId;
                _renderer.TextScreenUpdate();
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
        public bool ModeDeflectionX
        {
            get => _modeDeflectionX;
            set
            {
                DisableAllText();
                _modeDeflectionX = value;
                _renderer.Mode.Text = value;
                _renderer.Mode.TextType = TextType.DeflectionX;
                _renderer.TextScreenUpdate();
                RaisePropertyChanged();
            }
        }
        public bool ModeDeflectionY
        {
            get => _modeDeflectionY;
            set
            {
                DisableAllText();
                _modeDeflectionY = value;
                _renderer.Mode.Text = value;
                _renderer.Mode.TextType = TextType.DeflectionY;
                _renderer.TextScreenUpdate();
                RaisePropertyChanged();
            }
        }
        public bool ModeStressX
        {
            get => _modeStressX;
            set
            {
                DisableAllText();
                _modeStressX = value;
                _renderer.Mode.Text = value;
                _renderer.Mode.TextType = TextType.StressX;
                _renderer.TextScreenUpdate();
                RaisePropertyChanged();
            }
        }
        public bool ModeStressY
        {
            get => _modeStressY;
            set
            {
                DisableAllText();
                _modeStressY = value;
                _renderer.Mode.Text = value;
                _renderer.Mode.TextType = TextType.StressY;
                _renderer.TextScreenUpdate();
                RaisePropertyChanged();
            }
        }
        public bool ModeStressXY
        {
            get => _modeStressXY;
            set
            {
                DisableAllText();
                _modeStressXY = value;
                _renderer.Mode.Text = value;
                _renderer.Mode.TextType = TextType.StressXY;
                _renderer.TextScreenUpdate();
                RaisePropertyChanged();
            }
        }
        public bool ModeStress1
        {
            get => _modeStress1;
            set
            {
                DisableAllText();
                _modeStress1 = value;
                _renderer.Mode.Text = value;
                _renderer.Mode.TextType = TextType.Stress1;
                _renderer.TextScreenUpdate();
                RaisePropertyChanged();
            }
        }
        public bool ModeStress3
        {
            get => _modeStress3;
            set
            {
                DisableAllText();
                _modeStress3 = value;
                _renderer.Mode.Text = value;
                _renderer.Mode.TextType = TextType.Stress3;
                _renderer.TextScreenUpdate();
                RaisePropertyChanged();
            }
        }
        public VisualModeVM(SolverManager manager, Renderer renderer)
        {
            _manager = manager;
            _renderer = renderer;
            _renderer.PropertyChanged += (s, e) => { RaisePropertyChanged(e.PropertyName); };
        }
    }
}
