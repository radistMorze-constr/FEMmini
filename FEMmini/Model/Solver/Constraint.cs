using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace FEMmini
{
    public enum ConstraintType
    {
        X,
        Y,
        XY
    }

    public class Constraints
    {
        public int Id { get; set; }
        public ConstraintType Type { get; set; }
        public List<int> Nodes { get; set; }
        public Constraints(int id, ConstraintType type, List<int> nodes) 
        {
            Id = id;
            Type = type;
            Nodes = nodes;
        }
        public void Apply(Vector<double> globalRight)
        {
            foreach (var node in Nodes) 
            {
                var index_i = node - 1;
                if (Type != ConstraintType.Y)
                {
                    globalRight[2 * index_i] = 0;
                }
                if (Type != ConstraintType.X)
                {
                    globalRight[2 * index_i + 1] = 0;
                }
            }
        }
        public void Apply(Matrix<double> globalMatrix)
        {
            int dimension = globalMatrix.RowCount;
            foreach (var node in Nodes)
            {
                for (int i = 0; i < dimension; ++i)
                {
                    if (Type != ConstraintType.X)
                    {
                        globalMatrix[2 * (node - 1) + 1, i] = 0;
                        globalMatrix[i, 2 * (node - 1) + 1] = 0;
                    }
                    if (Type != ConstraintType.Y)
                    {
                        globalMatrix[2 * (node - 1), i] = 0;
                        globalMatrix[i, 2 * (node - 1)] = 0;
                    }
                }
                if (Type != ConstraintType.X)
                {
                    globalMatrix[2 * (node - 1) + 1, 2 * (node - 1) + 1] = 1;
                }
                if (Type != ConstraintType.Y)
                {
                    globalMatrix[2 * (node - 1), 2 * (node - 1)] = 1;
                }
            }
        }
    }
}
