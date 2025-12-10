using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EpidemicSimulation.Parallel
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

            int numThreads = Environment.ProcessorCount;
            
            if (args.Length > 0 && int.TryParse(args[0], out int threads))
            {
                numThreads = threads;
            }

            Console.WriteLine("=============================================================");
            Console.WriteLine("        SIMULACION MONTE-CARLO SIR - VERSION PARALELA        ");
            Console.WriteLine("=============================================================");
            Console.WriteLine();
            Console.WriteLine($"Parametros:");
            Console.WriteLine($"  Tamano de grilla: {GridSize}x{GridSize} ({GridSize * GridSize:N0} individuos)");
            Console.WriteLine($"  Dias a simular: {Days}");
            Console.WriteLine($"  Infectados iniciales: {InitialInfected}");
            Console.WriteLine($"  P(infeccion): {InfectionProb:F2}");
            Console.WriteLine($"  P(recuperacion): {RecoveryProb:F2}");
            Console.WriteLine($"  P(muerte): {DeathProb:F2}");
            Console.WriteLine($"  Cores disponibles: {Environment.ProcessorCount}");
            Console.WriteLine($"  Cores utilizados: {numThreads}");
            Console.WriteLine();

            Directory.CreateDirectory("output");
            Directory.CreateDirectory("output/grid_snapshots");

            var model = new SIRModelParallel(GridSize, InfectionProb, RecoveryProb,
                                            DeathProb, InitialInfected, numThreads);

            var statistics = new Statistics();
            var visualizer = new Visualizer(GridSize);

            Console.WriteLine("Iniciando simulacion paralela...");
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
            Console.WriteLine($"Cores utilizados: {numThreads}");
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
            statistics.SaveExecutionTime("output/execution_time.txt", 
                                        stopwatch.Elapsed.TotalSeconds, numThreads);

            Console.WriteLine("Archivos generados:");
            Console.WriteLine("  - output/statistics.csv");
            Console.WriteLine("  - output/execution_time.txt");
            Console.WriteLine("  - output/grid_snapshots/");
            Console.WriteLine();
            Console.WriteLine("Simulacion completada exitosamente.");
        }

        static void RunBenchmark()
        {
            const int GridSize = 1000;
            const int Days = 100;
            const double InfectionProb = 0.3;
            const double RecoveryProb = 0.1;
            const double DeathProb = 0.01;
            const int InitialInfected = 10;

            int[] threadCounts = { 1, 2, 4, 8 };
            
            Console.WriteLine("=============================================================");
            Console.WriteLine("            EXPERIMENTO DE STRONG SCALING                    ");
            Console.WriteLine("=============================================================");
            Console.WriteLine();

            var results = new System.Collections.Generic.List<(int cores, double time, double speedup, double efficiency)>();

            foreach (int threads in threadCounts)
            {
                if (threads > Environment.ProcessorCount)
                    break;

                Console.WriteLine($"Ejecutando con {threads} core(s)...");

                var model = new SIRModelParallel(GridSize, InfectionProb, RecoveryProb,
                                                DeathProb, InitialInfected, threads);

                var stopwatch = Stopwatch.StartNew();

                for (int day = 0; day < Days; day++)
                {
                    model.SimulateDay();
                    
                    var stats = model.GetStatistics();
                    if (stats.infected == 0)
                        break;
                }

                stopwatch.Stop();
                double time = stopwatch.Elapsed.TotalSeconds;

                results.Add((threads, time, 0, 0));
                Console.WriteLine($"  Tiempo: {time:F2} segundos");
                Console.WriteLine();
            }

            double baseTime = results[0].time;
            var finalResults = results.Select(r => (
                r.cores,
                r.time,
                speedup: baseTime / r.time,
                efficiency: (baseTime / r.time) / r.cores
            )).ToList();

            Directory.CreateDirectory("../results");
            SaveScalingResults(finalResults, "../results/scaling_results.csv");

            Console.WriteLine("=============================================================");
            Console.WriteLine("              RESULTADOS DE SCALING                          ");
            Console.WriteLine("=============================================================");
            Console.WriteLine($"{"Cores",-10}{"Tiempo (s)",-15}{"Speedup",-15}{"Eficiencia",-15}");
            Console.WriteLine(new string('-', 60));

            foreach (var r in finalResults)
            {
                Console.WriteLine($"{r.cores,-10}{r.time,-15:F2}{r.speedup,-15:F2}{r.efficiency,-15:P2}");
            }
        }

        static void SaveScalingResults(System.Collections.Generic.List<(int cores, double time, double speedup, double efficiency)> results, 
                                      string filename)
        {
            using var writer = new StreamWriter(filename);
            writer.WriteLine("Cores,Time,Speedup,Efficiency");

            foreach (var r in results)
            {
                writer.WriteLine($"{r.cores},{r.time:F4},{r.speedup:F4},{r.efficiency:F4}");
            }
        }
    }
}