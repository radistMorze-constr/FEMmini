using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using ScottPlot.Drawing.Colormaps;
using Engine;
using System.Data;
using System.Threading.Tasks.Sources;
using System.Windows.Documents;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;

namespace FEMmini
{
    public class GeometryDataConverter
    {
        FEM _solver;
        DrawGeometry _geometry;

        private T ConvertToEnum<T>(object o)
        {
            T enumVal = (T)Enum.Parse(typeof(T), o.ToString());
            return enumVal;
        }

        public GeometryDataConverter(FEM fem, DrawGeometry geometry)
        {
            _solver = fem;
            _geometry = geometry;
        }
        public DataContainerToRender ConvertSolutionData(SolutionID id, float multipleDeform, bool isCalculated = true)
        {
            var phase = id.IndexPhase;
            var phaseCharacteristics = _solver.GetPhaseCharacteristics(id.IndexPhase);
            var meshSetIds = phaseCharacteristics.MeshsetIDs;
            var nodesId = phaseCharacteristics.NodeIDs;
            var elementsId = phaseCharacteristics.ElementIDs;
            var loadNode = _solver.LoadsNoad;
            var loadNodeCount = 0;
            foreach (var x in loadNode.Values)
            {
                loadNodeCount += x.Nodes.Count;
            }
            var loadLine = _solver.LoadsLine;
            var loadLineCount = 0;
            foreach(var x in loadLine.Values)
            {
                loadLineCount += x.Nodes.Count;
            }
            var loadSurface = _solver.LoadsSurface;
            var loadSurfaceCount = 0;
            foreach (var x in loadSurface.Values)
            {
                loadSurfaceCount += x.Elements.Count;
            }
            var result = new DataContainerToRender(_geometry.Nodes.Count,
                nodesId.Count,
                elementsId.Count,
                phaseCharacteristics.ConstraintIDs.Count,
                loadNodeCount,
                _geometry.Elements.Count,
                loadSurfaceCount,
                loadLineCount);

            #region [Изначальная геометрия]
            //VertNodes
            foreach (var node in _geometry.Nodes.Values)
            {
                var index = 3 * (node.Id - 1);
                result.VertNodes[index] = (float)node.X;
                result.VertNodes[index + 1] = (float)node.Y;
            }
            //IndicesNodes
            result.IndicesNodes = nodesId.Select(x => (uint)(x-1)).ToArray();
            //IndicesElementsNodes
            int i = 0;
            foreach(var elemId in elementsId)
            {
                var nodes = _geometry.Elements[elemId].Nodes;
                result.IndicesElementsNodes[i] = (uint)(nodes[0] - 1);
                result.IndicesElementsNodes[i + 1] = (uint)(nodes[1] - 1);
                result.IndicesElementsNodes[i + 2] = (uint)(nodes[2] - 1);
                i += 3;
            }
            //IndicesConstraints
            var indicesConstraints = new List<int>();
            var constraintsTypes = new List<float>();
            constraintsTypes.AddRange(Enumerable.Repeat(0f, result.IndicesNodes.Count()));
            foreach (var constraintId in phaseCharacteristics.ConstraintIDs)
            {
                var constraint = _solver.GetConstraint(constraintId);
                indicesConstraints.AddRange(constraint.Nodes);
                foreach (var node in constraint.Nodes)
                {
                    constraintsTypes[node - 1] = (float)constraint.Type;
                }
            }
            result.IndicesConstraints = indicesConstraints.Select(x => (uint)(x - 1)).ToArray();
            //ConstraintsTypes
            result.ConstraintsTypes = constraintsTypes.ToArray();
            //VertElementCenter
            foreach (var elem in _geometry.Elements.Values)
            {
                var node1 = elem.Nodes[0] - 1;
                var node2 = elem.Nodes[1] - 1;
                var node3 = elem.Nodes[2] - 1;
                var x = (result.VertNodes[3 * node1] + result.VertNodes[3 * node2] + result.VertNodes[3 * node3]) / 3;
                var y = (result.VertNodes[3 * node1 + 1] + result.VertNodes[3 * node2 + 1] + result.VertNodes[3 * node3 + 1]) / 3;
                result.VertElementCenter[3 * (elem.Id - 1)] = x;
                result.VertElementCenter[3 * (elem.Id - 1) + 1] = y;
            }
            //IndicesElementCenter
            result.IndicesElementCenter = elementsId.Select(x => (uint)(x - 1)).ToArray();
            //VertLoadLineCenter
            foreach (var load in loadLine.Values)
            {
                var nodes = load.Nodes;
                foreach (var pairNode in nodes) 
                {
                    var node1 = pairNode.Item1;
                    var node2 = pairNode.Item2;
                    var x = (result.VertNodes[node1] + result.VertNodes[node2]) / 2;
                    var y = (result.VertNodes[node1 + 1] + result.VertNodes[node2] + 1) / 2;
                    result.VertLoadLineCenter[load.Id] = x;
                    result.VertLoadLineCenter[load.Id + 1] = y;
                }
            }
            #endregion

            #region [Нагрузки]
            //Контейнеры рабочие для работы с данными по нагрузкам
            var loadValues = new List<float>();
            var loadAngles = new List<float>();
            var loadMax = float.MinValue;
            //IndicesLoadNode
            var indicesLoadNode = new List<int>();
            foreach (var loadID in phaseCharacteristics.LoadNodeIDs)
            {
                var load = loadNode[loadID];
                indicesLoadNode.AddRange(load.Nodes);
                var loadXY = Math.Sqrt(load.ForceX * load.ForceX + load.ForceY * load.ForceY);
                if (loadMax < loadXY)
                {
                    loadMax = (float)loadXY;
                }
                loadValues.AddRange(Enumerable.Repeat((float)loadXY, load.Nodes.Count));
                var angle = Math.Asin(load.ForceY / loadXY);
                loadAngles.AddRange(Enumerable.Repeat((float)angle, load.Nodes.Count));
            }
            result.IndicesLoadNode = indicesLoadNode.Select(x => (uint)(x - 1)).ToArray();
            //LoadNodeSSBO
            for (int index = 0; index < loadValues.Count; ++index)
            {
                result.LoadNodeSSBO[index] = new LoadSSBO(loadValues[index] / loadMax, loadAngles[index]);
            }
            //IndicesLoadSurface
            var indicesLoadSurface = new List<int>();
            loadMax = float.MinValue;
            loadValues.Clear();
            loadAngles.Clear();
            foreach (var loadID in phaseCharacteristics.LoadSurfaceIDs)
            {
                var load = loadSurface[loadID];
                indicesLoadSurface.AddRange(load.Elements);
                var loadXY = Math.Sqrt(load.ForceX * load.ForceX + load.ForceY * load.ForceY);
                if (loadMax < loadXY)
                {
                    loadMax = (float)loadXY;
                }
                loadValues.AddRange(Enumerable.Repeat((float)loadXY, load.Elements.Count));
                var angle = Math.Asin(load.ForceY / loadXY);
                loadAngles.AddRange(Enumerable.Repeat((float)angle, load.Elements.Count));
            }
            result.IndicesLoadSurface = indicesLoadSurface.Select(x => (uint)(x - 1)).ToArray();
            //LoadNodeSSBO
            for (int index = 0; index < loadValues.Count; ++index)
            {
                result.LoadSurfaceSSBO[index] = new LoadSSBO(loadValues[index] / loadMax, loadAngles[index]);
            }
            //IndicesLoadLine
            var indicesLoadLine = new List<int>();
            loadMax = float.MinValue;
            loadValues.Clear();
            loadAngles.Clear();
            foreach (var loadID in phaseCharacteristics.LoadLineIDs)
            {
                var load = loadLine[loadID];
                var loadXY = Math.Sqrt(load.ForceX * load.ForceX + load.ForceY * load.ForceY);
                if (loadMax < loadXY)
                {
                    loadMax = (float)loadXY;
                }
                loadValues.AddRange(Enumerable.Repeat((float)loadXY, load.Nodes.Count));
                var angle = Math.Asin(load.ForceY / loadXY);
                loadAngles.AddRange(Enumerable.Repeat((float)angle, load.Nodes.Count));
                foreach (var loadTuple in load.Nodes)
                {
                    indicesLoadLine.Add(loadTuple.Item1);
                    indicesLoadLine.Add(loadTuple.Item2);
                }
            }
            result.IndicesLoadLine = indicesLoadLine.Select(x => (uint)(x - 1)).ToArray();
            //LoadLineSSBO
            for (int index = 0; index < loadValues.Count; ++index)
            {
                result.LoadLineSSBO[index] = new LoadSSBO(loadValues[index] / loadMax, loadAngles[index]);
            }
            #endregion

            #region [Деформированная схема]
            if (isCalculated)
            {
                var solution = _solver.GetSolution(id);
                //VertNodesDeformed
                result.VertNodes.CopyTo(result.VertNodesDeformed, 0);
                foreach (var nodeResult in solution.NodesResult.Values)
                {
                    var index = 3 * (nodeResult.Id - 1);
                    result.VertNodesDeformed[index] += (float)nodeResult.FullDeflectionX * multipleDeform;
                    result.VertNodesDeformed[index + 1] += (float)nodeResult.FullDeflectionY * multipleDeform;
                }
                //VertElementCenterDeformed
                result.VertElementCenter.CopyTo(result.VertElementCenterDeformed, 0);
                foreach (var elemId in elementsId)
                {
                    var nodes = _geometry.Elements[elemId].Nodes;
                    var node1 = nodes[0];
                    var node2 = nodes[1];
                    var node3 = nodes[2];
                    var x = (result.VertNodesDeformed[node1] + result.VertNodesDeformed[node2] + result.VertNodesDeformed[node3]) / 3;
                    var y = (result.VertNodesDeformed[node1 + 1] + result.VertNodesDeformed[node2 + 1] + result.VertNodesDeformed[node3] + 1) / 3;
                    result.VertElementCenterDeformed[elemId] = x;
                    result.VertElementCenterDeformed[elemId + 1] = y;
                }
            }
            #endregion
            return result;
        }
        public List<string> GetTextToRender<T>(SolutionID id, T enumType) 
        {
            var result = new List<string>();
            var solution = _solver.GetSolution(id);
            var solutionProperty = _solver.GetPhaseCharacteristics(id.IndexPhase);
            var phase = solution.ID.IndexPhase;
            var meshset = _geometry.MeshSets[phase];
            var loadNode = _solver.LoadsNoad;
            var loadLine = _solver.LoadsLine;
            var loadSurface = _solver.LoadsSurface;
            if (enumType.GetType() == typeof(VisualNodeText))
            {
                var enumValue = ConvertToEnum<VisualNodeText>(enumType);
                if (enumValue == VisualNodeText.Id)
                {
                    //result = meshset.NodeActiveID.Select(x => x.ToString()).ToList();
                }
                if (enumValue == VisualNodeText.LoadNodeValue)
                {
                    foreach (var loadId in solutionProperty.LoadNodeIDs)
                    {
                        var load = loadNode[loadId];
                        result.Add(load.ForceX.ToString());
                    }
                }
                if (enumValue == VisualNodeText.DeflectionX)
                {
                    foreach (var nodeResult in solution.NodesResult.Values)
                    {
                        result.Add(nodeResult.FullDeflectionX.ToString());
                    }
                }
                if (enumValue == VisualNodeText.DeflectionY)
                {
                    foreach (var nodeResult in solution.NodesResult.Values)
                    {
                        result.Add(nodeResult.FullDeflectionY.ToString());
                    }
                }
            }
            else if (enumType.GetType() == typeof(VisualElementText))
            {
                var enumValue = ConvertToEnum<VisualElementText>(enumType);
                if (enumValue == VisualElementText.Id)
                {
                    result = meshset.ElementsID.Select(x => x.ToString()).ToList();
                }
                if (enumValue == VisualElementText.Stiffness)
                {
                    foreach (var elem in meshset.ElementsID)
                    {
                        var prop = _geometry.Elements[elem];
                        result.Add(prop.Properties.ToString());
                    }
                }
                if (enumValue == VisualElementText.LoadSurfaceValue)
                {
                    foreach (var loadId in solutionProperty.LoadSurfaceIDs)
                    {
                        var load = loadSurface[loadId];
                        result.Add(load.ForceX.ToString());
                    }
                }
                if (enumValue == VisualElementText.SressX)
                {
                    foreach (var elemResult in solution.ElementResult.Values)
                    {
                        result.Add(elemResult.StressX.ToString());
                    }
                }
                if (enumValue == VisualElementText.SressY)
                {
                    foreach (var elemResult in solution.ElementResult.Values)
                    {
                        result.Add(elemResult.StressY.ToString());
                    }
                }
                if (enumValue == VisualElementText.SressXY)
                {
                    foreach (var elemResult in solution.ElementResult.Values)
                    {
                        result.Add(elemResult.StressXY.ToString());
                    }
                }
            }
            else if (enumType.GetType() == typeof(VisualLineCenterText))
            {
                var enumValue = ConvertToEnum<VisualLineCenterText>(enumType);
                if (enumValue == VisualLineCenterText.LoadLineValue)
                {
                    foreach (var loadId in solutionProperty.LoadLineIDs)
                    {
                        var load = loadLine[loadId];
                        result.Add(load.ForceX.ToString());
                    }
                }
            }
            return result;
        }
    }
}
