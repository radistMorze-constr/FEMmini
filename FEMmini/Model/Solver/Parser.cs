using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Security.Cryptography.X509Certificates;
using MathNet.Numerics.LinearAlgebra;
using System.Windows.Shapes;
using System.Linq;
using System.Text.Json;
using Common;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FEMmini
{
    public class TextProblemData
    {
        public string Name { get; set; } = string.Empty;
        public Dictionary<int, Element> Elements { get; set; } = new Dictionary<int, Element>();
        public Dictionary<int, Node> Nodes { get; set; } = new Dictionary<int, Node>();
        public Dictionary<int, MaterialModel> Properties { get; set; } = new Dictionary<int, MaterialModel>();
        public Dictionary<int, MeshSet> MeshSetPhase { get; set; } = new Dictionary<int, MeshSet>();
        public Dictionary<int, NodeLoad> LoadsNode { get; set; } = new Dictionary<int, NodeLoad>();
        public Dictionary<int, LineLoad> LoadsLine { get; set; } = new Dictionary<int, LineLoad>();
        public Dictionary<int, SurfaceLoad> LoadsSurface { get; set; } = new Dictionary<int, SurfaceLoad>();
        public Dictionary<int, Constraints> Constraints { get; set; } = new Dictionary<int, Constraints>();
        public Dictionary<int, SolutionProperties> SolutionProperties { get; set; } = new Dictionary<int, SolutionProperties>();
        public TextProblemData() { }
    }

    public static class Parser
    {
        private static string ReadLine(StreamReader reader)
        {
            string line;
            while(true)
            {
                line = reader.ReadLine();
                if (line == null)
                {
                    return null;
                }
                line = line.Trim('\t', ' ');
                //line = Regex.Replace(line, @"^(\\[rntv]|\s)*", "").Trim();
                if (line != null && line.StartsWith("//"))
                {
                    continue;
                }
                break;
            }
            return line;
        }
        #region Чтение задачи из файла
        public static TextProblemData ReadProblemData(string path)
        {
            var problemData = new TextProblemData();
            
            string line;
            using (StreamReader reader = new StreamReader(path))
            {
                while ((line = ReadLine(reader)) != null)
                {
                    switch (line)
                    {
                        case "<Имя задачи>":
                            ReadName(reader, problemData);
                            break;
                        case "<Узлы>":
                            ReadNodes(reader, problemData);
                            break;
                        case "<Тип жесткости>":
                            ReadProperties(reader, problemData);
                            break;
                        case "<Элементы>":
                            ReadElements(reader, problemData);
                            break;
                        case "<Набор геометрии Meshset>":
                            ReadMeshSetPhase(reader, problemData);
                            break;
                        case "<Нагрузки>":
                            ReadLoads(reader, problemData);
                            break;
                        case "<Граничные условия>":
                            ReadConstraints(reader, problemData);
                            break;
                        case "<Фазы расчета>":
                            ReadPhases(reader, problemData);
                            break;
                    }
                }
            }
            try
            {
                
            }
            catch (Exception)
            { }
            return problemData;
        }

        public static void ReadName(StreamReader reader, TextProblemData problemData)
        {
            string line;
            while ((line = ReadLine(reader)) != null)
            {
                //line = line.Trim();
                //if (line.StartsWith("//")) continue;
                if (line == "</Имя задачи>") break;
                problemData.Name += line;
            }
        }

        public static void ReadNodes(StreamReader reader, TextProblemData problemData)
        {
            string line;
            while ((line = ReadLine(reader)) != null)
            {
                if (line == "</Узлы>") break;
                var numbers = Array.ConvertAll(line.Split(' '), double.Parse);
                problemData.Nodes[(int)numbers[0]] = new Node((int)numbers[0], numbers[1], numbers[2], numbers[3]);
            }
        }

        public static void ReadProperties(StreamReader reader, TextProblemData problemData)
        {
            string line;
            while ((line = ReadLine(reader)) != null)
            {
                if (line == "</Тип жесткости>") break;
                var numbers = Array.ConvertAll(line.Split(' '), double.Parse);
                problemData.Properties[(int)numbers[0]] = new Elastic((int)numbers[0], numbers[1], numbers[2], numbers[3], (ProblemType)numbers[4]);
            }
        }

        public static void ReadElements(StreamReader reader, TextProblemData problemData)
        {
            string line;
            while ((line = ReadLine(reader)) != null)
            {
                if (line == "</Элементы>") break;
                var numbers = Array.ConvertAll(line.Split(' '), int.Parse);
                problemData.Elements[(int)numbers[0]] = new Element(numbers[0], numbers[2], numbers[3], numbers[4], numbers[1]);
            }
        }

        public static void ReadMeshSetPhase(StreamReader reader, TextProblemData problemData)
        {
            string line;
            while ((line = ReadLine(reader)) != null)
            {
                if (line == "</Набор геометрии Meshset>") break;
                if (line == "<Meshset>")
                {
                    ReadOneMeshSet(reader, problemData);
                }
            }
        }

        public static void ReadOneMeshSet(StreamReader reader, TextProblemData problemData)
        {
            string line;
            var id = int.Parse(reader.ReadLine());
            var elements = Array.ConvertAll(reader.ReadLine().Split(' '), int.Parse);
            var nodes = Array.ConvertAll(reader.ReadLine().Split(' '), int.Parse);
            problemData.MeshSetPhase[id] = new MeshSet(id, elements.ToList(), nodes.ToList());
            reader.ReadLine();
        }

        public static void ReadLoads(StreamReader reader, TextProblemData problemData)
        {
            string line;
            while ((line = ReadLine(reader)) != null)
            {
                if (line == "</Нагрузки>") break;
                if (line == "<Узлы>")
                {
                    ReadNodeLoad(reader, problemData);
                }
                if (line == "<Линия>")
                {
                    ReadLineLoad(reader, problemData);
                }
                if (line == "<По площади>")
                {
                    ReadSurfaceLoad(reader, problemData);
                }
            }
        }

        public static void ReadNodeLoad(StreamReader reader, TextProblemData problemData)
        {
            var id = int.Parse(ReadLine(reader));
            var x = double.Parse(ReadLine(reader));
            var y = double.Parse(ReadLine(reader));
            var nodes = Array.ConvertAll(ReadLine(reader).Split(' '), int.Parse).ToList();
            problemData.LoadsNode[id] = new NodeLoad(id, nodes, x, y);
            ReadLine(reader);
        }

        public static void ReadLineLoad(StreamReader reader, TextProblemData problemData)
        {
            string line;
            var id = int.Parse(ReadLine(reader));
            var x = double.Parse(ReadLine(reader));
            var y = double.Parse(ReadLine(reader));
            var nodes = new List<Tuple<int, int>>();
            while ((line = ReadLine(reader)) != "</Линия>")
            {
                var numbers = Array.ConvertAll(line.Split(' '), int.Parse);
                nodes.Add(new Tuple<int, int>(numbers[0], numbers[1]));
            }
            problemData.LoadsLine[id] = new LineLoad(id, nodes, x, y);
        }

        public static void ReadSurfaceLoad(StreamReader reader, TextProblemData problemData)
        {
            var id = int.Parse(ReadLine(reader));
            var x = double.Parse(ReadLine(reader));
            var y = double.Parse(ReadLine(reader));
            var elements = Array.ConvertAll(ReadLine(reader).Split(' '), int.Parse).ToList();
            problemData.LoadsSurface[id] = new SurfaceLoad(id, elements, x, y);
            ReadLine(reader);
        }

        public static void ReadConstraints(StreamReader reader, TextProblemData problemData)
        {
            string line;
            while ((line = ReadLine(reader)) != null)
            {
                if (line == "</Граничные условия>") break;
                if (line == "<Закрепление>")
                {
                    ReadConstraint(reader, problemData);
                }
            }
        }

        public static void ReadConstraint(StreamReader reader, TextProblemData problemData)
        {
            var id = int.Parse(ReadLine(reader));
            var type = (ConstraintType)int.Parse(ReadLine(reader));
            var nodes = Array.ConvertAll(ReadLine(reader).Split(' '), int.Parse).ToList();
            problemData.Constraints[id] = new Constraints(id, type, nodes);
            ReadLine(reader);
        }

        public static void ReadPhases(StreamReader reader, TextProblemData problemData)
        {
            string line;
            while ((line = ReadLine(reader)) != null)
            {
                if (line == "</Фазы расчета>") break;
                if (line == "<Фаза>")
                {
                    ReadPhase(reader, problemData);
                }
            }
        }

        public static void ReadPhase(StreamReader reader, TextProblemData problemData)
        {
            var id = int.Parse(ReadLine(reader));
            var count = int.Parse(ReadLine(reader));
            var id_constraints = ReadId(reader);
            var id_loadsNode = ReadId(reader);
            var id_loadsLine = ReadId(reader);
            var id_loadsSurface = ReadId(reader); ;
            problemData.SolutionProperties[id] = new SolutionProperties(id, count, id_constraints, id_loadsNode, id_loadsLine, id_loadsSurface);
            ReadLine(reader);
        }

        public static List<int> ReadId(StreamReader reader)
        {
            var str = ReadLine(reader);
            var result = new List<int>();
            if (str != "empty")
            {
                result = Array.ConvertAll(str.Split(' '), int.Parse).ToList();
            }
            return result;
        }
        #endregion

        #region Запись результата решения в файл
        public static void WriteSolution(string path, Dictionary<SolutionID, Solution> solutions)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                    foreach (var solution in solutions.Values) 
                    {
                        var json = JsonSerializer.Serialize(solution, options);
                        writer.Write(json);
                    }
                }
            }
            catch (Exception)
            { }
        }
        #endregion

        #region Запись в файл матрицы и вектора
        public static void WriteMatrix(string matrixPath, Matrix<double> matrix)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(matrixPath))
                {
                    for (int j = 0; j < matrix.RowCount; j++)
                    {
                        for (int i = 0; i < matrix.ColumnCount; i++)
                        {
                            writer.Write(matrix[j, i] + " ");
                        }
                        writer.WriteLine();
                    }
                }
            }
            catch (Exception)
            { }
        }

        public static void WriteVector(string vectorPath, Vector<double> vector)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(vectorPath))
                {
                    for (int j = 0; j < vector.Count; j++)
                    {
                        writer.Write(vector[j]);
                        writer.WriteLine();
                    }
                }
            }
            catch (Exception)
            { }
        }
        #endregion
    }
}
