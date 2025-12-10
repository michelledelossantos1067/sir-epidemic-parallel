using System;

namespace EpidemicSimulation.Sequential
{
    public enum CellState : byte
    {
        Susceptible = 0,
        Infected = 1,
        Recovered = 2,
        Dead = 3
    }

    public class SIRModel
    {
        public int GridSize { get; }
        public double InfectionProb { get; }
        public double RecoveryProb { get; }
        public double DeathProb { get; }
        public int InitialInfected { get; }

        private CellState[,] grid;
        private Random random;

        public SIRModel(int gridSize, double infectionProb, double recoveryProb, 
                       double deathProb, int initialInfected, int seed = 42)
        {
            GridSize = gridSize;
            InfectionProb = infectionProb;
            RecoveryProb = recoveryProb;
            DeathProb = deathProb;
            InitialInfected = initialInfected;
            
            grid = new CellState[gridSize, gridSize];
            random = new Random(seed);
            
            PlaceInitialInfected();
        }

        private void PlaceInitialInfected()
        {
            int placed = 0;
            while (placed < InitialInfected)
            {
                int x = random.Next(GridSize);
                int y = random.Next(GridSize);
                
                if (grid[x, y] == CellState.Susceptible)
                {
                    grid[x, y] = CellState.Infected;
                    placed++;
                }
            }
        }

        public CellState[,] GetGrid()
        {
            return (CellState[,])grid.Clone();
        }

        public void SimulateDay()
        {
            CellState[,] newGrid = (CellState[,])grid.Clone();

            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    CellState currentState = grid[i, j];

                    if (currentState == CellState.Susceptible)
                    {
                        int infectedNeighbors = CountInfectedNeighbors(i, j);
                        
                        if (infectedNeighbors > 0)
                        {
                            double infectionChance = 1.0 - Math.Pow(1.0 - InfectionProb, infectedNeighbors);
                            
                            if (random.NextDouble() < infectionChance)
                            {
                                newGrid[i, j] = CellState.Infected;
                            }
                        }
                    }
                    else if (currentState == CellState.Infected)
                    {
                        if (random.NextDouble() < DeathProb)
                        {
                            newGrid[i, j] = CellState.Dead;
                        }
                        else if (random.NextDouble() < RecoveryProb)
                        {
                            newGrid[i, j] = CellState.Recovered;
                        }
                    }
                }
            }

            grid = newGrid;
        }

        private int CountInfectedNeighbors(int i, int j)
        {
            int count = 0;

            for (int di = -1; di <= 1; di++)
            {
                for (int dj = -1; dj <= 1; dj++)
                {
                    if (di == 0 && dj == 0) continue;

                    int ni = i + di;
                    int nj = j + dj;

                    if (ni >= 0 && ni < GridSize && nj >= 0 && nj < GridSize)
                    {
                        if (grid[ni, nj] == CellState.Infected)
                        {
                            count++;
                        }
                    }
                }
            }

            return count;
        }

        public (int susceptible, int infected, int recovered, int dead) GetStatistics()
        {
            int s = 0, inf = 0, r = 0, d = 0;

            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    switch (grid[i, j])
                    {
                        case CellState.Susceptible: s++; break;
                        case CellState.Infected: inf++; break;
                        case CellState.Recovered: r++; break;
                        case CellState.Dead: d++; break;
                    }
                }
            }

            return (s, inf, r, d);
        }

        public double CalculateR0(int day)
        {
            if (day == 0 || InitialInfected == 0)
                return 0.0;

            var stats = GetStatistics();
            int totalInfected = stats.infected + stats.recovered + stats.dead;
            
            return (double)totalInfected / InitialInfected;
        }
    }
}