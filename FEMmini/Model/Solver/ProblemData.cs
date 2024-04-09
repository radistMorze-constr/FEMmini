using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;
using Common;

namespace FEMmini
{
    public enum ProblemType
    {
        PlaneStress,
        PlaneStrain
    }

    public enum LinearityType
    {
        Linear,
        NonLinear
    }

    public class SolutionProperties
    {
        public int IndexPhase { get; set; }
        public int CountSteps { get; set; }
        /// <summary>
        /// Идексы всех групп граничных условий, которые активны на текущей фазе
        /// </summary>
        public List<int> ConstraintIDs { get; set; }
        /// <summary>
        /// Индексы узловых нагрузок, прикладываемых на текущей фазе к схеме
        /// </summary>
        public List<int> LoadNodeIDs { get; set; }
        /// <summary>
        /// Индексы линейных нагрузок, прикладываемых на текущей фазе к схеме
        /// </summary>
        public List<int> LoadLineIDs { get; set; }
        /// <summary>
        /// Индексы распределенных по площади нагрузок, прикладываемых на текущей фазе к схеме
        /// </summary>
        public List<int> LoadSurfaceIDs { get; set; }
        /// <summary>
        /// Индексы групп КЭ сетки активных на текущей фазе
        /// </summary>
        //public List<int> MeshsetIDs { get; set; }

        public SolutionProperties(int indexPhase, int countSteps, List<int> constraintIDs, List<int> loadNodeIDs, List<int> loadLineIDs, List<int> loadSurfaceIDs)
        {
            IndexPhase = indexPhase;
            CountSteps = countSteps;
            ConstraintIDs = constraintIDs;
            LoadNodeIDs = loadNodeIDs;
            LoadLineIDs = loadLineIDs;
            LoadSurfaceIDs = loadSurfaceIDs;
            //MeshsetIDs = meshsetIDs;
        }
    }

    public class Solution
    {
        public Dictionary<int, ResultNode> NodesResult { get; set; }
        public Dictionary<int, ResultElement> ElementResult { get; set; }
        public SolutionID ID { get; set; }
        public Solution(SolutionID id)
        {
            ID = id;
            NodesResult = new Dictionary<int, ResultNode>();
            ElementResult = new Dictionary<int, ResultElement>();
        }

        public Solution(SolutionID id, Dictionary<int, ResultNode> nodeResult, Dictionary<int, ResultElement> elementResult)
        {
            ID = id;
            NodesResult = nodeResult;
            ElementResult = elementResult;
        }
    }
}
