using RaceTracker.LogicHelpers;
using RaceTracker.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace RaceTracker.Analysis
{
    public static class CommonAnalyses
    {
        public static Dictionary<List<DateTime>, List<double>> GetNumberRaceCoursesData(string resolution, DateTime minDate, DateTime maxDate)
        {
            var relevantColumns = new List<Tuple<DateTime, string>>();
            var dates = new List<DateTime>();
            var raceTracks = new List<string>();

            foreach (var column in Data.ProcessedRaceData)
            {
                if (column.Key.ToLower() == "date")
                {
                    foreach (var item in column.Value.Data)
                    {
                        dates.Add((DateTime)item);
                    }
                }
                else if (column.Key.ToLower() == "racetrack")
                {
                    foreach (var item in column.Value.Data)
                    {
                        raceTracks.Add((string)item);
                    }
                }
            }

            for (int i = 0; i < dates.Count; i++)
            {
                relevantColumns.Add(new Tuple<DateTime, string>(dates[i], raceTracks[i]));
            }

            return GetNumberOfRaceTracksPerDate(relevantColumns, minDate, maxDate, resolution);
        }

        private static Dictionary<List<DateTime>, List<double>> GetNumberOfRaceTracksPerDate(List<Tuple<DateTime, string>> rows, DateTime minDate, DateTime maxDate, string resolution)
        {
            var dates = new List<DateTime>();
            var tracksPerDate = new List<double>();
            var tracksPerDelimiter = new Dictionary<string, HashSet<string>>();
            foreach (var row in rows)
            {
                // Check date is within range
                if (row.Item1 >= minDate && row.Item1 <= maxDate)
                {
                    string delimiter = string.Empty;
                    switch (resolution)
                    {
                        case TimeResolutionFields.Day: delimiter = Formatting.EditStringLength(row.Item1.Year.ToString(), 4) + Formatting.EditStringLength(row.Item1.Month.ToString(), 2) + Formatting.EditStringLength(row.Item1.Day.ToString(), 2); break;
                        case TimeResolutionFields.Month: delimiter = Formatting.EditStringLength(row.Item1.Year.ToString(), 4) + Formatting.EditStringLength(row.Item1.Month.ToString(), 2); break;
                        case TimeResolutionFields.Year: delimiter = Formatting.EditStringLength(row.Item1.Year.ToString(), 4); break;
                        default:
                            MessageBox.Show("ERROR: Invalid time resolution selected: " + resolution, AppSettings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            throw new Exception();
                    }

                    if (tracksPerDelimiter.ContainsKey(delimiter))
                    {
                        tracksPerDelimiter[delimiter].Add(row.Item2.Trim());
                    }
                    else
                    {
                        tracksPerDelimiter.Add(delimiter, new HashSet<string> { row.Item2.Trim() });
                    }
                }
            }

            // Count the number of unique racetracks per delimiter
            foreach (var delimiter in tracksPerDelimiter)
            {
                string dateFormat = string.Empty;
                switch (resolution)
                {
                    case TimeResolutionFields.Day: dateFormat = "yyyyMMdd"; break;
                    case TimeResolutionFields.Month: dateFormat = "yyyyMM"; break;
                    case TimeResolutionFields.Year: dateFormat = "yyyy"; break;
                    default:
                        MessageBox.Show("ERROR: Invalid time resolution selected: " + resolution, AppSettings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        throw new Exception();
                }

                dates.Add(DateTime.ParseExact(delimiter.Key, dateFormat, CultureInfo.InvariantCulture));
                tracksPerDate.Add(delimiter.Value.Count);
            }

            var result = new Dictionary<List<DateTime>, List<double>>
            {
                { dates, tracksPerDate }
            };

            return result;
        }
    }
}
