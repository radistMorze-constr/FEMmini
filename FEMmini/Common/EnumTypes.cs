using FEMmini;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public enum VBOEnum
    {
        Node,
        NodeDeformed,
        Element,
        ElementDeformed,
        LoadLine,
        ConstraintType,
        LoadValue,
        LaodAngel
    }
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
        public DataContainerToRender(int vertNodes, int indicesNodes, int currentElementCount, int indicesConstraints, 
            int indicesLoadNode, int allElementCount,
            int indicesLoadSurface, int lineLoadCount)
        {
            VertNodes = new float[3 * vertNodes];
            VertNodesDeformed = new float[3 * vertNodes];
            IndicesNodes = new uint[indicesNodes];
            IndicesElementsNodes = new uint[3 * currentElementCount];
            IndicesConstraints = new uint[indicesConstraints];
            ConstraintsTypes = new float[indicesConstraints];
            IndicesLoadNode = new uint[indicesLoadNode];
            VertElementCenter = new float[3 * allElementCount];
            VertElementCenterDeformed = new float[3 * currentElementCount];
            IndicesElementCenter = new uint[currentElementCount];
            IndicesLoadSurface = new uint[indicesLoadSurface];
            VertLoadLineCenter = new float[3 * lineLoadCount];
            IndicesLoadLine = new uint[lineLoadCount];
            LoadValue = new float[lineLoadCount];
            LoadAngle = new float[lineLoadCount];
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
        /// Идентификаторы типов связей: 0(X), 1(Y), 2(XY)
        /// </summary>
        public float[] ConstraintsTypes { get; set; }
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
        /// <summary>
        /// Коэффициенты нагрузки от максимума (модуль длины для рендера)
        /// </summary>
        public float[] LoadValue { get; set; }
        /// <summary>
        /// Углы наклона нагрузок
        /// </summary>
        public float[] LoadAngle { get; set; }
    }
}
