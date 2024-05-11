using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Engine;
using Microsoft.VisualBasic.ApplicationServices;
using Prism.Mvvm;

namespace FEMmini
{
    public class SolverManager : BindableBase
    {
        private SolutionID _solutionIDtoRender;
        private int _multipleDeform = 10;
        public int MultipleDeform 
        {
            get => _multipleDeform;
            set
            {
                _multipleDeform = value;
                SetSolutionToRender(_solutionIDtoRender);
            }
        }
        private string _name = "Новая задача";
        public string Name 
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }
        public SolutionID SolutionID 
        {
            get => _solutionIDtoRender;
            set
            {
                _solutionIDtoRender = value;
                SetSolutionToRender(_solutionIDtoRender);
            }
        }
        public SolverManager()
        {
            Fem = new FEM(Geometry);
            _geometryConverter = new GeometryDataConverter(Fem, Geometry);
        }
        public DrawGeometry Geometry { get; } = new DrawGeometry();
        public Renderer Renderer { get; set; } = new Renderer();
        /// <summary>
        /// Хранит текущие данные в простейшем виде для вывода на экран
        /// </summary>
        public GeometryDataConverter _geometryConverter;
        public FEM Fem { get; }

        public void ReadProblemFile(string problemFile)
        {
            var problemData = Parser.ReadProblemData(problemFile);
            if (problemData.Name != string.Empty)
            {
                Name = problemData.Name;
            }
            Geometry.Initialize(problemData.Elements, problemData.Nodes, problemData.MeshSets);
            Fem.Initialize(problemData.LoadsNode, problemData.LoadsLine, problemData.LoadsSurface, 
                           problemData.Constraints, problemData.Properties, problemData.PhaseCharacteristics);
            //Renderer.Borders = Geometry.Borders;
        }
        public void SetSolutionToRender(SolutionID id,bool isCalculated = true)
        {
            //выполнить конвертацию данных в списке и передать в рендер
            /*
            if (id.IndexPhase != _solutionIDtoRender.IndexPhase && id.StepLoad != _solutionIDtoRender.StepLoad)
            {
                var problemData = _geometryConverter.ConvertSolutionData(id, MultipleDeform, isCalculated);
                Renderer.InitializeGeometry(problemData);
            }
            */
            var problemData = _geometryConverter.ConvertSolutionData(id, MultipleDeform, isCalculated);
            Renderer.InitializeGeometry(problemData);
            Renderer.MouseWheelPressed();
            Renderer.InitializeText(problemData, _geometryConverter.GetTextToRender(id, isCalculated));
        }
        public void WriteSolutionToFile(string outFile)
        {
            Parser.WriteSolution(outFile, Fem.Solutions);
        }
    }
}
