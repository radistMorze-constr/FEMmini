using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEMmini
{
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
        public PositiveDirection PositiveDirection { get => _solver.PositiveDirection; set => _solver.PositiveDirection = value; }
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
}
