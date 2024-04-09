using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Engine;
using Microsoft.VisualBasic.ApplicationServices;

namespace FEMmini
{
    public class SolverManager
    {
        private SolutionID _solutionIDtoRender;
        private VisualNodeText _nodeResult = VisualNodeText.Nothing;
        private VisualElementText _elementResult = VisualElementText.Nothing;
        private VisualLineCenterText _lineCenterText = VisualLineCenterText.Nothing;
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
        public SolutionID SolutionID 
        {
            get => _solutionIDtoRender;
            set
            {
                _solutionIDtoRender = value;
                SetSolutionToRender(_solutionIDtoRender);
            }
        }
        public VisualNodeText NodeResult
        {
            get => _nodeResult;
            set
            {
                _nodeResult = value;
                if (_nodeResult == VisualNodeText.Nothing) return;
                SetTextToRender(_nodeResult);
            }
        }
        public VisualElementText ElementResult
        {
            get => _elementResult;
            set
            {
                _elementResult = value;
                if (_elementResult == VisualElementText.Nothing) return;
                SetTextToRender(_elementResult);
            }
        }
        public VisualLineCenterText LineCenterText
        {
            get => _lineCenterText;
            set
            {
                _lineCenterText = value;
                if (_lineCenterText == VisualLineCenterText.Nothing) return;
                SetTextToRender(_lineCenterText);
            }
        }

        public SolverManager()
        {
            Fem = new FEM(Geometry);
            _geometryConverter = new GeometryDataConverter(Fem, Geometry);
        }
        public string Name { get; set; } = "Новая задача";
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
            Geometry.Initialize(problemData.Elements, problemData.Nodes, problemData.MeshSetPhase);
            Fem.Initialize(problemData.LoadsNode, problemData.LoadsLine, problemData.LoadsSurface, 
                           problemData.Constraints, problemData.Properties, problemData.SolutionProperties);
            //Renderer.Borders = Geometry.Borders;
        }
        public void SetSolutionToRender(SolutionID id)
        {
            //выполнить конвертацию данных в списке и передать в рендер
            if (id.IndexPhase != _solutionIDtoRender.IndexPhase && id.StepLoad != _solutionIDtoRender.StepLoad)
            {
                Renderer.InitializeGeometry(_geometryConverter.ConvertSolutionData(id, MultipleDeform));
            }
            Renderer.InitializeGeometry(_geometryConverter.ConvertSolutionData(id, MultipleDeform));
        }
        public void SetTextToRender<T>(T enumText) where T : Enum
        {
            //нужно передавать массив индексов вершин в метод
            var indices = new uint[] { };



            Renderer.InitializeText(enumText, indices, _geometryConverter.GetTextToRender(SolutionID, enumText));
        }
        public void WriteSolutionToFile(string outFile)
        {
            Parser.WriteSolution(outFile, Fem.Solutions);
        }
    }
}
