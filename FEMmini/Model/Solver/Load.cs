using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace FEMmini
{
    public enum TypeLoad
    { 
        Node,
        Line,
        Surface
    }
    public abstract class Load 
    {
        public int Id { get; set; }
        public double ForceX { get; set; }
        public double ForceY { get; set; }
        public Load(int id, double forceX, double forceY)
        {
            Id = id;
            ForceX = forceX;
            ForceY = forceY;
        }
            
        public abstract void Apply(DrawGeometry geometry, Dictionary<int, double> elementDeterminantC, Vector<double> globalRight);
    }

    public class NodeLoad : Load
    {
        public List<int> _nodes;
        public NodeLoad(int id, List<int> nodes, double forceX, double forceY) :
            base(id, forceX, forceY)
        {
            _nodes = nodes;
        }

        public override void Apply(DrawGeometry geometry, Dictionary<int, double> elementDeterminantC, Vector<double> globalRight)
        {
            foreach (var node in _nodes)
            {
                globalRight[2 * node] += ForceX;
                globalRight[2 * node + 1] += ForceY;
            }
        }
    }

    public class LineLoad : Load
    {
        public List<Tuple<int, int>> Nodes { get; private set; }
        public LineLoad(int id, List<Tuple<int, int>> nodes, double forceX, double forceY) :
            base(id, forceX, forceY)
        {
            Nodes = nodes;
        }
        public override void Apply(DrawGeometry geometry, Dictionary<int, double> elementDeterminantC, Vector<double> globalRight)
        {
            foreach (var node in Nodes)
            {
                var lengthY = Math.Abs(geometry.Nodes[node.Item2].Y - geometry.Nodes[node.Item1].Y);
                var lengthX = Math.Abs(geometry.Nodes[node.Item2].X - geometry.Nodes[node.Item1].X);
                var lineX = ForceX / 2 * lengthX;
                var lineY = ForceY / 2 * lengthX;
                globalRight[2 * (node.Item1 - 1)] += lineX;
                globalRight[2 * (node.Item2 - 1)] += lineX;

                globalRight[2 * (node.Item1 - 1) + 1] += lineY;
                globalRight[2 * (node.Item2 - 1) + 1] += lineY;
            }
        }
    }

    public class SurfaceLoad : Load
    {
        public List<int> Elements { get; private set; }
        public SurfaceLoad(int id, List<int> elements, double forceX, double forceY) :
            base(id, forceX, forceY)
        {
            Elements = elements;
        }
        public override void Apply(DrawGeometry geometry, Dictionary<int, double> elementDeterminantC, Vector<double> globalRight)
        {
            foreach (var index in Elements)
            {
                var elem = geometry.Elements[index];
                var nodes = elem.Nodes;
                var detC = elementDeterminantC[index];
                foreach (var node in nodes)
                {
                    globalRight[2 * node] += detC / 6 * ForceX;
                    globalRight[2 * node + 1] += detC / 6 * ForceY;
                }
            }
        }
    }
}