using System;
using System.Diagnostics;
using System.IO;

namespace EpidemicSimulation.Sequential
{
    class Program
    {
        static void Main(string[] args)
        {
            const int GridSize = 1000;
            const int Days = 365;
            const double InfectionProb = 0.3;
            const double RecoveryProb = 0.1;
            const double DeathProb = 0.01;
            const int InitialInfected = 10;

            Console.WriteLine("=============================================================");
            Console.WriteLine("       SIMULACION MONTE-CARLO SIR - VERSION SECUENCIAL       ");
            Console.WriteLine("=============================================================");
            Console.WriteLine();
            Console.WriteLine($"Parametros:");
            Console.WriteLine($"  Tamano de grilla: {GridSize}x{GridSize} ({GridSize * GridSize:N0} individuos)");
            Console.WriteLine($"  Dias a simular: {Days}");
            Console.WriteLine($"  Infectados iniciales: {InitialInfected}");
            Console.WriteLine($"  P(infeccion): {InfectionProb:F2}");
            Console.WriteLine($"  P(recuperacion): {RecoveryProb:F2}");
            Console.WriteLine($"  P(muerte): {DeathProb:F2}");
            Console.WriteLine();

            Directory.CreateDirectory("output");
            Directory.CreateDirectory("output/grid_snapshots");

            var model = new SIRModel(GridSize, InfectionProb, RecoveryProb, 
                                    DeathProb, InitialInfected);
            
            var statistics = new Statistics();
            var visualizer = new Visualizer(GridSize);

            Console.WriteLine("Iniciando simulacion...");
            Console.WriteLine();

            var stopwatch = Stopwatch.StartNew();

            for (int day = 0; day < Days; day++)
            {
                model.SimulateDay();

                var stats = model.GetStatistics();
                double r0 = model.CalculateR0(day + 1);

                statistics.RecordDay(day + 1, stats.susceptible, stats.infected, 
                                    stats.recovered, stats.dead, r0);

                if ((day + 1) % 50 == 0 || day == 0)
                {
                    Console.WriteLine($"Dia {day + 1,3}/{Days} - " +
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
                    Console.WriteLine();
                    Console.WriteLine($"Epidemia terminada en dia {day + 1}");
                    break;
                }
            }

            stopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("=============================================================");
            Console.WriteLine("                    RESUMEN DE RESULTADOS                    ");
            Console.WriteLine("=============================================================");
            Console.WriteLine($"Tiempo total de ejecucion: {stopwatch.Elapsed.TotalSeconds:F2} segundos");
            Console.WriteLine();

            var finalStats = model.GetStatistics();
            Console.WriteLine($"Susceptibles finales: {finalStats.susceptible,10:N0}");
            Console.WriteLine($"Infectados finales:   {finalStats.infected,10:N0}");
            Console.WriteLine($"Recuperados finales:  {finalStats.recovered,10:N0}");
            Console.WriteLine($"Fallecidos finales:   {finalStats.dead,10:N0}");
            
            int totalAffected = finalStats.infected + finalStats.recovered + finalStats.dead;
            double percentAffected = (double)totalAffected / (GridSize * GridSize) * 100;
            
            Console.WriteLine($"Total afectados:      {totalAffected,10:N0} ({percentAffected:F2}%)");
            Console.WriteLine();

            statistics.SaveToCSV("output/statistics.csv");
            statistics.SaveExecutionTime("output/execution_time.txt", stopwatch.Elapsed.TotalSeconds);

            Console.WriteLine("Archivos generados:");
            Console.WriteLine("  - output/statistics.csv");
            Console.WriteLine("  - output/execution_time.txt");
            Console.WriteLine("  - output/grid_snapshots/");
            Console.WriteLine();
            Console.WriteLine("Simulacion completada exitosamente.");
        }
    }
}