using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Common
{
    public struct VisualStyle
    {
        public Color4 Color;
        public int LineWidth; public int PointWidth;
        public VisualStyle(Color4 color, int lineWidth, int pointWidth)
        {
            Color = color;
            LineWidth = lineWidth;
            PointWidth = pointWidth;
        }
    }

    public class GraphicSettings
    {
        /// <summary>
        /// Цвет фона
        /// </summary>
        public Color4 BackgroundColor { get; set; } = Color4.White;
        /// <summary>
        /// Стиль ребер конечных элементов
        /// </summary>
        public VisualStyle EdgeStyle { get; set; } = new VisualStyle(Color4.Black, 3, 0);
        /// <summary>
        /// Стиль конечных элементов
        /// </summary>
        public VisualStyle ElementStyle { get; set; } = new VisualStyle(Color4.Gray, 3, 0);
        /// <summary>
        /// Стиль конечных элементов
        /// </summary>
        public VisualStyle ConstraintStyle { get; set; } = new VisualStyle(Color4.Red, 1, 0);
        /// <summary>
        /// Стиль нагрузок
        /// </summary>
        public VisualStyle ForceStyle { get; set; } = new VisualStyle(Color4.Green, 1, 0);
        /// <summary>
        /// Стиль узлов
        /// </summary>
        public VisualStyle NodeStyle { get; set; } = new VisualStyle(Color4.Blue, 1, 5);
        public GraphicSettings() { }
    }
}
