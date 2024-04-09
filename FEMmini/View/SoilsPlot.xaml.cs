using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ScottPlot;

namespace FEMmini
{
    /// <summary>
    /// Логика взаимодействия для SoilsPlot.xaml
    /// </summary>
    public partial class SoilsPlot : UserControl
    {
        public SoilsPlot()
        {
            InitializeComponent();
        }

        private void DrawSoilGraph(object sender, EventArgs e)
        {
            /*
            var indexElem = int.Parse(ElemComboBox.Text);
            var indexIter = int.Parse(IterationComboBox.Text);
            var result = _elementResult[indexElem];
            var soilModel = (NonLinearModel)_properties[(int)result.Properties];

            var plt1 = SoilStateChart.Plot;
            var plt2 = SoilStrength.Plot;
            plt1.Clear();
            plt2.Clear();
            double[] x1, x2, x3, x4, y1, y2, y3, y4;
            var stress = result.GetStressStep(indexIter);
            var G1t = stress[0];
            var G3t = stress[1];
            var G1f = stress[2];
            var G3f = stress[3];
            //var G1 = result.GetValue(InfoType.Stress1);
            //var G3 = result.GetValue(InfoType.Stress3);
            var radiusT = (G1t - G3t) / 2;
            var radiusF = (G1f - G3f) / 2;
            var T = soilModel.T;
            var S = soilModel.S;
            var phi = soilModel.Phi;
            var tanPhi = Math.Tan(phi);
            var c = soilModel.C;
            var ctgPsi = soilModel.CtgPhi;
            var betta = soilModel.Betta;

            //Первый график
            {
                x1 = new double[] { T, T, 1.5 * G1t };
                y1 = new double[] { 0, c + T * tanPhi, c + 1.5 * G1t * tanPhi };
                x2 = new double[] { -c / tanPhi, T };
                y2 = new double[] { 0, c + T * tanPhi };

                //Круг Мора и предельная поверхность
                //Теоретические напряжения
                plt1.AddCircle(x: G3t + radiusT, y: 0, radius: radiusT, lineStyle: LineStyle.Solid, lineWidth: 3);
                //Фактиечские напряжения
                plt1.AddCircle(x: G3f + radiusF, y: 0, radius: radiusF, lineStyle: LineStyle.Solid, lineWidth: 3);
                plt1.AddScatterLines(x1, y1, System.Drawing.Color.Green, lineWidth: 3);
                plt1.AddScatterLines(x2, y2, System.Drawing.Color.Green, lineStyle: LineStyle.Dash, lineWidth: 1);

                //Точки напряжений G1, G3
                //Теоретические
                plt1.AddPoint(G1t, 0, System.Drawing.Color.Red, 8);
                plt1.AddPoint(G3t, 0, System.Drawing.Color.Red, 8);
                //Фактиечские
                plt1.AddPoint(G1f, 0, System.Drawing.Color.Black, 8);
                plt1.AddPoint(G3f, 0, System.Drawing.Color.Black, 8);
                var textG3t = string.Format("G3t={0}", G3t.Round(1));
                var textG1t = string.Format("G1t={0}", G1t.Round(1));
                plt1.AddText(textG3t, G3t, 0, size: 16, color: System.Drawing.Color.Blue);
                plt1.AddText(textG1t, G1t, 0, size: 16, color: System.Drawing.Color.Blue);

                //Оси из точки нуля
                plt1.AddHorizontalLine(0, color: System.Drawing.Color.Black);
                plt1.AddVerticalLine(0, color: System.Drawing.Color.Black);

                //Текстовые примечания
                plt1.Title("НДС в осях G-T");
                plt1.XLabel("Нормальные напряжения G (кН)");
                plt1.YLabel("Касательные напряжения T (кН)");

                SoilStateChart.Plot.SetAxisLimits(-1.5 * S / tanPhi, 1.5 * G1t,
                                                  -1.5 * S / tanPhi, 1.5 * G1t);
                SoilStateChart.Refresh();
            }

            //Второй график
            {
                x1 = new double[] { T, T, 1.5 * G3t };
                y1 = new double[] { T, S + T * ctgPsi, S + 1.5 * G3t * ctgPsi };
                var left_down = 1.5 * Math.Min(T, G3t);
                var right_up = 1.5 * Math.Max(G3t, S + G3t * ctgPsi);
                x2 = new double[] { 0, left_down };
                y2 = new double[] { S, S - left_down * Math.Tan(betta) };
                x3 = new double[] { 0, left_down };
                y3 = new double[] { S, S };

                //Ось отсечения возможных напряжений
                plt2.AddFunction(x => x, System.Drawing.Color.Blue, lineStyle: LineStyle.Dash);

                //Поверхность прочности
                plt2.AddScatterLines(x1, y1, System.Drawing.Color.Green, lineWidth: 3); //Поверхность прочности
                //plt2.AddScatterLines(x2, y2, System.Drawing.Color.Green, lineWidth: 3);
                plt2.AddScatterLines(x2, y2, System.Drawing.Color.Blue, lineStyle: LineStyle.Dash); //Граница между областями II и III
                plt2.AddScatterLines(x3, y3, System.Drawing.Color.Blue, lineStyle: LineStyle.Dash); //Граница между областями IV и III

                //Оси из точки нуля
                plt2.AddHorizontalLine(0, color: System.Drawing.Color.Black);
                plt2.AddVerticalLine(0, color: System.Drawing.Color.Black);

                //Текстовые примечания
                plt2.Title("НДС в осях G3-G1");
                plt2.XLabel("Главное напряжения G3 (кН)");
                plt2.YLabel("Главное напряжения G1 (кН)");

                //Точки напряжений G1, G3
                //Теоретические
                plt2.AddPoint(G3t, G1t, System.Drawing.Color.Red, 8);
                //Фактиечские
                plt2.AddPoint(G3f, G1f, System.Drawing.Color.Black, 8);
                var textGt = string.Format("({0};{1})", G3t.Round(1), G1t.Round(1));
                var textGf = string.Format("({0};{1})", G3f.Round(1), G1f.Round(1));
                plt2.AddText(textGt, G3t, G1t, size: 16, color: System.Drawing.Color.Blue);
                plt2.AddText(textGf, G3f, G1f, size: 16, color: System.Drawing.Color.Blue);

                plt2.SetAxisLimits(left_down, right_up, left_down, right_up);
                SoilStrength.Refresh();
            }
            */
        }

        private void UpdateData(object sender, EventArgs e)
        {
            /*
            FillTables();
            for (int i = 0; i < _elementResult.Count(); i++)
            {
                ElemComboBox.Items.Add(i);
            }
            for (int i = 0; i < fem.IterationCount; i++)
            {
                IterationComboBox.Items.Add(i);
            }
            */
        }

        private void FillTables()
        {
            /*
            var elementsInfo = new List<Dictionary<String, double>>();
            foreach (var result in _elementResult)
            {
                elementsInfo.Add(new Dictionary<String, Double>()
                     {
                         {"Num",result.Num},
                         {"Sxx",result.GetValue(InfoType.StressX)},
                         {"Syy",result.GetValue(InfoType.StressY)},
                         {"Sxy",result.GetValue(InfoType.StressXY)},
                         {"Szz",result.GetValue(InfoType.StressZ)},
                         {"S1",result.GetValue(InfoType.Stress1)},
                         {"S3",result.GetValue(InfoType.Stress3)}
                     });
            }
            foreach (var node in elementsInfo.First())
            {
                elemsGrid.Columns.Add(new DataGridTextColumn { Header = node.Key, Binding = new Binding(string.Format("[{0}]", node.Key)) });
            }
            elemsGrid.AutoGenerateColumns = false; //Remove redundant columns
            elemsGrid.CanUserAddRows = false; //Remove extra rows
            elemsGrid.ItemsSource = elementsInfo;

            var nodesInfo = new List<Dictionary<String, double>>();
            foreach (var result in _nodesResult)
            {
                nodesInfo.Add(new Dictionary<String, Double>()
                     {
                         {"Num",result.Num},
                         {"X",result.X},
                         {"Y",result.Y},
                         {"dx",result.GetValue(InfoType.DeflectionX)},
                         {"dy",result.GetValue(InfoType.DeflectionY)}
                     });
            }
            foreach (var node in nodesInfo.First())
            {
                nodesGrid.Columns.Add(new DataGridTextColumn { Header = node.Key, Binding = new Binding(string.Format("[{0}]", node.Key)) });
            }
            nodesGrid.ItemsSource = nodesInfo;
            nodesGrid.AutoGenerateColumns = false; //Remove redundant columns
            nodesGrid.CanUserAddRows = false; //Remove extra rows

            propertiesGrid.ItemsSource = _properties;
            */
        }
    }
}
