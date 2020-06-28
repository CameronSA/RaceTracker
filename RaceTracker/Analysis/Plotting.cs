using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void PlotScatter(WpfPlot plot, IEnumerable<double> x, IEnumerable<double> y, bool reset, string xLabel, string yLabel, List<string> seriesNames, string seriesName)
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
            plot.plt.Legend(location: legendLocation.upperRight);
            plot.Render();
        }
    }
}
