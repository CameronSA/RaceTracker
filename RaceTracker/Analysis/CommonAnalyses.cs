using RaceTracker.LogicHelpers;
using RaceTracker.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RaceTracker.Analysis
{
    public static class CommonAnalyses
    {
        public static Dictionary<List<DateTime>, List<double>> NumberRaceCoursesData { get; private set; } = new Dictionary<List<DateTime>, List<double>>();

        public static Dictionary<int, int> NumberOfDaysWithGivenNumberOfRaceCourses { get; private set; } = new Dictionary<int, int>(); //Key: number race courses. Value: days

        private static DateTime NumberRaceCoursesMin { get; set; }
        private static DateTime NumberRaceCoursesMax { get; set; }
        private static DateTime NumberDaysMin { get; set; }
        private static DateTime NumberDaysMax { get; set; }

        private static string Resolution { get; set; }

        public static void LoadInitialData()
        {
            NumberRaceCoursesData = new Dictionary<List<DateTime>, List<double>>();
            NumberOfDaysWithGivenNumberOfRaceCourses = new Dictionary<int, int>();
            Resolution = TimeResolutionFields.Day;
            if (AppSettings.UseFullDataSet)
            {
                NumberRaceCoursesMin = Data.GetMinDate();
                NumberRaceCoursesMax = Data.GetMaxDate();
                NumberDaysMin = Data.GetMinDate();
                NumberDaysMax = Data.GetMaxDate();
            }
            else
            {
                NumberRaceCoursesMin = AppSettings.MinDate;
                NumberRaceCoursesMax = AppSettings.MaxDate;
                NumberDaysMin = AppSettings.MinDate;
                NumberDaysMax = AppSettings.MaxDate;
            }

            CommonAnalyses.NumberRaceCoursesData = CommonAnalyses.GetNumberRaceCoursesData(Resolution, NumberRaceCoursesMin, NumberRaceCoursesMax);

            for (int i = 1; i <= AppSettings.MaxNumberRaceCourses; i++)
            {
                var result = CommonAnalyses.CalculateNumberOfDaysWithGivenNumberOfRaceCourses(i, NumberDaysMin, NumberDaysMax);
                if (CommonAnalyses.NumberOfDaysWithGivenNumberOfRaceCourses.ContainsKey(i))
                {
                    CommonAnalyses.NumberOfDaysWithGivenNumberOfRaceCourses[i] = result;
                }
                else
                {
                    CommonAnalyses.NumberOfDaysWithGivenNumberOfRaceCourses.Add(i, result);
                }
            }
        }

        public static Dictionary<List<DateTime>, List<double>> RetrieveNumberRaceCoursesData(string resolution, DateTime minDate, DateTime maxDate)
        {
            if (resolution == Resolution)
            {
                if (minDate.Year == NumberRaceCoursesMin.Year && minDate.Month == NumberRaceCoursesMin.Month && minDate.Day == NumberRaceCoursesMin.Day && maxDate.Year == NumberRaceCoursesMax.Year && maxDate.Month == NumberRaceCoursesMax.Month && maxDate.Day == NumberRaceCoursesMax.Day)
                {
                    return NumberRaceCoursesData;
                }
                else
                {
                    var dates = new List<DateTime>();
                    var numberCourses = new List<double>();
                    NumberRaceCoursesMin = minDate;
                    NumberRaceCoursesMax = maxDate;
                    foreach (var dataSet in NumberRaceCoursesData)
                    {
                        var indices = new List<int>();
                        for (int i = 0; i < dataSet.Key.Count; i++)
                        {
                            if (dataSet.Key[i] >= minDate && dataSet.Key[i] <= maxDate)
                            {
                                indices.Add(i);
                            }
                        }

                        foreach (var index in indices)
                        {
                            dates.Add(dataSet.Key[index]);
                            numberCourses.Add(dataSet.Value[index]);
                        }
                    }

                    NumberRaceCoursesData = new Dictionary<List<DateTime>, List<double>> { { dates, numberCourses } };
                    return NumberRaceCoursesData;
                }
            }
            else
            {
                Resolution = resolution;
                NumberRaceCoursesData = CommonAnalyses.GetNumberRaceCoursesData(resolution, minDate, maxDate);
                return NumberRaceCoursesData;
            }
        }

        public static int RetrieveNumberOfDaysWithGivenNumberOfRaceCourses(int numberRaceCourses, DateTime minDate, DateTime maxDate)
        {
            if (minDate.Year == NumberDaysMin.Year && minDate.Month == NumberDaysMin.Month && minDate.Day == NumberDaysMin.Day && maxDate.Year == NumberDaysMax.Year && maxDate.Month == NumberDaysMax.Month && maxDate.Day == NumberDaysMax.Day)
            {
                if(NumberOfDaysWithGivenNumberOfRaceCourses.ContainsKey(numberRaceCourses))
                {
                    return NumberOfDaysWithGivenNumberOfRaceCourses[numberRaceCourses];
                }
                else
                {
                    int result = CalculateNumberOfDaysWithGivenNumberOfRaceCourses(numberRaceCourses, minDate, maxDate);
                    NumberOfDaysWithGivenNumberOfRaceCourses.Add(numberRaceCourses, result);
                    return result;
                }
            }
            else
            {
                NumberDaysMin = minDate;
                NumberDaysMax = maxDate;
                for (int i = 1; i <= AppSettings.MaxNumberRaceCourses; i++)
                {
                    var result = CommonAnalyses.CalculateNumberOfDaysWithGivenNumberOfRaceCourses(i, NumberDaysMin, NumberDaysMax);
                    if (CommonAnalyses.NumberOfDaysWithGivenNumberOfRaceCourses.ContainsKey(i))
                    {
                        CommonAnalyses.NumberOfDaysWithGivenNumberOfRaceCourses[i] = result;
                    }
                    else
                    {
                        CommonAnalyses.NumberOfDaysWithGivenNumberOfRaceCourses.Add(i, result);
                    }
                }

                return RetrieveNumberOfDaysWithGivenNumberOfRaceCourses(numberRaceCourses, minDate, maxDate);
            }
        }

        private static Dictionary<List<DateTime>, List<double>> GetNumberRaceCoursesData(string resolution, DateTime minDate, DateTime maxDate)
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

        private static int CalculateNumberOfDaysWithGivenNumberOfRaceCourses(int numberRaceCourses, DateTime minDate, DateTime maxDate)
        {
            var data = CommonAnalyses.NumberRaceCoursesData;
            var days = new HashSet<DateTime>();

            foreach (var dataSet in data)
            {
                for (int i = 0; i < dataSet.Key.Count; i++)
                {
                    if (dataSet.Value[i] == numberRaceCourses)
                    {
                        days.Add(dataSet.Key[i]);
                    }
                }
            }

            return days.Count;
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
