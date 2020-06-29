using ScottPlot;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RaceTracker.Analysis
{
    public class Plotting
    {
        public Plotting()
        {
            this.TimeSeriesPlotSeriesNames = new List<string>();
        }

        private List<string> TimeSeriesPlotSeriesNames { get; }

        public void PlotTimeSeries(WpfPlot plot, IEnumerable<DateTime> x, IEnumerable<double> y, bool reset, string xLabel, string yLabel, string seriesName)
        {
            if (x.ToArray().Length < 1 || y.ToArray().Length < 1)
            {
                return;
            }

            if (reset)
            {
                plot.Reset();
            }

            this.TimeSeriesPlotSeriesNames.Add(seriesName);

            var xNumeric = new List<double>();
            foreach (var item in x)
            {
                xNumeric.Add(item.ToOADate());
            }

            plot.plt.PlotScatter(xNumeric.ToArray(), y.ToArray(), lineWidth: 0, label: seriesName);
            plot.plt.Ticks(dateTimeX: true);
            plot.plt.XLabel(xLabel);
            plot.plt.YLabel(yLabel);
            plot.plt.Legend(location: legendLocation.upperRight);
            plot.Render();
        }

        public void PlotScatter(WpfPlot plot, IEnumerable<double> x, IEnumerable<double> y, bool reset, string xLabel, string yLabel, List<string> seriesNames, string seriesName, double? xLower = null, double? xUpper = null, double? yLower = null, double? yUpper = null)
        {
            if (x.ToArray().Length < 1 || y.ToArray().Length < 1)
            {
                return;
            }

            if (reset)
            {
                plot.Reset();
            }

            seriesNames.Add(seriesName);
            plot.plt.PlotScatter(x.ToArray(), y.ToArray(), lineWidth: 0, label: seriesName);
            plot.plt.XLabel(xLabel);
            plot.plt.YLabel(yLabel);
            plot.plt.Axis(xLower, xUpper, yLower, yUpper);
            //plot.plt.AxisAutoY();
            
            plot.plt.Legend(location: legendLocation.upperRight);
            plot.Render();
        }

        public void PlotScatterLabels(WpfPlot plot, IEnumerable<double> x, IEnumerable<double> y, string xLabel, string yLabel)
        {
            plot.Reset();
            var xVals = x.ToArray();
            var yVals = y.ToArray();
            var densities = new List<int>();
            if (xVals.Length < 1 || yVals.Length < 1)
            {
                return;
            }

            var coordinates = new List<Tuple<double, double>>();
            for (int i = 0; i < xVals.Length; i++)
            {
                coordinates.Add(new Tuple<double, double>(xVals[i], yVals[i]));
            }

            foreach (var coordinate in coordinates)
            {
                int numberOccurances = 0;
                foreach (var coord in coordinates)
                {
                    if (coord.Item1 == coordinate.Item1 && coord.Item2 == coordinate.Item2)
                    {
                        numberOccurances++;
                    }
                }

                densities.Add(numberOccurances);
            }


            plot.plt.PlotScatter(x.ToArray(), y.ToArray(), lineWidth: 0);
            plot.plt.XLabel(xLabel);
            plot.plt.YLabel(yLabel);

            var plottableTextPoints = new Dictionary<string, int>();
            for (int i = 0; i < coordinates.Count; i++)
            {
                var key = coordinates[i].Item1 + "," + coordinates[i].Item2;

                if (plottableTextPoints.ContainsKey(key))
                {
                    if (densities[i] > plottableTextPoints[key])
                    {
                        plottableTextPoints[key] = densities[i];
                    }
                }
                else
                {
                    plottableTextPoints.Add(key, densities[i]);
                }
            }

            var colorThresholds = new double[5];            
            int maxValue = int.MinValue;
            foreach (var value in plottableTextPoints.Values)
            {
                if (value >= maxValue)
                {
                    maxValue = value;
                }
            }

            for (int i = 0; i < 5; i++)
            {
                if (i > 0)
                {
                    colorThresholds[i] = ((double)maxValue / 6) + colorThresholds[i - 1];
                }
                else
                {
                    colorThresholds[i] = ((double)maxValue / 6);
                }
            }

            foreach (var point in plottableTextPoints)
            {
                var coords = point.Key.Split(',');
                var color = Color.Black;

                if (point.Value >= colorThresholds[4])
                {
                    color = Color.Red;
                }
                else if (point.Value >= colorThresholds[3])
                {
                    color = Color.Orange;
                }
                else if (point.Value >= colorThresholds[2])
                {
                    color = Color.YellowGreen;
                }
                else if (point.Value >= colorThresholds[1])
                {
                    color = Color.Green;
                }
                else if (point.Value >= colorThresholds[0])
                {
                    color = Color.Blue;
                }

                plot.plt.PlotText(point.Value.ToString(), double.Parse(coords[0]), double.Parse(coords[1]), color: color, fontSize: 16, bold: true);
            }

            plot.Render();
        }
    }
}
