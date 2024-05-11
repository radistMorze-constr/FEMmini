using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using System.Windows.Controls;
using ScottPlot.Drawing.Colormaps;
using Engine;

namespace FEMmini
{
    public abstract class Result 
    {
        public int Id { get; protected set; }
        public Result() 
        { }
    }

    public class ResultNode: Result 
    {
        /// <summary>
        /// Координата по деформированной схеме
        /// </summary>
        public double X { get; protected set; }
        /// <summary>
        /// Координата по деформированной схеме
        /// </summary>
        public double Y { get; protected set; }
        /// <summary>
        /// Смещение узла от нагрузке, приложенной на текущем шаге нагружения
        /// </summary>
        public double DeflectionX { get; private set; }
        /// <summary>
        /// Смещение узла от нагрузке, приложенной на текущем шаге нагружения
        /// </summary>
        public double DeflectionY { get; private set; }
        /// <summary>
        /// Накопленное смещение узла от полной нагрузки
        /// </summary>
        public double FullDeflectionX { get; private set; }
        /// <summary>
        /// Накопленное смещение узла от полной нагрузки
        /// </summary>
        public double FullDeflectionY { get; private set; }

        public ResultNode(ResultNode node) : base()
        {
            Id = node.Id; X = node.X; Y = node.Y;
            DeflectionX = node.DeflectionX; DeflectionY = node.DeflectionY;
            FullDeflectionX = node.FullDeflectionX; FullDeflectionY = node.FullDeflectionY;
        }
        public ResultNode(Node node) : base()
        {
            Id = node.Id; X = node.X; Y = node.Y;
        }
        public ResultNode() : base()
        { }
        public void Increase(double dx, double dy)
        {
            X += dx; Y += dy;
            DeflectionX = dx; DeflectionY = dy;
            FullDeflectionX += dx; FullDeflectionY += dy;
        }
    }
	
	public class ResultElement: Result
    {
        private Vector<double> _stress;
        private Vector<double> _stressPrincipal;
        private Vector<double> _stressAccum;
        private Vector<double> _strains;

        public Vector<double> StressElastic { get { return _stress; } }
        public Vector<double> StressAccum 
        { 
            get { return _stressAccum; } 
            set { _stressAccum = value; }
        }
        public Vector<double> StrainElastic { get { return _strains; } }
        public double StressX { get { return _stress[0]; } }
        public double StressY { get { return _stress[1]; } }
        public double StressXY { get { return _stress[2]; } }
        public double StressZ { get; private set; }
        public double Stress1 { get => _stressPrincipal[0]; }
        public double Stress3 { get => _stressPrincipal[2]; }
        public double StrainsX { get { return _strains[0]; } }
        public double StrainsY { get { return _strains[1]; } }
        public double StrainsXY { get { return _strains[2]; } }

        public ResultElement(ResultElement element) : base()
        {
            _stress = element._stress;
            _stressPrincipal = element._stressPrincipal;
            _stressAccum = element._stressAccum;
            _strains = element._strains;
            StressZ = element.StressZ;
            Id = element.Id;
        }
        public ResultElement(Element element) : this()
        {
            Id = element.Id;
        }
        public ResultElement() : base()
        {
            _stress = Vector<double>.Build.Dense(3, 0);
            _stressPrincipal = Vector<double>.Build.Dense(3, 0);
            _stressAccum = Vector<double>.Build.Dense(3, 0);
            _strains = Vector<double>.Build.Dense(3, 0);
        }
        public void Increase(double sxx, double syy, double sxy, double szz, 
                            double strainX, double strainY, double strainXY,
                            double s1, double s3) 
        {
            _stress[0] = sxx; _stress[1] = syy; _stress[2] = sxy; StressZ = szz;
            _strains[0] = strainX; _strains[1] = strainY; _strains[2] = strainXY;
            _stressPrincipal[0] = s1; _stressPrincipal[2] = s3;
        }
    }
}
