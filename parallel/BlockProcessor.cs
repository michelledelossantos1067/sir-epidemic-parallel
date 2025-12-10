using System;

namespace EpidemicSimulation.Parallel
{
    public class BlockProcessor
    {
        private int gridSize;
        private double infectionProb;
        private double recoveryProb;
        private double deathProb;

        public BlockProcessor(int gridSize, double infectionProb, 
                            double recoveryProb, double deathProb)
        {
            this.gridSize = gridSize;
            this.infectionProb = infectionProb;
            this.recoveryProb = recoveryProb;
            this.deathProb = deathProb;
        }

        public void ProcessBlock(CellState[,] grid, CellState[,] newGrid, 
                                int startI, int endI, int startJ, int endJ, 
                                Random random)
        {
            for (int i = startI; i < endI; i++)
            {
                for (int j = startJ; j < endJ; j++)
                {
                    CellState currentState = grid[i, j];

                    if (currentState == CellState.Susceptible)
                    {
                        int infectedNeighbors = CountInfectedNeighbors(grid, i, j);

                        if (infectedNeighbors > 0)
                        {
                            double infectionChance = 1.0 - Math.Pow(1.0 - infectionProb, infectedNeighbors);

                            if (random.NextDouble() < infectionChance)
                            {
                                newGrid[i, j] = CellState.Infected;
                            }
                        }
                    }
                    else if (currentState == CellState.Infected)
                    {
                        if (random.NextDouble() < deathProb)
                        {
                            newGrid[i, j] = CellState.Dead;
                        }
                        else if (random.NextDouble() < recoveryProb)
                        {
                            newGrid[i, j] = CellState.Recovered;
                        }
                    }
                }
            }
        }

        private int CountInfectedNeighbors(CellState[,] grid, int i, int j)
        {
            int count = 0;

            for (int di = -1; di <= 1; di++)
            {
                for (int dj = -1; dj <= 1; dj++)
                {
                    if (di == 0 && dj == 0) continue;

                    int ni = i + di;
                    int nj = j + dj;

                    if (ni >= 0 && ni < gridSize && nj >= 0 && nj < gridSize)
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

        public (int startI, int endI, int startJ, int endJ) GetBlockBounds(
            int blockId, int blocksPerDim, int blockSize)
        {
            int blockRow = blockId / blocksPerDim;
            int blockCol = blockId % blocksPerDim;

            int startI = blockRow * blockSize;
            int endI = Math.Min(startI + blockSize, gridSize);
            int startJ = blockCol * blockSize;
            int endJ = Math.Min(startJ + blockSize, gridSize);

            return (startI, endI, startJ, endJ);
        }

        public (int, int, int, int) GetBlockBoundsWithGhost(
            int blockId, int blocksPerDim, int blockSize)
        {
            var (startI, endI, startJ, endJ) = GetBlockBounds(blockId, blocksPerDim, blockSize);

            int ghostStartI = Math.Max(0, startI - 1);
            int ghostEndI = Math.Min(gridSize, endI + 1);
            int ghostStartJ = Math.Max(0, startJ - 1);
            int ghostEndJ = Math.Min(gridSize, endJ + 1);

            return (ghostStartI, ghostEndI, ghostStartJ, ghostEndJ);
        }
    }
}