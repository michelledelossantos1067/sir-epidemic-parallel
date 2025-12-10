using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EpidemicSimulation.Parallel
{
    public class DayStatistics
    {
        public int Day { get; set; }
        public int Susceptible { get; set; }
        public int Infected { get; set; }
        public int Recovered { get; set; }
        public int Dead { get; set; }
        public double R0 { get; set; }
    }

    public class Statistics
    {
        private List<DayStatistics> history;

        public Statistics()
        {
            history = new List<DayStatistics>();
        }

        public void RecordDay(int day, int susceptible, int infected,
                             int recovered, int dead, double r0)
        {
            history.Add(new DayStatistics
            {
                Day = day,
                Susceptible = susceptible,
                Infected = infected,
                Recovered = recovered,
                Dead = dead,
                R0 = r0
            });
        }

        public void SaveToCSV(string filename)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Day,Susceptible,Infected,Recovered,Dead,R0");

            foreach (var stat in history)
            {
                sb.AppendLine($"{stat.Day},{stat.Susceptible},{stat.Infected}," +
                             $"{stat.Recovered},{stat.Dead},{stat.R0:F4}");
            }

            File.WriteAllText(filename, sb.ToString());
        }

        public void SaveExecutionTime(string filename, double seconds, int cores)
        {
            File.WriteAllText(filename, $"{seconds.ToString("F4", System.Globalization.CultureInfo.InvariantCulture)}\n{cores}");
        }

        public List<DayStatistics> GetHistory()
        {
            return new List<DayStatistics>(history);
        }
    }
}