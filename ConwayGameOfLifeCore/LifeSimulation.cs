using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConwayGameOfLifeCore
{
    /// <summary>
    /// Primary constructors
    /// </summary>
    /// <param name="sizex"></param>
    /// <param name="sizey"></param>
    public class LifeSimulation(int sizex, int sizey)
    {
        private bool[,] world;

        private readonly (int offsetx, int offsety)[] neighbors =
        [
           (-1, 1), (0, 1),   (1, 1),   
           (-1, 0),      /*CENTER*/         (1, 0), 
           (-1, -1), (0, -1), (1, -1)
        ];

        public int SizeX { get; } = sizex;
        public int SizeY { get; } = sizey;

        public int Generation { get; private set; }

        public bool this[int x, int y]
        {
            get => world[x, y];
            set => world[x, y] = value;
        }

        public async Task Update()
        {
            world = await ProcessGeneration();
            Generation++;
        }

        private async Task<bool[,]> ProcessGeneration()
        {
            bool[,] nextGeneration = new bool[SizeX, SizeY];
            await foreach (var dataPoint in GetUpdatedDatapoints())
            {
                nextGeneration[dataPoint.x, dataPoint.y] = dataPoint.shouldLive;
            }

            return nextGeneration;
        }

        private async IAsyncEnumerable<(int x, int y, bool shouldLive)> GetUpdatedDatapoints()
        {
            await foreach (var (x, y, isAlive, aliveNeighbors) in GetCellsWithNeighborCount())
            {
                yield return (x, y, isAlive switch
                {
                    true when aliveNeighbors is 2 or 3 => true,   // Cell stays alive
                    false when aliveNeighbors == 3 => true,      // Cell becomes alive (zombification)
                    _ => false                                   // Cell dies
                });
            }
        }

        private async IAsyncEnumerable<(int x, int y, bool isAlive, int aliveNeighbors)> GetCellsWithNeighborCount()
        {
            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    bool isAlive = world[x, y];
                    int aliveNeighbors = await Task.Run(() =>
                        neighbors
                            .Select(tuple => (x: tuple.offsetx + x, y: tuple.offsety + y))
                            .Count(IsNeighborAlive)
                    );

                    yield return (x, y, isAlive, aliveNeighbors);
                }
            }
        }

        private bool IsNeighborAlive((int x, int y) neighbor)
        {
            bool outOfBounds = neighbor.x < 0 || neighbor.x >= SizeX | neighbor.y < 0 || neighbor.y >= SizeY;
            return !outOfBounds && world[neighbor.x, neighbor.y];
        }

        public void Randomize()
        {
            var random = new Random();
            world = new bool[SizeX, SizeY];
            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    world[x, y] = random.Next(2) != 0;
                }
            }
        }
    }
}
