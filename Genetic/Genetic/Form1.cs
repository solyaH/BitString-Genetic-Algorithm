using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Numerics;
using System.Windows.Forms.DataVisualization.Charting;

namespace Genetic
{
    public partial class Form1 : Form
    {
        GeneticBasic gb;
        GeneticModified gm;

        public Form1()
        {
            InitializeComponent();
        }

        private void buttonDraw_Click(object sender, EventArgs e)
        {
            double[] intervalStarts = { -5, -5};
            double[] intervalEnds = { 5, 5 };
            int n = 100;
            double Pm = 0.01;
            double Pc = 0.8;
            gb = new GeneticBasic(intervalStarts, intervalEnds, n, Pm, Pc);
            List<double> maxFuncEachIteration = gb.maxFuncEachIteration;
            CreateChart(maxFuncEachIteration,"chart1");
            int maxIter=maxFuncEachIteration.IndexOf(maxFuncEachIteration.Max())+1;
            textMaxBasic.Text = gb.absMaxFunction.ToString();
            textIterBasic.Text = maxIter.ToString();

            Pc = 0.25;
            int MinLT = 1;
            int MaxLT = 2;
            int p = 4;
            gm = new GeneticModified(intervalStarts, intervalEnds, n, Pm, Pc, MinLT, MaxLT, p);
            maxFuncEachIteration = gm.maxFuncEachIteration;
            CreateChart(maxFuncEachIteration, "chart2");
            
            maxIter = maxFuncEachIteration.IndexOf(maxFuncEachIteration.Max())+1;
            textMaxModified1.Text = gm.absMaxFunction.ToString();
            textIterModified1.Text = maxIter.ToString();
            //Pc = 0.5;
            //MinLT = 1;
            //MaxLT = 2;
            //p = 2;
            Pc = 0.2;
            MinLT = 1;
            MaxLT = 3;
            p = 2;
            //Pc = 0.8;
            //MinLT = 1;
            //MaxLT = 5;
            //p = 1;
            gm = new GeneticModified(intervalStarts, intervalEnds, n, Pm, Pc, MinLT, MaxLT, p);
            maxFuncEachIteration = gm.maxFuncEachIteration;
            CreateChart(maxFuncEachIteration, "chart3");
            maxIter = maxFuncEachIteration.IndexOf(maxFuncEachIteration.Max())+1;
            textMaxModified2.Text = gm.absMaxFunction.ToString();
            textIterModified2.Text = maxIter.ToString();
        }

        private void CreateChart(List<double> maxFuncEachIteration,string objectName)
        {
            Chart control =(Chart)Controls[objectName];
            control.Series.Clear();
            control.ChartAreas[0].AxisX.IsMarginVisible = false;
            //control.ChartAreas[0].AxisY.Maximum = maxFuncEachIteration.Max()+0.01;
            //control.ChartAreas[0].AxisY.Minimum = maxFuncEachIteration.Min()-0.01;
            string name = "MaxFunction";
            control.Series.Add(name);
            control.Series[name].IsVisibleInLegend = false;
            control.Series[name].ChartType = SeriesChartType.Spline;
            control.Series[name].BorderWidth = 2;

            for (int i = 0; i < maxFuncEachIteration.Count; i++)
            {
                control.Series[name].Points.AddXY(i+1, maxFuncEachIteration[i]);
            }
        }
    }
}
