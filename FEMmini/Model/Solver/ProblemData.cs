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

    public class PhaseCharacteristics
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
        /// Индексы мешсетов, добавленных на текущей фазе
        /// </summary>
        public List<int> MeshsetIDsAdded { get; set; }
        /// <summary>
        /// Индексы мешсетов, удаленных на текущей фазе
        /// </summary>
        public List<int> MeshsetIDsDeleted { get; set; }
        /// <summary>
        /// Индексы активных мешсетов на текущей фазе
        /// </summary>
        public List<int> MeshsetIDs{ get; set; }
        /// <summary>
        /// Индексы активных узлов на текущей фазе
        /// </summary>
        public List<int> NodeIDs { get; set; }
        /// <summary>
        /// Индексы активных элементов на текущей фазе
        /// </summary>
        public List<int> ElementIDs { get; set; }

        public PhaseCharacteristics(int indexPhase, int countSteps, List<int> meshsetIDsAdded, List<int> meshsetIDsDeleted, List<int> meshsetIDs, 
            List<int> constraintIDs, List<int> loadNodeIDs, List<int> loadLineIDs, List<int> loadSurfaceIDs,
            List<int> nodeIDs, List<int> elementIDs)
        {
            IndexPhase = indexPhase;
            CountSteps = countSteps;
            MeshsetIDsAdded = meshsetIDsAdded;
            MeshsetIDsDeleted = meshsetIDsDeleted;
            ConstraintIDs = constraintIDs;
            LoadNodeIDs = loadNodeIDs;
            LoadLineIDs = loadLineIDs;
            LoadSurfaceIDs = loadSurfaceIDs;
            MeshsetIDs = meshsetIDs;
            NodeIDs = nodeIDs;
            ElementIDs = elementIDs;
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
