using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public struct VisibleRectangle 
    {
        public float MinX, MaxX, MinY, MaxY;
        public VisibleRectangle(float MaxX, float MinX, float MaxY, float MinY)
        {
            this.MaxX = MaxX;
            this.MinX = MinX;
            this.MaxY = MaxY;
            this.MinY = MinY;
        }
    }
    public class VisaulMode 
    {
        public bool Node { get; set; }
        public bool Element { get; set; }
        public bool Constraint { get; set; }
        public bool LoadNode { get; set; }
        public bool LoadLine { get; set; }
        public bool LoadSurface { get; set; }
        public bool StiffnessColor { get; set; }
        public bool Text { get; set; }
        public bool IsDeformed { get; set; }
    }

    public struct SolutionID
    {
        public int IndexPhase { get; set; }
        public int StepLoad { get; set; }
        public SolutionID(int index, int step)
        {
            IndexPhase = index;
            StepLoad = step;
        }
    }
    public enum TypeToRender
    {
        Node,
        Element,
        Constraint,
        LoadNode,
        LoadLine,
        LoadSurface,
        Text
    }
    public enum VisualNodeText
    {
        Nothing,
        Id,
        LoadNodeValue,
        DeflectionX,
        DeflectionY
    }
    public enum VisualElementText
    {
        Nothing,
        Id,
        Stiffness,
        LoadSurfaceValue,
        SressX,
        SressY,
        SressXY,
        Sress1,
        Sress2,
        Sress3,
        SressMises
    }
    public enum VisualLineCenterText
    {
        Nothing,
        LoadLineValue
    }
    public class DataContainerToRender 
    {
        public DataContainerToRender(int vertNodes, int indicesNodes, int indicesElementsNodes, int indicesConstraints, 
            int indicesLoadNode, int allElementCount, int currentElementCount,
            int indicesLoadSurface, int lineLoadCount)
        {
            VertNodes = new float[3 * vertNodes];
            VertNodesDeformed = new float[3 * vertNodes];
            IndicesNodes = new uint[indicesNodes];
            IndicesElementsNodes = new uint[3 * indicesElementsNodes];
            IndicesConstraints = new uint[indicesConstraints];
            IndicesLoadNode = new uint[indicesLoadNode];
            VertElementCenter = new float[3 * allElementCount];
            VertElementCenterDeformed = new float[3 * currentElementCount];
            IndicesElementCenter = new uint[indicesElementsNodes];
            IndicesLoadSurface = new uint[indicesLoadSurface];
            VertLoadLineCenter = new float[3 * lineLoadCount];
            IndicesLoadLine = new uint[lineLoadCount];
        }
        /// <summary>
        /// Координаты всех узлов в схеме
        /// </summary>
        public float[] VertNodes { get; set; }
        /// <summary>
        /// Координаты всех узлов деформированной схемы (с учетом мультиплаера)
        /// </summary>
        public float[] VertNodesDeformed { get; set; }
        /// <summary>
        /// Индексы текущих вершин для отображения
        /// </summary>
        public uint[] IndicesNodes { get; set; }
        /// <summary>
        /// Индексы текущих вершин для отображения текущих элементов
        /// </summary>
        public uint[] IndicesElementsNodes { get; set; }
        /// <summary>
        /// Индексы узлов со связями
        /// </summary>
        public uint[] IndicesConstraints{ get; set; }
        /// <summary>
        /// Индексы узлов с нагрузкой
        /// </summary>
        public uint[] IndicesLoadNode { get; set; }
        /// <summary>
        /// Координаты ц.т. всех элементов в схеме
        /// </summary>
        public float[] VertElementCenter { get; set; }
        /// <summary>
        /// Координаты ц.т. всех элементов в деформированной схеме (с учетом мультиплаера)
        /// </summary>
        public float[] VertElementCenterDeformed { get; set; }
        /// <summary>
        /// Индексы центров элементов текущей фазы
        /// </summary>
        public uint[] IndicesElementCenter { get; set; }
        /// <summary>
        /// Индексы элементов с распределенной нагрузкой на текущей фазе
        /// </summary>
        public uint[] IndicesLoadSurface { get; set; }
        /// <summary>
        /// Координаты середин отрезков с линейными нагрузками
        /// </summary>
        public float[] VertLoadLineCenter { get; set; }
        /// <summary>
        /// Индексы с линейными нагрузками на текущей фазе
        /// </summary>
        public uint[] IndicesLoadLine { get; set; }
    }
}
