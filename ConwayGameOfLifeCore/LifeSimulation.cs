using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConwayGameOfLifeCore
{
    public class LifeSimulation(int sizex, int sizey)
    {
        private bool[,] world;

        private readonly List<(int offsetx, int offsety)> neighbors =
        [
            (-1, 0),
            (-1, 1),
            (0, 1),
            (1, 1),
            (1, 0),
            (1, -1),
            (0, -1),
            (-1, -1)
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
            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    bool shouldLive = false;
                    await Task.Run(
                      () =>
                        {
                            int numberOfAliveNeighbors = neighbors.Select(tuple => (x: tuple.offsetx + x, y: tuple.offsety + y)).Select(IsNeighborAlive).Sum();
                            bool isAlive = world[x, y];

                            if (isAlive && (numberOfAliveNeighbors == 2 || numberOfAliveNeighbors == 3))
                            {
                                shouldLive = true;
                            }
                            else if (!isAlive && numberOfAliveNeighbors == 3) //zombification
                            {
                                shouldLive = true;
                            }
                        });

                    yield return (x, y, shouldLive);
                }
            }
        }

        private int IsNeighborAlive((int x, int y) neighbor)
        {
            bool outOfBounds = neighbor.x < 0 || neighbor.x >= SizeX | neighbor.y < 0 || neighbor.y >= SizeY;
            return Convert.ToInt32(!outOfBounds && world[neighbor.x, neighbor.y]);
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
