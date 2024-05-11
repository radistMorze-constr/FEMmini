using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEMmini
{
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

            NodeVM.Clear();
            ElementVM.Clear();
            StiffnessVM.Clear();
            LoadsVM.Clear();

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
}
