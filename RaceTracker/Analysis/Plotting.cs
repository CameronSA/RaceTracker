using RaceTracker.LogicHelpers;
using RaceTracker.Models;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RaceTracker.Analysis
{
    public class Plotting
    {
        public Plotting()
        {
            this.MarkerSize = 7;
        }

        public float MarkerSize { get; set; }

        public void PlotTimeSeries(WpfPlot plot, IEnumerable<DateTime> x, IEnumerable<double> y, bool reset, string xLabel, string yLabel, List<string> seriesList, string seriesName)
        {
            if (x.ToArray().Length < 1 || y.ToArray().Length < 1)
            {
                return;
            }

            if (x.ToArray().Length != y.ToArray().Length)
            {
                MessageBox.Show("ERROR: x and y data have different lengths (" + x.ToArray().Length + " and " + y.ToArray().Length + ")", AppSettings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (reset)
            {
                plot.Reset();
            }

            var xNumeric = new List<double>();
            foreach (var item in x)
            {
                xNumeric.Add(item.ToOADate());
            }

            if (!string.IsNullOrEmpty(seriesName))
            {
                seriesList.Add(seriesName);
            }

            plot.plt.PlotScatter(xNumeric.ToArray(), y.ToArray(), lineWidth: 0, label: seriesName, markerSize: this.MarkerSize);
            plot.plt.Ticks(dateTimeX: true);
            plot.plt.XLabel(xLabel);
            plot.plt.YLabel(yLabel);
            plot.plt.AxisAuto();

            if (seriesList.Count > 0)
            {
                plot.plt.Legend(location: legendLocation.upperRight);
            }

            plot.Render();
        }

        public void PlotTimeSeriesError(WpfPlot plot, IEnumerable<DateTime> x, IEnumerable<double> y, IEnumerable<double> yError, bool reset, string xLabel, string yLabel, List<string> seriesList, string seriesName)
        {
            if (x.ToArray().Length < 1 || y.ToArray().Length < 1)
            {
                return;
            }

            if (x.ToArray().Length != y.ToArray().Length)
            {
                MessageBox.Show("ERROR: x and y data have different lengths (" + x.ToArray().Length + " and " + y.ToArray().Length + ")", AppSettings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if(y.ToArray().Length != yError.ToArray().Length)
            {
                MessageBox.Show("ERROR: y and yError data have different lengths (" + y.ToArray().Length + " and " + yError.ToArray().Length + ")", AppSettings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (reset)
            {
                plot.Reset();
            }

            var xNumeric = new List<double>();
            foreach (var item in x)
            {
                xNumeric.Add(item.ToOADate());
            }

            if (!string.IsNullOrEmpty(seriesName))
            {
                seriesList.Add(seriesName);
            }

            plot.plt.PlotScatter(xNumeric.ToArray(), y.ToArray(), lineWidth: 0, label: seriesName, markerSize: this.MarkerSize, errorY: yError.ToArray());
            plot.plt.Ticks(dateTimeX: true);
            plot.plt.XLabel(xLabel);
            plot.plt.YLabel(yLabel);
            plot.plt.AxisAuto();

            if (seriesList.Count > 0)
            {
                plot.plt.Legend(location: legendLocation.upperRight);
            }

            plot.Render();
        }

        public void PlotScatter(WpfPlot plot, IEnumerable<double> x, IEnumerable<double> y, bool reset, string xLabel, string yLabel, List<string> seriesNames, string seriesName, double? xLower = null, double? xUpper = null, double? yLower = null, double? yUpper = null)
        {
            if (x.ToArray().Length < 1 || y.ToArray().Length < 1)
            {
                return;
            }

            if(x.ToArray().Length != y.ToArray().Length)
            {
                MessageBox.Show("ERROR: x and y data have different lengths (" + x.ToArray().Length + " and " + y.ToArray().Length + ")", AppSettings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (reset)
            {
                plot.Reset();
            }

            if (!string.IsNullOrEmpty(seriesName))
            {
                seriesNames.Add(seriesName);
            }

            plot.plt.PlotScatter(x.ToArray(), y.ToArray(), lineWidth: 0, label: seriesName, markerSize: this.MarkerSize);
            plot.plt.XLabel(xLabel);
            plot.plt.YLabel(yLabel);
            plot.plt.Axis(xLower, xUpper, yLower, yUpper);

            if (seriesNames.Count > 0)
            {
                plot.plt.Legend(location: legendLocation.upperRight);
            }

            plot.Render();
        }

        public void PlotScatterLabels(WpfPlot plot, IEnumerable<double> x, IEnumerable<double> y, string xLabel, string yLabel, string normalisingFactor, DateTime minDate, DateTime maxDate)
        {
            plot.Reset();
            var xVals = x.ToArray();
            var yVals = y.ToArray();
            var densities = new List<double>();
            if (xVals.Length < 1 || yVals.Length < 1)
            {
                return;
            }

            if (xVals.Length != yVals.Length)
            {
                MessageBox.Show("ERROR: x and y data have different lengths (" + xVals.Length + " and " + yVals.Length + ")", AppSettings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var coordinates = new List<Tuple<double, double>>();
            for (int i = 0; i < xVals.Length; i++)
            {
                coordinates.Add(new Tuple<double, double>(xVals[i], yVals[i]));
            }

            foreach (var coordinate in coordinates)
            {
                double normalisedNumberOccurances = 0;
                double numberRaceTracks = 0;
                foreach (var coord in coordinates)
                {
                    if (coord.Item1 == coordinate.Item1 && coord.Item2 == coordinate.Item2)
                    {
                        normalisedNumberOccurances++;
                        numberRaceTracks = coordinate.Item2;
                    }
                }

                switch (normalisingFactor)
                {
                    case NormalisingFactors.NumberOfRaceTracks:
                        normalisedNumberOccurances /= (double)CommonAnalyses.RetrieveNumberOfDaysWithGivenNumberOfRaceCourses((int)numberRaceTracks, minDate, maxDate);
                        break;
                }

                densities.Add(normalisedNumberOccurances);
            }


            plot.plt.PlotScatter(x.ToArray(), y.ToArray(), lineWidth: 0, markerSize: this.MarkerSize);
            plot.plt.XLabel(xLabel);
            plot.plt.YLabel(yLabel);

            var plottableTextPoints = new Dictionary<string, double>();
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
            float maxValue = float.MinValue;
            foreach (var value in plottableTextPoints.Values)
            {
                if (value >= maxValue)
                {
                    maxValue = (float)value;
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

                string displayValue = point.Value.ToString();
                if (displayValue.Length > 3)
                {
                    displayValue = displayValue.Substring(0, 3);
                }

                plot.plt.PlotText(displayValue, double.Parse(coords[0]), double.Parse(coords[1]), color: color, fontSize: 16, bold: true);
            }

            plot.plt.AxisAuto();
            plot.Render();
        }

        public void PlotBar(WpfPlot plot, IEnumerable<string> x, IEnumerable<double> y, bool reset, string xLabel, string yLabel, float rotation = 0, int yMin = 0, int yMax = 0)
        {
            if (x.ToArray().Length < 1 || y.ToArray().Length < 1)
            {
                return;
            }

            if (x.ToArray().Length != y.ToArray().Length)
            {
                MessageBox.Show("ERROR: x and y data have different lengths (" + x.ToArray().Length + " and " + y.ToArray().Length + ")", AppSettings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (reset)
            {
                plot.Reset();
            }

            var xs = DataGen.Consecutive(x.ToArray().Length);
            plot.plt.PlotBar(xs, y.ToArray());
            plot.plt.XTicks(xs, x.ToArray());
            plot.plt.Ticks(xTickRotation: rotation);
            plot.plt.YLabel(yLabel);
            plot.plt.XLabel(xLabel);
            if (yMin != 0 || yMax != 0)
            {
                plot.plt.Axis(y1: yMin, y2: yMax);
            }

            plot.Render();
        }

        public void PlotBarGroups(WpfPlot plot, IEnumerable<string> x, IEnumerable<double> y, List<double[]> seriesValues, bool reset, string xLabel, string yLabel, List<string> seriesNames, string seriesName, int yMin = 0, int yMax = 0)
        {
            if (x.ToArray().Length < 1 || y.ToArray().Length < 1)
            {
                return;
            }

            if (x.ToArray().Length != y.ToArray().Length)
            {
                MessageBox.Show("ERROR: x and y data have different lengths (" + x.ToArray().Length + " and " + y.ToArray().Length + ")", AppSettings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (reset)
            {
                plot.Reset();
            }

            if (!string.IsNullOrEmpty(seriesName))
            {
                seriesNames.Add(seriesName);
            }

            seriesValues.Add(y.ToArray());

            plot.plt.PlotBarGroups(groupLabels: x.ToArray(), seriesLabels: seriesNames.ToArray(), seriesValues.ToArray());
            plot.plt.YLabel(yLabel);
            plot.plt.XLabel(xLabel);
            plot.plt.Grid(enableVertical: false, enableHorizontal: true, lineStyle: LineStyle.Dot);
            if (yMin != 0 || yMax != 0)
            {
                plot.plt.Axis(y1: yMin, y2: yMax);
            }

            if (seriesNames.Count > 0)
            {
                plot.plt.Legend(location: legendLocation.upperRight);
            }

            plot.Render();
        }

        public void PlotGaussian(WpfPlot plot, IEnumerable<double> values, string xLabel, string yLabel, string seriesName, string seriesUnit, bool reset)
        {
            if(reset)
            {
                plot.Reset();
            }

            if (values.ToArray().Length < 1)
            {
                return;
            }

            var uniqueValues = new HashSet<double>();
            foreach(var value in values)
            {
                uniqueValues.Add(value);
            }

            double minValue = uniqueValues.Min();
            double maxValue = uniqueValues.Max();

            var hist = new ScottPlot.Statistics.Histogram(values.ToArray(), minValue - (0.1 * minValue), maxValue + (0.1 * maxValue));
            double barWidth = hist.binSize * 1.2;
            seriesName += "\n" + Math.Round(hist.mean, 2) + " +/- " + Math.Round(hist.stdev, 2) + " " + seriesUnit;

            plot.plt.PlotBar(hist.bins, hist.countsFrac, barWidth: barWidth, outlineWidth: 0);
            plot.plt.PlotScatter(hist.bins, hist.countsFracCurve, markerSize: 0, lineWidth: 2, color: Color.Black, label: seriesName);
            plot.plt.YLabel(yLabel);
            plot.plt.XLabel(xLabel);
            plot.plt.Legend(location: legendLocation.upperRight);
            plot.Render();
        }
    }
}
