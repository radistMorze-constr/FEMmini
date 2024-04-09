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
        protected Dictionary<ModelParameter, double> InfoToValue = new Dictionary<ModelParameter, double>();
        public double Id { get { return InfoToValue[ModelParameter.Id]; } }
        public double E { get { return InfoToValue[ModelParameter.ModulElastic]; } }
        public double Nu { get { return InfoToValue[ModelParameter.PoisonRation]; } }
        public double Rhof { get { return InfoToValue[ModelParameter.Density]; } }
        public Matrix<double> D { get; private set; }

        protected MaterialModel(int id, double e, double nu, double rhof, ProblemType problemType)
        {
            InfoToValue[ModelParameter.Id] = id;
            InfoToValue[ModelParameter.ModulElastic] = e;
            InfoToValue[ModelParameter.PoisonRation] = nu;
            InfoToValue[ModelParameter.Density] = rhof;
            if (problemType == ProblemType.PlaneStrain)
            {
                InfoToValue[ModelParameter.ModulElastic] = e / (1 - nu * nu);
                InfoToValue[ModelParameter.PoisonRation] = nu / (1 - nu);
            }
            D = Matrix<double>.Build.Dense(3, 3, 0);
            D[0, 0] = 1; D[0, 1] = Nu;
            D[1, 0] = Nu; D[1, 1] = 1;
            D[2, 2] = (1 - Nu) / 2;
            D *= E / (1 - Math.Pow(Nu, 2));
        }
        public double GetValue(ModelParameter type)
        {
            return InfoToValue[type];
        }
    }

    public class Elastic : MaterialModel
    {
        public Elastic(int num, double e, double nu, double rhof, ProblemType problemType) :
            base(num, e, nu, rhof, problemType)
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
        public IdealPlasticity(int num, double e, double nu, double rhof, ProblemType problemType, double c, double phi) :
            base(num, e, nu, rhof, problemType)
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
