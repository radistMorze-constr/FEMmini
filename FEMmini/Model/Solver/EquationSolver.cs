using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Engine;
using MathNet.Numerics.LinearAlgebra;

namespace FEMmini
{
    //delegate IEnumerable<Load> LoadDelegate(List<int> loads);
    public abstract class EquationSolver
    {
        public int PositiveDirectionStrain { get; set; }
        private readonly DrawGeometry _geometry;
        private readonly Dictionary<int, PhaseCharacteristics> _phaseCharacteristics;
        private readonly Dictionary<int, MaterialModel> _properties;
        private Dictionary<int, double> _elementDeterminantC;
        private Dictionary<int, Matrix<double>> _elementMatrixB;
        /// <summary>
        /// Основной контейнер для результатов, постепенно по шагам накапливает и потом копируется в объект FEM
        /// </summary>
        private Dictionary<int, ResultNode> _nodesResult = new Dictionary<int, ResultNode>();
        /// <summary>
        /// Основной контейнер для результатов, постепенно по шагам накапливает и потом копируется в объект FEM
        /// </summary>
        private Dictionary<int, ResultElement> _elementResult = new Dictionary<int, ResultElement>();

        public EquationSolver(DrawGeometry geometry, Dictionary<int, PhaseCharacteristics> phaseCharacteristics, Dictionary<int, MaterialModel> properties)
        {
            _geometry = geometry;
            _phaseCharacteristics = phaseCharacteristics;
            _properties = properties;
            _elementMatrixB = new Dictionary<int, Matrix<double>>();
            _elementDeterminantC = new Dictionary<int, double>();
        }
        public virtual void ApplyConstraints(IEnumerable<Constraints> iteratorConstraint, Vector<double> globalRight, Matrix<double> globalMatrix)
        {
            foreach(var constraint in iteratorConstraint) 
            {
                constraint.Apply(globalMatrix);
                constraint.Apply(globalRight);
            }
        }
        public virtual void ApplyLoads(IEnumerable<Load> iteratorLoad, Vector<double> globalRight) 
        {
            foreach (var load in iteratorLoad)
            {
                load.Apply(_geometry, _elementDeterminantC, globalRight);
            }
        }
        private Matrix<double> CalcMatrixC(Element elem)
        {
            var x1 = _nodesResult[elem.Nodes[0]].X;
            var x2 = _nodesResult[elem.Nodes[1]].X;
            var x3 = _nodesResult[elem.Nodes[2]].X;
            var y1 = _nodesResult[elem.Nodes[0]].Y;
            var y2 = _nodesResult[elem.Nodes[1]].Y;
            var y3 = _nodesResult[elem.Nodes[2]].Y;
            var C = Matrix<double>.Build.DenseOfArray(new[,] {
                               { 1.0,  x1, y1},
                               {1.0, x2, y2},
                               {1.0,  x3, y3}});
            return C;
        }
        private void CalcMatrixB(Element elem, Matrix<double> C)
        {
            var inverseC = C.Inverse();
            var B = Matrix<double>.Build.Dense(3, 6, 0);
            for (int i = 0; i < 3; i++)
            {
                B[0, 2 * i + 0] = inverseC[1, i];
                B[0, 2 * i + 1] = 0.0f;
                B[1, 2 * i + 0] = 0.0f;
                B[1, 2 * i + 1] = inverseC[2, i];
                B[2, 2 * i + 0] = inverseC[2, i];
                B[2, 2 * i + 1] = inverseC[1, i];
            }
            _elementMatrixB[elem.Id] = B;
        }   
        public virtual Matrix<double> CalcLocalMatrix(Element elem)
        {
            var D = _properties[elem.Properties].D;
            var C = CalcMatrixC(elem);
            CalcMatrixB(elem, C);
            var B = _elementMatrixB[elem.Id];
            var detC = C.Determinant();
            _elementDeterminantC[elem.Id] = detC;
            var result = B.Transpose() * D * B * detC / 2;
            /*
            var path = "D:\\PetProjects\\3D_graphics\\Second task\\newIter\\LocalMatrix";
            path += elem.Id;
            path += ".txt";
            Parser.WriteMatrix(path, result);
            */
            return result;
        }   
        public virtual void CombineGlobal(Matrix<double> globalMatrix) 
        {
            foreach (var elem in _geometry.Elements.Values)
            {
                var locMatrix = CalcLocalMatrix(elem);
                //Заполнение глобальной матрицы
                for (int i = 0; i < 3; ++i)
                {
                    var index_i = elem.Nodes[i] - 1;
                    for (int j = 0; j < 3; ++j)
                    {
                        var index_j = elem.Nodes[j] - 1;
                        globalMatrix[2 * index_i, 2 * index_j] += locMatrix[2 * i, 2 * j];
                        globalMatrix[2 * index_i, 2 * index_j + 1] += locMatrix[2 * i, 2 * j + 1];
                        globalMatrix[2 * index_i + 1, 2 * index_j] += locMatrix[2 * i + 1, 2 * j];
                        globalMatrix[2 * index_i + 1, 2 * index_j + 1] += locMatrix[2 * i + 1, 2 * j + 1];
                    }
                }
            }
        }

        private void PrepareSolver()
        {
            foreach (var node in _geometry.Nodes)
            {
                _nodesResult[node.Key] = new ResultNode(node.Value);
            }
            foreach (var element in _geometry.Elements)
            {
                _elementResult[element.Key] = new ResultElement(element.Value);
            }
        }

        public virtual void SolveEquations(IEnumerable<Load> iteratorLoad, IEnumerable<Constraints> iteratorConstraint, Solution current)
        {
            PrepareSolver();
            var id_phase = current.ID.IndexPhase;
            var phaseCharacteristics = _phaseCharacteristics[id_phase];
            var nodes = phaseCharacteristics.NodeIDs;
            var size = 2 * (nodes.Count);
            var globalMatrix = Matrix<double>.Build.Dense(size, size, 0);
            var globalRight = Vector<double>.Build.Dense(size);

            CombineGlobal(globalMatrix);
            
            //var path = "D:\\PetProjects\\3D_graphics\\Frame geometry Second\\newIter\\GlobalMatrixBeforeGU.txt";
            //Parser.WriteMatrix(path, globalMatrix);
            
            ApplyLoads(iteratorLoad, globalRight);
            ApplyConstraints(iteratorConstraint, globalRight, globalMatrix);
            /*
            path = "D:\\PetProjects\\3D_graphics\\Frame geometry Second\\newIter\\GlobalMatrix.txt";
            Parser.WriteMatrix(path, globalMatrix);
            path = "D:\\PetProjects\\3D_graphics\\Frame geometry Second\\newIter\\RightVector.txt";
            Parser.WriteVector(path, globalRight);
            */
            var solution = globalMatrix.Solve(globalRight);
            /*
            path = "D:\\PetProjects\\3D_graphics\\Frame geometry Second\\newIter\\solutionVector.txt";
            Parser.WriteVector(path, solution);
            */
            foreach (var node in nodes)
            {
                _nodesResult[node].Increase(solution[2 * (node - 1)], solution[2 * (node - 1) + 1]);
            }
            CalculateStress();
            UpdateSolution(current);
        }
        private void UpdateSolution(Solution solution) 
        {
            var id_phase = solution.ID.IndexPhase;
            var phaseCharacteristics = _phaseCharacteristics[id_phase];
            var nodes = phaseCharacteristics.NodeIDs;
            var elements = phaseCharacteristics.ElementIDs;

            foreach (var node in nodes)
            {
                solution.NodesResult[node] = new ResultNode(_nodesResult[node]);
            }
            foreach (var elem in elements)
            {
                solution.ElementResult[elem] = new ResultElement(_elementResult[elem]);
            }
        }
        private void CalculateStress()
        {
            //_elementResult.Add(new ResultElement(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0));

            for (int i = 0; i < _geometry.Elements.Count; ++i)
            {
                //_elementResult.Add(CalcElementStress(_geom._elements[i], i+1));
                CalcElementStress(_geometry.Elements[i + 1], i + 1, _elementResult[i + 1]);
            }
        }
        private void CalcElementStress(Element elem, int num, ResultElement resultElem)
        {
            double strainX, strainXY, strainY;
            double sxx, sxy, syy, szz;

            var index = elem.Properties;
            var D = _properties[index].D;
            var B = _elementMatrixB[elem.Id];

            var dx1 = _nodesResult[elem.Nodes[0]].DeflectionX;
            var dx2 = _nodesResult[elem.Nodes[1]].DeflectionX;
            var dx3 = _nodesResult[elem.Nodes[2]].DeflectionX;
            var dy1 = _nodesResult[elem.Nodes[0]].DeflectionY;
            var dy2 = _nodesResult[elem.Nodes[1]].DeflectionY;
            var dy3 = _nodesResult[elem.Nodes[2]].DeflectionY;

            var displacements = Vector<double>.Build.DenseOfArray(new[] { dx1, dy1, dx2, dy2, dx3, dy3 });
            var strains = B * displacements;
            //Уножаю на коэф. направления определяем знак отн. деформации растяжения
            strains *= PositiveDirectionStrain;
            strainX = strains[0];
            strainY = strains[1];
            strainXY = strains[2];
            var sigma = D * strains;
            sxx = sigma[0];
            syy = sigma[1];
            sxy = sigma[2];
            szz = 0;
            /*
            if (_problemType == ProblemType.PlaneStrain)
            {
                szz = _properties[index].Nu * (sxx + syy);
            }
            */
            var s1 = 0.5 * (sxx + syy + Math.Sqrt(Math.Pow(sxx - syy, 2) + 4 * Math.Pow(sxy, 2)));
            var s3 = 0.5 * (sxx + syy - Math.Sqrt(Math.Pow(sxx - syy, 2) + 4 * Math.Pow(sxy, 2)));
            resultElem.Increase(sxx, syy, sxy, szz,
                                            strainX, strainY, strainXY,
                                            s1, s3);
        }
    }

    public class SolverPlaneLinear : EquationSolver
    {
        public SolverPlaneLinear(DrawGeometry geometry, Dictionary<int, PhaseCharacteristics> phaseCharacteristics, Dictionary<int, MaterialModel> properties) : 
            base(geometry, phaseCharacteristics, properties) { }
    }

    public class SolverPlaneNonLinear : EquationSolver
    {
        public SolverPlaneNonLinear(DrawGeometry geometry, Dictionary<int, PhaseCharacteristics> phaseCharacteristics, Dictionary<int, MaterialModel> properties) : 
            base(geometry, phaseCharacteristics, properties) { }
        public virtual void SolveEquations(List<Load> loads, List<Constraints> constraints, Solution current)
        {
            /*
            CombineGlobal();
            var elementInPlastic = new List<int>();
            var flag = true;
            while (flag)
            {
                elementInPlastic.Clear();
                ++IterationCount;
                flag = false;
                SolveEquations();
                CalculateStress();
                for (int i = 0; i < _geometry.Elements.Count; ++i)
                {
                    if (CalcIteration(_geometry.Elements[i], ElementResult[i + 1]))
                    {
                        flag = true;
                        elementInPlastic.Add(i + 1);
                    }
                }
                if (IterationCount == 100)
                {
                    break;
                }
            }
            */
        }
        private bool CalcIteration(Element elem, ResultElement result)
        {
            /*
            //Коэффициент ускорения итерационного процесса
            const double k = 1.5;
            //Процент сходимости по напряжениям
            const double residual = 0.05;

            var index = elem.Properties;
            var model = (IdealPlasticity)_properties[index];

            var E = model.E;
            var nu = model.Nu;
            var phi = model.Phi;
            var c = model.C;
            var ctgPsi = model.CtgPhi;
            var betta = model.Betta; //Угол, определяющий закон пластического течения
            var S = model.S;
            var T = model.T;

            //Вычислим главные фактические напряжения за вычетом накопленных начальных напряжений
            var Gfact = result.StressElastic - result.StressAccum;
            var Gfact1 = 0.5 * (Gfact[0] + Gfact[1] + Math.Sqrt(Math.Pow(Gfact[0] - Gfact[1], 2) + 4 * Math.Pow(Gfact[2], 2)));
            var Gfact3 = 0.5 * (Gfact[0] + Gfact[1] - Math.Sqrt(Math.Pow(Gfact[0] - Gfact[1], 2) + 4 * Math.Pow(Gfact[2], 2)));
            //В ФОРМУЛЕ ВЫЧИСЛЕНИЯ УГЛА В ЗНАМЕНАТЕЛЕ ДВОЙКА??
            //В ВЫЧИСЛЕНИИ УГЛА ОШИБКА
            //var alpha = 0.5 * Math.Atan(Gfact[2] / (Gfact1 - Gfact[1])); // угол в радианах
            var alpha = Math.Atan(Gfact[2] / (Gfact1 - Gfact[1])); // угол в радианах

            //Вычислим главные деформации для рассчитанных напряжений
            var Sfact = result.StrainElastic;
            /*
            var Strain1 = 0.5 * (Sfact[0] + Sfact[1] + Math.Sqrt(Math.Pow(Sfact[0] - Sfact[1], 2) + 4 * Math.Pow(Sfact[2], 2)));
            var Strain3 = 0.5 * (Sfact[0] + Sfact[1] - Math.Sqrt(Math.Pow(Sfact[0] - Sfact[1], 2) + 4 * Math.Pow(Sfact[2], 2)));
            //В ФОРМУЛЕ ВЫЧИСЛЕНИЯ УГЛА В ЗНАМЕНАТЕЛЕ ДВОЙКА??
            var poAngle = 0.5*Math.Atan(0.5*Sfact[2]/(Strain1 - Sfact[1])); //Наклон площадки главных деформаций в радианах
            
            //Вычисляем главные теоритические напряжения по главным деформациям
            var Gtheory3 = (E*(Strain1 + Strain3) + S*(nu - 1)) / (1 - nu / Math.Tan(betta) + 1 / Math.Tan(betta) - nu);
            var Gtheory1 = S + Gtheory3 * ctgPsi;
            
            //Вычисляем главные теориwические напряжения в зависимости от положения точки на графике
            double Gtheory3, Gtheory1;
            var G1_limit = S + Gfact3 * ctgPsi;
            var G1_II = S - Math.Tan(betta) * Gfact3;
            //Точка лежит в урпугой зоне I, корректировка напряжений не требуется
            if (Gfact3 >= T && Gfact1 <= G1_limit)
            {
                Gtheory1 = Gfact1;
                Gtheory3 = Gfact3;
                var stepStress = Vector<double>.Build.DenseOfArray(new[] { Gtheory1, Gtheory3, Gtheory1, Gtheory3 });
                result.AddStressStep(stepStress);
                return false;
            }
            //Точка лежит в пластической зоне II
            else if (Gfact3 < 0 && Gfact1 >= G1_II || Gfact3 >= 0 && Gfact1 >= G1_limit)
            {
                Gtheory3 = (Gfact1 - S + Math.Tan(betta) * Gfact3) / (Math.Tan(betta) + ctgPsi);
                Gtheory1 = S + Gtheory3 * ctgPsi;
            }
            //Точка лежит в пластической зоне III
            else if (Gfact3 <= 0 && Gfact1 >= S && Gfact1 <= G1_limit)
            {
                Gtheory1 = S;
                Gtheory3 = 0;
            }
            //Точка лежит в пластической зоне IV
            else if (Gfact3 <= 0 && Gfact1 <= S && Gfact1 >= 0)
            {
                Gtheory1 = Gfact1;
                Gtheory3 = 0;
            }
            //Точка лежит в пластической зоне V
            else
            {
                Gtheory1 = 0;
                Gtheory3 = 0;
            }

            //Теоретические напряжения
            var Gtheory = Vector<double>.Build.Dense(3);
            Gtheory[0] = Gtheory1 * Math.Pow(Math.Cos(alpha), 2) + Gtheory3 * Math.Pow(Math.Sin(alpha), 2);
            Gtheory[1] = Gtheory1 * Math.Pow(Math.Sin(alpha), 2) + Gtheory3 * Math.Pow(Math.Cos(alpha), 2);
            Gtheory[2] = 0.5 * (Gtheory1 - Gtheory3) * Math.Sin(2 * alpha);

            //Приращения начальных напряжений
            var deltaGaccum = k * (Gfact - Gtheory);
            result.StressAccum += deltaGaccum;
            var Fnodes = elem.B.Transpose() * deltaGaccum;
            AddAccumForces(Fnodes, elem.Nodes);

            //Сохраним текущий шаг с напряжениями в историю
            {
                var stepStress = Vector<double>.Build.DenseOfArray(new[] { Gtheory1, Gtheory3, Gfact1, Gfact3 });
                result.AddStressStep(stepStress);
            }

            //Проверка на продолжение итераций deltaGaccum > residual * Gtheory
            return StressComparator(deltaGaccum, Gtheory, residual);
            */
            return true; 
        }
        private void AddAccumForces(Vector<double> Fnodes, List<int> nodes) 
        {
            /*
            //Заполнение вектора правой части
            for (int i = 0; i < 3; ++i)
            {
                var index = nodes[i] - 1;
                _globalRight[2 * index] += Fnodes[2 * i];
                _globalRight[2 * index + 1] += Fnodes[2 * i + 1];
            }
            */
        }
        private bool StressComparator(Vector<double> left, Vector<double> right, double residual) 
        {
           /*
            if (left[0] > right[0] * residual || left[1] > right[1] * residual || left[2] > right[2] * residual)
            {
                return true;
            }
           */
            return false;
        }
    }
}
