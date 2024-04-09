using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using ScottPlot.Drawing.Colormaps;
using Engine;

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
        public DataContainerToRender ConvertSolutionData(SolutionID id, float multipleDeform)
        {
            var solution = _solver.GetSolution(id);
            var solutionProperty = _solver.GetSolutionProperty(id.IndexPhase);
            var phase = solution.ID.IndexPhase;
            var meshset = _geometry.MeshSetPhase[phase];
            var loadNode = _solver.LoadsNoad;
            var loadLine = _solver.LoadsLine;
            var loadSurface = _solver.LoadsSurface;
            var result = new DataContainerToRender(_geometry.Nodes.Count,
                meshset.NodeActiveID.Count,
                meshset.ElementActiveID.Count,
                solutionProperty.ConstraintIDs.Count,
                loadNode.Count,
                _geometry.Elements.Count,
                meshset.ElementActiveID.Count,
                loadSurface.Count,
                loadLine.Count);
            //VertNodes
            foreach (var node in _geometry.Nodes.Values)
            {
                var index = 3 * (node.Id - 1);
                result.VertNodes[index] = (float)node.X;
                result.VertNodes[index + 1] = (float)node.Y;
            }
            //VertNodesDeformed
            result.VertNodes.CopyTo(result.VertNodesDeformed, 0);
            foreach (var nodeResult in solution.NodesResult.Values)
            {
                var index = 3 * (nodeResult.Id - 1);
                result.VertNodesDeformed[index] += (float)nodeResult.FullDeflectionX * multipleDeform;
                result.VertNodesDeformed[index + 1] += (float)nodeResult.FullDeflectionY * multipleDeform;
            }
            //IndicesNodes
            result.IndicesNodes = meshset.NodeActiveID.Select(x => (uint)(x-1)).ToArray();
            //IndicesElementsNodes
            int i = 0;
            foreach(var elemId in meshset.ElementActiveID)
            {
                var nodes = _geometry.Elements[elemId].Nodes;
                result.IndicesElementsNodes[i] = (uint)(nodes[0] - 1);
                result.IndicesElementsNodes[i + 1] = (uint)(nodes[1] - 1);
                result.IndicesElementsNodes[i + 2] = (uint)(nodes[2] - 1);
                i += 3; //вроде бы надо индекс увеличить, наощуп действую
            }
            //IndicesConstraints
            var constraints = _solver.GetConstraint(solutionProperty.ConstraintIDs[0]);
            result.IndicesConstraints = constraints.Nodes.Select(x => (uint)(x - 1)).ToArray();
            //IndicesLoadNode
            result.IndicesLoadNode = solutionProperty.LoadNodeIDs.Select(x => (uint)x).ToArray();
            //VertElementCenter
            foreach (var elem in _geometry.Elements.Values)
            {
                var node1 = elem.Nodes[0];
                var node2 = elem.Nodes[1];
                var node3 = elem.Nodes[2];
                var x = (result.VertNodes[node1] + result.VertNodes[node2] + result.VertNodes[node3]) / 3;
                var y = (result.VertNodes[node1 + 1] + result.VertNodes[node2 + 1] + result.VertNodes[node3] + 1) / 3;
                result.VertElementCenter[elem.Id] = x;
                result.VertElementCenter[elem.Id + 1] = y;
            }
            //VertElementCenterDeformed
            result.VertElementCenter.CopyTo(result.VertElementCenterDeformed, 0);
            foreach (var elemId in meshset.ElementActiveID)
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
            //IndicesElementCenter
            result.IndicesElementCenter = meshset.ElementActiveID.Select(x => (uint)x).ToArray();
            //IndicesLoadSurface
            result.IndicesLoadSurface = solutionProperty.LoadSurfaceIDs.Select(x => (uint)x).ToArray();
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
            //IndicesLoadLine
            result.IndicesLoadLine = solutionProperty.LoadLineIDs.Select(x => (uint)x).ToArray();

            return result;
        }
        public List<string> GetTextToRender<T>(SolutionID id, T enumType) 
        {
            var result = new List<string>();
            var solution = _solver.GetSolution(id);
            var solutionProperty = _solver.GetSolutionProperty(id.IndexPhase);
            var phase = solution.ID.IndexPhase;
            var meshset = _geometry.MeshSetPhase[phase];
            var loadNode = _solver.LoadsNoad;
            var loadLine = _solver.LoadsLine;
            var loadSurface = _solver.LoadsSurface;
            if (enumType.GetType() == typeof(VisualNodeText))
            {
                var enumValue = ConvertToEnum<VisualNodeText>(enumType);
                if (enumValue == VisualNodeText.Id)
                {
                    result = meshset.NodeActiveID.Select(x => x.ToString()).ToList();
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
                    result = meshset.ElementActiveID.Select(x => x.ToString()).ToList();
                }
                if (enumValue == VisualElementText.Stiffness)
                {
                    foreach (var elem in meshset.ElementActiveID)
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
