using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Media;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using ScottPlot.Drawing.Colormaps;
using static OpenTK.Graphics.OpenGL.GL;
using Engine;
using Prism.Mvvm;
using Common;

namespace FEMmini
{
    public class FEM : BindableBase
    {
        private int _idModels = 0;
        private readonly DrawGeometry _geometry;
        private EquationSolver _solver;
        public Dictionary<int, MaterialModel> Properties = new Dictionary<int, MaterialModel>();
        public Dictionary<int, NodeLoad> LoadsNoad { get; private set; } = new Dictionary<int, NodeLoad>();
        public Dictionary<int, LineLoad> LoadsLine { get; private set; } = new Dictionary<int, LineLoad>();
        public Dictionary<int, SurfaceLoad> LoadsSurface { get; private set; } = new Dictionary<int, SurfaceLoad>();
        public Dictionary<int, Constraints> Constraints = new Dictionary<int, Constraints>();
        private Dictionary<int, PhaseCharacteristics> _phaseCharacteristics = new Dictionary<int, PhaseCharacteristics>();
        public Dictionary<SolutionID, Solution> Solutions { get; private set; } = new Dictionary<SolutionID, Solution>();
        private SolutionID _solutionID;

        public LinearityType LinearityType { get; set; }
        public ProblemType ProblemType { get; set; }

        public FEM(DrawGeometry geom)
        {
            _geometry = geom;
        }

        public void Initialize(Dictionary<int, NodeLoad> loadsNode,
            Dictionary<int, LineLoad> loadsLine,
            Dictionary<int, SurfaceLoad> loadsSurface,
            Dictionary<int, Constraints> constraints, 
            Dictionary<int, MaterialModel> properties,
            Dictionary<int, PhaseCharacteristics> phaseCharacteristics)
        {
            LoadsNoad = loadsNode;
            LoadsLine = loadsLine;
            LoadsSurface =  loadsSurface;
            Constraints = constraints;
            Properties = properties;
            _phaseCharacteristics = phaseCharacteristics;
            _solver = new SolverPlaneLinear(ProblemType, _geometry, _phaseCharacteristics, Properties);
        }
            
        public Solution GetSolution(SolutionID id) 
        {
            return Solutions[id];
        }
        public PhaseCharacteristics GetPhaseCharacteristics(int phaseId)
        {
            return _phaseCharacteristics[phaseId];
        }
        public Load GetLoad(int loadId)
        {
            return LoadsNoad[loadId];
        }
        public Constraints GetConstraint(int constraintId)
        {
            return Constraints[constraintId];
        }
        public void AddMaterial(int index, Typematerial typeModel, double E, double nu, double rhof, double c, double phi)
        {
            ++_idModels;
            if (typeModel == Typematerial.Elastic)
            {
                Properties[_idModels] = new Elastic(index, E, nu, rhof, ProblemType);
            }
            else
            {
                Properties[_idModels] = new IdealPlasticity(index, E, nu, rhof, ProblemType, c, phi);
            }
        }

        public IEnumerable<Constraints> IteratorConstraint(List<int> constraints)
        {
            foreach (var index in constraints)
            {
                yield return Constraints[index];
            }
            yield break;
        }

        public IEnumerable<Load> IteratorLoad(List<int> loads)
        {
            foreach (var index in loads) 
            {
                if (LoadsNoad.ContainsKey(index))
                {
                    yield return LoadsNoad[index];
                }
                else if (LoadsLine.ContainsKey(index))
                {
                    yield return LoadsLine[index];
                }
                else if (LoadsSurface.ContainsKey(index))
                {
                    yield return LoadsSurface[index];
                }
                else
                {
                    throw new Exception("Не существует нагрузки в библиотеке с таким индексом!");
                }
            }
            yield break;
        }

        public void Solve()
        {
            foreach(var solutionProperty in _phaseCharacteristics.Values)
            {
                var count = solutionProperty.CountSteps;
                for (int i = 0; i < count; i++) 
                {
                    _solutionID = new SolutionID(solutionProperty.IndexPhase, i);
                    Solutions[_solutionID] = new Solution(_solutionID);
                    var loadsList = new List<int>();
                    loadsList.AddRange(solutionProperty.LoadNodeIDs);
                    loadsList.AddRange(solutionProperty.LoadLineIDs);
                    loadsList.AddRange(solutionProperty.LoadSurfaceIDs);
                    _solver.SolveEquations(IteratorLoad(loadsList),
                            IteratorConstraint(solutionProperty.ConstraintIDs),
                            Solutions[_solutionID]);
                }
            }
        }
    }
}
