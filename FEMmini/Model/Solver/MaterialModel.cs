using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace FEMmini
{
    public enum Typematerial
    {
        Elastic,
        IdealPlasticity
    }

    public enum ModelParameter
    {
        Id,
        ModulElastic,
        PoisonRation,
        Density,
        AngleFriction,
        Cohesion,
        CtgPhi,
        Betta,
        S,
        T
    }

    public abstract class MaterialModel
    {
        protected double _EplaneStrain;
        protected double _NuplaneStrain;
        protected Matrix<double> _DplaneStrain;
        protected double _EplaneStress;
        protected double _NuplaneStress;
        protected Matrix<double> _DplaneStress;
        public ProblemType ProblemType { get; set; }
        protected Dictionary<ModelParameter, double> InfoToValue = new Dictionary<ModelParameter, double>();
        public double Id { get { return InfoToValue[ModelParameter.Id]; } }
        public double E 
        {
            get
            {
                if (ProblemType == ProblemType.PlaneStrain) return _EplaneStrain;
                else return _EplaneStress;
            }
        }
        public double Nu
        {
            get
            {
                if (ProblemType == ProblemType.PlaneStrain) return _NuplaneStrain;
                else return _NuplaneStress;
            }
        }
        public double Rhof { get { return InfoToValue[ModelParameter.Density]; } }
        public Matrix<double> D
        {
            get
            {
                if (ProblemType == ProblemType.PlaneStrain) return _DplaneStrain;
                else return _DplaneStress;
            }
        }

        protected MaterialModel(int id, double e, double nu, double rhof)
        {
            InfoToValue[ModelParameter.Id] = id;
            _EplaneStress = e;
            _NuplaneStress = nu;
            InfoToValue[ModelParameter.Density] = rhof;
            _EplaneStrain = e / (1 - nu * nu);
            _NuplaneStrain = nu / (1 - nu);
            {
                _DplaneStress = Matrix<double>.Build.Dense(3, 3, 0);
                _DplaneStress[0, 0] = 1; _DplaneStress[0, 1] = _NuplaneStress;
                _DplaneStress[1, 0] = _NuplaneStress; _DplaneStress[1, 1] = 1;
                _DplaneStress[2, 2] = (1 - _NuplaneStress) / 2;
                _DplaneStress *= _EplaneStress / (1 - Math.Pow(_NuplaneStress, 2));
            }
            {
                _DplaneStrain = Matrix<double>.Build.Dense(3, 3, 0);
                _DplaneStrain[0, 0] = 1; _DplaneStrain[0, 1] = _NuplaneStrain;
                _DplaneStrain[1, 0] = _NuplaneStrain; _DplaneStrain[1, 1] = 1;
                _DplaneStrain[2, 2] = (1 - _NuplaneStrain) / 2;
                _DplaneStrain *= _EplaneStrain / (1 - Math.Pow(_NuplaneStrain, 2));
            }
        }
        public double GetValue(ModelParameter type)
        {
            return InfoToValue[type];
        }
    }

    public class Elastic : MaterialModel
    {
        public Elastic(int num, double e, double nu, double rhof) :
            base(num, e, nu, rhof)
        { }
    }

    public class IdealPlasticity : MaterialModel
    {
        public double C { get { return InfoToValue[ModelParameter.Cohesion]; } }
        public double Phi { get { return InfoToValue[ModelParameter.AngleFriction]; } }
        public double CtgPhi { get { return InfoToValue[ModelParameter.CtgPhi]; } }
        public double Betta { get { return InfoToValue[ModelParameter.Betta]; } }
        public double S { get { return InfoToValue[ModelParameter.S]; } }
        public double T { get { return InfoToValue[ModelParameter.T]; } }
        public IdealPlasticity(int num, double e, double nu, double rhof, double c, double phi) :
            base(num, e, nu, rhof)
        {
            InfoToValue[ModelParameter.Cohesion] = c;
            InfoToValue[ModelParameter.AngleFriction] = phi;
            InfoToValue[ModelParameter.CtgPhi] = (1 + Math.Sin(phi)) / (1 - Math.Sin(phi));
            InfoToValue[ModelParameter.Betta] = Math.PI / 4;
            InfoToValue[ModelParameter.S] = 2 * c / Math.Tan(Math.PI / 4 - phi / 2);
            InfoToValue[ModelParameter.T] = -0.2 * c;
        }
    }
}
