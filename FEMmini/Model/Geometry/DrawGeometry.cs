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
        /// Номера активных элементов
        /// </summary>
        public List<int> ElementActiveID { get; set; }
        /// <summary>
        /// Номера активных узлов
        /// </summary>
        public List<int> NodeActiveID { get; set; }
        public MeshSet(int id, List<int> elementActiveID, List<int> nodeActiveID)
        {
            Id = id;
            ElementActiveID = elementActiveID;
            NodeActiveID = nodeActiveID;
        }
    }

    public class DrawGeometry
    {
        public Dictionary<int, Element> Elements { get; private set; }
        public Dictionary<int, Node> Nodes { get; private set; }
        /// <summary>
        /// Наборы геометрии по фазам
        /// </summary>
        public Dictionary<int, MeshSet> MeshSetPhase { get; private set; }
        public VisibleRectangle Borders { get; private set; }
        public DrawGeometry()
        {

        }

        public void Initialize(Dictionary<int, Element> elements, Dictionary<int, Node> nodes, Dictionary<int, MeshSet> meshSetPhase)
        {
            Elements = elements;
            Nodes = nodes;
            MeshSetPhase = meshSetPhase;

            //FindLimits();
        }
        /*
        private void FindLimits()
        {
            var MinX = double.MaxValue; var MinY = double.MaxValue;
            var MaxX = double.MinValue; var MaxY = double.MinValue;

            foreach(var node in Nodes)
            {
                if (node.Value.X > MaxX) { MaxX = node.Value.X; }
                if (node.Value.X < MinX) { MinX = node.Value.X; }

                if (node.Value.Y > MaxY) { MaxY = node.Value.Y; }
                if (node.Value.Y < MinY) { MinY = node.Value.Y; }
            }

            var dx = MaxX - MinX;
            var dy = MaxY - MinY;

            MinX -= 0.1 * dx;
            MaxX += 0.1 * dx;
            MinY -= 0.1 * dy;
            MaxY += 0.1 * dy;
            Borders = new VisibleRectangle((float)MaxX, (float)MinX, (float)MaxY, (float)MinY);
        }
        */
    }
}
