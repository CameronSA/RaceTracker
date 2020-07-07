using RaceTracker.LogicHelpers;
using RaceTracker.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RaceTracker.Analysis
{
    public static class CommonAnalyses
    {
        public static Dictionary<List<DateTime>, List<double>> NumberRaceCoursesData { get; private set; } = new Dictionary<List<DateTime>, List<double>>();

        public static Dictionary<int, int> NumberOfDaysWithGivenNumberOfRaceCourses { get; private set; } = new Dictionary<int, int>(); //Key: number race courses. Value: days
        public static Dictionary<string,Tuple<List<DateTime>,List<double>>> NumberRaceTypePerDay { get; private set; } //Key: race type value: number of occurances per day

        public static DateTime MinDataDate { get; private set; }
        public static DateTime MaxDataDate { get; private set; }
        private static DateTime NumberRaceCoursesMin { get; set; }
        private static DateTime NumberRaceCoursesMax { get; set; }
        private static DateTime NumberDaysMin { get; set; }
        private static DateTime NumberDaysMax { get; set; }

        private static string Resolution { get; set; }

        public static void LoadInitialData()
        {
            MinDataDate = Data.GetMinDate();
            MaxDataDate = Data.GetMaxDate();
            NumberRaceCoursesData = new Dictionary<List<DateTime>, List<double>>();
            NumberOfDaysWithGivenNumberOfRaceCourses = new Dictionary<int, int>();
            Resolution = TimeResolutionFields.Day;
            if (AppSettings.UseFullDataSet)
            {
                NumberRaceCoursesMin = MinDataDate;
                NumberRaceCoursesMax = MaxDataDate;
                NumberDaysMin = MinDataDate;
                NumberDaysMax = MaxDataDate;
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

            CommonAnalyses.NumberRaceTypePerDay = CommonAnalyses.GetRaceTypesVsTime(MinDataDate, MaxDataDate);
        }

        public static Tuple<List<DateTime>, List<double>, List<double>, int> BinData(List<Tuple<DateTime, double>> timeSeriesData, int numberBins)
        {
            int datesPerBin = (int)Math.Ceiling((double)timeSeriesData.Count / (double)numberBins);
            var binnedDates = new List<DateTime>();
            var averages = new List<double>();
            var averagesError = new List<double>();
            for (int i = 0; i < timeSeriesData.Count; i += datesPerBin)
            {
                var values = new List<double>();
                for (int n = i; n < i + datesPerBin; n++)
                {
                    if (n < timeSeriesData.Count)
                    {
                        values.Add(timeSeriesData[n].Item2);
                    }
                }

                if (values.Count > 0)
                {
                    var average = values.Average();
                    var sumSquares = values.Sum(x => (x - average) * (x - average));
                    var stdDev = Math.Sqrt(sumSquares);

                    binnedDates.Add(timeSeriesData[i].Item1);
                    averages.Add(average);
                    averagesError.Add(stdDev);
                }
            }

            return new Tuple<List<DateTime>, List<double>, List<double>, int>(binnedDates, averages, averagesError, datesPerBin);
        }

        public static Dictionary<string, Tuple<List<DateTime>, List<double>>> GetRaceTypesVsTime(DateTime minDate, DateTime maxDate)
        {
            var relevantColumns = new List<Tuple<DateTime, DateTime, string>>();
            var dates = new List<DateTime>();
            var times = new List<DateTime>();
            var raceTypes = new List<string>();

            foreach (var column in Data.ProcessedRaceData)
            {
                if (column.Key.ToLower().Trim() == "date")
                {
                    foreach (var item in column.Value.Data)
                    {
                        dates.Add((DateTime)item);
                    }
                }
                else if (column.Key.ToLower().Trim() == "time")
                {
                    foreach (var item in column.Value.Data)
                    {
                        times.Add((DateTime)item);
                    }
                }
                else if (column.Key.ToLower().Trim() == "race type")
                {
                    foreach (var item in column.Value.Data)
                    {
                        raceTypes.Add((string)item);
                    }
                }
            }

            for (int i = 0; i < dates.Count; i++)
            {
                relevantColumns.Add(new Tuple<DateTime, DateTime, string>(dates[i], times[i], raceTypes[i]));
            }

            var raceTypeNumberPerDay = new Dictionary<string, Dictionary<string, double>>(); // key: type value: dictionary of time and number of type
            foreach(var row in relevantColumns)
            {
                string dateString = row.Item1 + "," + row.Item2;
                if (raceTypeNumberPerDay.ContainsKey(row.Item3))
                {
                    if(raceTypeNumberPerDay[row.Item3].ContainsKey(dateString))
                    {
                        raceTypeNumberPerDay[row.Item3][dateString] = raceTypeNumberPerDay[row.Item3][dateString] + 1;
                    }
                    else
                    {
                        raceTypeNumberPerDay[row.Item3].Add(dateString, 1);
                    }
                }
                else
                {
                    raceTypeNumberPerDay.Add(row.Item3, new Dictionary<string, double>() { { dateString, 1 } });
                };
            }

            var results = new Dictionary<string, Tuple<List<DateTime>, List<double>>>();
            foreach(var raceType in raceTypeNumberPerDay)
            {
                var raceTypeNumbers = new Dictionary<DateTime, double>();
                foreach(var dateString in raceType.Value)
                {
                    var date = DateTime.Parse(dateString.Key.Split(new char[] { ',' })[0]);
                    if (raceTypeNumbers.ContainsKey(date))
                    {
                        raceTypeNumbers[date] = raceTypeNumbers[date] + dateString.Value;
                    }
                    else
                    {
                        raceTypeNumbers.Add(date, dateString.Value);
                    }
                }

                var dateList = new List<DateTime>();
                var numberList = new List<double>();
                foreach(var item in raceTypeNumbers)
                {
                    dateList.Add(item.Key);
                    numberList.Add(item.Value);
                }

                results.Add(raceType.Key, new Tuple<List<DateTime>, List<double>>(dateList, numberList));
            }

            return results;
        }

        public static Dictionary<List<string>, List<double>> GetNumberRaceTypes(DateTime minDate, DateTime maxDate)
        {
            var relevantColumns = new List<Tuple<DateTime, DateTime, string>>();
            var dates = new List<DateTime>();
            var times = new List<DateTime>();
            var raceTypes = new List<string>();

            foreach (var column in Data.ProcessedRaceData)
            {
                if (column.Key.ToLower().Trim() == "date")
                {
                    foreach (var item in column.Value.Data)
                    {
                        dates.Add((DateTime)item);
                    }
                }
                else if (column.Key.ToLower().Trim() == "time")
                {
                    foreach (var item in column.Value.Data)
                    {
                        times.Add((DateTime)item);
                    }
                }
                else if (column.Key.ToLower().Trim() == "race type")
                {
                    foreach (var item in column.Value.Data)
                    {
                        raceTypes.Add((string)item);
                    }
                }
            }

            for (int i = 0; i < dates.Count; i++)
            {
                relevantColumns.Add(new Tuple<DateTime, DateTime, string>(dates[i], times[i], raceTypes[i]));
            }

            return GetNumberRaceTypes(relevantColumns, minDate, maxDate);
        }

        private static Dictionary<List<string>, List<double>> GetNumberRaceTypes(List<Tuple<DateTime, DateTime, string>> rows, DateTime minDate, DateTime maxDate)
        {
            var raceTypes = new List<string>();
            var counts = new List<double>();
            var raceTypesByRace = new Dictionary<string, HashSet<string>>();
            foreach (var row in rows)
            {
                if (row.Item1 >= minDate && row.Item1 <= maxDate)
                {
                    var key = row.Item1.ToString() + " " + row.Item2.ToString();
                    if (raceTypesByRace.ContainsKey(key))
                    {
                        raceTypesByRace[key].Add(row.Item3);
                    }
                    else
                    {
                        raceTypesByRace.Add(key, new HashSet<string> { row.Item3 });
                    }
                }
            }

            var raceTypesCount = new Dictionary<string, double>();
            foreach (var race in raceTypesByRace)
            {
                foreach(var raceType in race.Value)
                {
                    if(raceTypesCount.ContainsKey(raceType))
                    {
                        raceTypesCount[raceType] += 1;
                    }
                    else
                    {
                        raceTypesCount.Add(raceType, 1);
                    }
                }
            }

            foreach(var race in raceTypesCount)
            {
                raceTypes.Add(race.Key);
                counts.Add(race.Value);
            }

            return new Dictionary<List<string>, List<double>> { { raceTypes, counts } };
        }

        public static List<Tuple<string, Dictionary<List<double>, List<double>>>> GetNumberRaceCoursesByDayCountBySeason()
        {
            var numberRaceCoursesByDay = CommonAnalyses.RetrieveNumberRaceCoursesData(TimeResolutionFields.Day, MinDataDate, MaxDataDate);
            var springIndices = new List<int>();
            var summerIndices = new List<int>();
            var autumnIndices = new List<int>();
            var winterIndices = new List<int>();

            var numberVsCountWinter = new Dictionary<double, double>();
            var numberVsCountSpring = new Dictionary<double, double>();
            var numberVsCountSummer = new Dictionary<double, double>();
            var numberVsCountAutumn = new Dictionary<double, double>();

            var winterNumbers = new List<double>();
            var winterCounts = new List<double>();
            var springNumbers = new List<double>();
            var springCounts = new List<double>();
            var summerNumbers = new List<double>();
            var summerCounts = new List<double>();
            var autumnNumbers = new List<double>();
            var autumnCounts = new List<double>();

            foreach (var dataSet in numberRaceCoursesByDay)
            {
                for (int i = 0; i < dataSet.Key.Count; i++)
                {
                    if (dataSet.Key[i].Month == 12 || dataSet.Key[i].Month == 1 || dataSet.Key[i].Month == 2)
                    {
                        winterIndices.Add(i);
                    }
                    else if (dataSet.Key[i].Month == 3 || dataSet.Key[i].Month == 4 || dataSet.Key[i].Month == 5)
                    {
                        springIndices.Add(i);
                    }
                    else if (dataSet.Key[i].Month == 6 || dataSet.Key[i].Month == 7 || dataSet.Key[i].Month == 8)
                    {
                        summerIndices.Add(i);
                    }
                    else
                    {
                        autumnIndices.Add(i);
                    }
                }

                foreach (var index in winterIndices)
                {
                    if (numberVsCountWinter.ContainsKey(dataSet.Value[index]))
                    {
                        numberVsCountWinter[dataSet.Value[index]] = numberVsCountWinter[dataSet.Value[index]] + 1;
                    }
                    else
                    {
                        numberVsCountWinter.Add(dataSet.Value[index], 1);
                    }
                }

                foreach (var index in springIndices)
                {
                    if (numberVsCountSpring.ContainsKey(dataSet.Value[index]))
                    {
                        numberVsCountSpring[dataSet.Value[index]] = numberVsCountSpring[dataSet.Value[index]] + 1;
                    }
                    else
                    {
                        numberVsCountSpring.Add(dataSet.Value[index], 1);
                    }
                }

                foreach (var index in summerIndices)
                {
                    if (numberVsCountSummer.ContainsKey(dataSet.Value[index]))
                    {
                        numberVsCountSummer[dataSet.Value[index]] = numberVsCountSummer[dataSet.Value[index]] + 1;
                    }
                    else
                    {
                        numberVsCountSummer.Add(dataSet.Value[index], 1);
                    }
                }

                foreach (var index in autumnIndices)
                {
                    if (numberVsCountAutumn.ContainsKey(dataSet.Value[index]))
                    {
                        numberVsCountAutumn[dataSet.Value[index]] = numberVsCountAutumn[dataSet.Value[index]] + 1;
                    }
                    else
                    {
                        numberVsCountAutumn.Add(dataSet.Value[index], 1);
                    }
                }

                foreach (var number in numberVsCountWinter)
                {
                    winterNumbers.Add(number.Key);
                    winterCounts.Add(number.Value);
                }

                foreach (var number in numberVsCountSpring)
                {
                    springNumbers.Add(number.Key);
                    springCounts.Add(number.Value);
                }

                foreach (var number in numberVsCountSummer)
                {
                    summerNumbers.Add(number.Key);
                    summerCounts.Add(number.Value);
                }

                foreach (var number in numberVsCountAutumn)
                {
                    autumnNumbers.Add(number.Key);
                    autumnCounts.Add(number.Value);
                }

                break;
            }

            return new List<Tuple<string, Dictionary<List<double>, List<double>>>>
            {
                new Tuple<string, Dictionary<List<double>, List<double>>>("Winter",new Dictionary<List<double>, List<double>>{{winterNumbers,winterCounts}}),
                new Tuple<string, Dictionary<List<double>, List<double>>>("Spring",new Dictionary<List<double>, List<double>>{{springNumbers,springCounts}}),
                new Tuple<string, Dictionary<List<double>, List<double>>>("Summer",new Dictionary<List<double>, List<double>>{{summerNumbers,summerCounts}}),
                new Tuple<string, Dictionary<List<double>, List<double>>>("Autumn",new Dictionary<List<double>, List<double>>{{autumnNumbers,autumnCounts}})
            };
        }
        
        public static Dictionary<List<double>,List<double>> GetNumberRaceCoursesByDayCount()
        {
            var numbers = new List<double>();
            var counts = new List<double>();
            var numberRaceCoursesByDay = CommonAnalyses.RetrieveNumberRaceCoursesData(TimeResolutionFields.Day, MinDataDate, MaxDataDate);
            var numbersVsCount = new Dictionary<double, double>();
            foreach(var dataSet in numberRaceCoursesByDay)
            {
                foreach (var numberRaceCourses in dataSet.Value)
                {
                    if (numbersVsCount.ContainsKey(numberRaceCourses))
                    {
                        numbersVsCount[numberRaceCourses] = numbersVsCount[numberRaceCourses] + 1;
                    }
                    else
                    {
                        numbersVsCount.Add(numberRaceCourses, 1);
                    }
                }

                foreach(var number in numbersVsCount)
                {
                    numbers.Add(number.Key);
                    counts.Add(number.Value);
                }

                break;
            }

            return new Dictionary<List<double>, List<double>> { { numbers, counts } };
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
