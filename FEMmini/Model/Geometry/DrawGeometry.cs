using Common;
using MathNet.Numerics.LinearAlgebra;
using OpenTK.Graphics.ES20;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Media;

namespace FEMmini
{
    public class Node 
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Node(int id, double x, double y, double z) 
        {
            Id = id;
            X = x;
            Y = y;
            Z = z;
        }
    }

    public class Element
    {
        public int Id { get; set; }
        public List<int> Nodes { get; set; }
        public int Properties { get; set; }

        public Element(int id, int node1, int node2, int node3, int properties = 0)
        {
            Id = id;
            Nodes = new List<int> { node1, node2, node3 };
            Properties = properties;
        }
    }

    public class MeshSet 
    {
        public int Id { get; set; }
        /// <summary>
        /// Номера элементов мешсета
        /// </summary>
        public List<int> ElementsID { get; set; }
        public MeshSet(int id, List<int> elementActiveID)
        {
            Id = id;
            ElementsID = elementActiveID;
        }
    }

    public class DrawGeometry
    {
        public Dictionary<int, Element> Elements { get; private set; }
        public Dictionary<int, Node> Nodes { get; private set; }
        /// <summary>
        /// Наборы геометрии по фазам
        /// </summary>
        public Dictionary<int, MeshSet> MeshSets { get; private set; }
        public VisibleRectangle Borders { get; private set; }
        public DrawGeometry()
        {

        }

        public void Initialize(Dictionary<int, Element> elements, Dictionary<int, Node> nodes, Dictionary<int, MeshSet> meshSetPhase)
        {
            Elements = elements;
            Nodes = nodes;
            MeshSets = meshSetPhase;
        }
    }
}
