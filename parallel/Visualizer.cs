using System;
using System.IO;
using System.Text;

namespace EpidemicSimulation.Parallel
{
    public class Visualizer
    {
        private int gridSize;
        private const int SampleSize = 50;

        public Visualizer(int gridSize)
        {
            this.gridSize = gridSize;
        }

        public void PrintGridSummary(CellState[,] grid, int day)
        {
            int step = gridSize / SampleSize;

            Console.WriteLine();
            Console.WriteLine($"  Muestra de grilla (dia {day}):");
            Console.Write("  ");

            for (int i = 0; i < SampleSize && i * step < gridSize; i++)
            {
                for (int j = 0; j < SampleSize && j * step < gridSize; j++)
                {
                    char symbol = grid[i * step, j * step] switch
                    {
                        CellState.Susceptible => '.',
                        CellState.Infected => 'I',
                        CellState.Recovered => 'R',
                        CellState.Dead => 'X',
                        _ => '?'
                    };
                    Console.Write(symbol);
                }

                if (i < SampleSize - 1)
                {
                    Console.WriteLine();
                    Console.Write("  ");
                }
            }
            Console.WriteLine();
        }

        public void SaveGridSnapshot(CellState[,] grid, int day, string directory)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    sb.Append((int)grid[i, j]);
                    if (j < gridSize - 1)
                        sb.Append(',');
                }
                sb.AppendLine();
            }

            string filename = Path.Combine(directory, $"grid_day_{day:D3}.csv");
            File.WriteAllText(filename, sb.ToString());
        }

        public void PrintColoredCell(CellState state)
        {
            ConsoleColor color = state switch
            {
                CellState.Susceptible => ConsoleColor.Blue,
                CellState.Infected => ConsoleColor.Red,
                CellState.Recovered => ConsoleColor.Green,
                CellState.Dead => ConsoleColor.DarkGray,
                _ => ConsoleColor.White
            };

            Console.ForegroundColor = color;
            Console.Write("█");
            Console.ResetColor();
        }
    }
}