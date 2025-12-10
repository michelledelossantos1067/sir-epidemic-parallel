using System;
using System.Diagnostics;

namespace EpidemicSimulation.Sequential
{
    public class GridSimulator
    {
        private SIRModel model;
        private Statistics statistics;
        private Visualizer visualizer;
        private int days;
        private bool verbose;

        public GridSimulator(SIRModel model, int days, bool verbose = true)
        {
            this.model = model;
            this.days = days;
            this.verbose = verbose;
            this.statistics = new Statistics();
            this.visualizer = new Visualizer(model.GridSize);
        }

        public (double executionTime, Statistics stats) Run()
        {
            var stopwatch = Stopwatch.StartNew();

            for (int day = 0; day < days; day++)
            {
                model.SimulateDay();

                var stats = model.GetStatistics();
                double r0 = model.CalculateR0(day + 1);

                statistics.RecordDay(day + 1, stats.susceptible, stats.infected,
                                   stats.recovered, stats.dead, r0);

                if (verbose && ((day + 1) % 50 == 0 || day == 0))
                {
                    Console.WriteLine($"Dia {day + 1,3}/{days} - " +
                                    $"Infectados: {stats.infected,8:N0} - " +
                                    $"R0: {r0,5:F2}");

                    visualizer.PrintGridSummary(model.GetGrid(), day + 1);
                }

                if ((day + 1) % 30 == 0)
                {
                    visualizer.SaveGridSnapshot(model.GetGrid(), day + 1,
                                               "output/grid_snapshots");
                }

                if (stats.infected == 0)
                {
                    if (verbose)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"Epidemia terminada en dia {day + 1}");
                    }
                    break;
                }
            }

            stopwatch.Stop();

            if (verbose)
            {
                Console.WriteLine();
                Console.WriteLine($"Tiempo de ejecucion: {stopwatch.Elapsed.TotalSeconds:F2} segundos");
            }

            return (stopwatch.Elapsed.TotalSeconds, statistics);
        }

        public void SaveResults(string statsFile, string timeFile)
        {
            statistics.SaveToCSV(statsFile);
            statistics.SaveExecutionTime(timeFile, 
                statistics.GetHistory()[^1].Day);
        }

        public Statistics GetStatistics()
        {
            return statistics;
        }
    }
}