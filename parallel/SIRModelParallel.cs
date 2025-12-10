using System;
using System.Threading.Tasks;

namespace EpidemicSimulation.Parallel
{
    public enum CellState : byte
    {
        Susceptible = 0,
        Infected = 1,
        Recovered = 2,
        Dead = 3
    }

    public class SIRModelParallel
    {
        public int GridSize { get; }
        public double InfectionProb { get; }
        public double RecoveryProb { get; }
        public double DeathProb { get; }
        public int InitialInfected { get; }
        public int NumThreads { get; }

        private CellState[,] grid;
        private Random[] randoms;
        private int blocksPerDim;
        private int blockSize;

        public SIRModelParallel(int gridSize, double infectionProb, double recoveryProb,
                               double deathProb, int initialInfected, int numThreads, int seed = 42)
        {
            GridSize = gridSize;
            InfectionProb = infectionProb;
            RecoveryProb = recoveryProb;
            DeathProb = deathProb;
            InitialInfected = initialInfected;
            NumThreads = numThreads;

            grid = new CellState[gridSize, gridSize];
            
            randoms = new Random[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                randoms[i] = new Random(seed + i);
            }

            blocksPerDim = (int)Math.Ceiling(Math.Sqrt(numThreads));
            blockSize = (int)Math.Ceiling((double)gridSize / blocksPerDim);

            PlaceInitialInfected(seed);
        }

        private void PlaceInitialInfected(int seed)
        {
            var random = new Random(seed);
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
            int totalBlocks = blocksPerDim * blocksPerDim;

            System.Threading.Tasks.Parallel.For(0, totalBlocks, new ParallelOptions 
            { 
                MaxDegreeOfParallelism = NumThreads 
            }, 
            blockId =>
            {
                int threadId = Task.CurrentId ?? blockId;
                threadId = threadId % NumThreads;
                
                ProcessBlock(blockId, newGrid, randoms[threadId]);
            });

            grid = newGrid;
        }

        private void ProcessBlock(int blockId, CellState[,] newGrid, Random random)
        {
            int blockRow = blockId / blocksPerDim;
            int blockCol = blockId % blocksPerDim;

            int startI = blockRow * blockSize;
            int endI = Math.Min(startI + blockSize, GridSize);
            int startJ = blockCol * blockSize;
            int endJ = Math.Min(startJ + blockSize, GridSize);

            for (int i = startI; i < endI; i++)
            {
                for (int j = startJ; j < endJ; j++)
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
            int[] counts = new int[4];

            System.Threading.Tasks.Parallel.For(0, GridSize, i =>
            {
                int localS = 0, localInf = 0, localR = 0, localD = 0;

                for (int j = 0; j < GridSize; j++)
                {
                    switch (grid[i, j])
                    {
                        case CellState.Susceptible: localS++; break;
                        case CellState.Infected: localInf++; break;
                        case CellState.Recovered: localR++; break;
                        case CellState.Dead: localD++; break;
                    }
                }

                System.Threading.Interlocked.Add(ref counts[0], localS);
                System.Threading.Interlocked.Add(ref counts[1], localInf);
                System.Threading.Interlocked.Add(ref counts[2], localR);
                System.Threading.Interlocked.Add(ref counts[3], localD);
            });

            return (counts[0], counts[1], counts[2], counts[3]);
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