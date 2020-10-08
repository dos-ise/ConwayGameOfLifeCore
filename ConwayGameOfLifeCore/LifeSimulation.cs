namespace ConwayGameOfLifeCore
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;

  public class LifeSimulation
  {
    private bool[,] world;
    private readonly List<(int offsetx, int offsety)> neighbors = new List<(int offsetx, int offsety)>()
                                                                    {
                                                                      (-1, 0),
                                                                      (-1, 1),
                                                                      (0, 1),
                                                                      (1, 1),
                                                                      (1, 0),
                                                                      (1, -1),
                                                                      (0, -1),
                                                                      (-1, -1),
                                                                    };
    public LifeSimulation(int size)
    {
      if (size < 0) throw new ArgumentOutOfRangeException("Size must be greater than zero");
      Size = size;
      world = new bool[size, size];
    }

    public int Size { get; }

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
      bool[,] nextGeneration = new bool[Size, Size];
      await foreach (var dataPoint in GetUpdatedDatapoints())
      {
        nextGeneration[dataPoint.x, dataPoint.y] = dataPoint.shouldLive;
      }

      return nextGeneration;
    }
    private async IAsyncEnumerable<(int x, int y, bool shouldLive)> GetUpdatedDatapoints()
    {
      for (int x = 0; x < Size; x++)
      {
        for (int y = 0; y < Size; y++)
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
      bool outOfBounds = neighbor.x < 0 || neighbor.x >= Size | neighbor.y < 0 || neighbor.y >= Size;
      return Convert.ToInt32(!outOfBounds && world[neighbor.x, neighbor.y]);
    }

    public void Randomize()
    {
      var random = new Random();
      for (int x = 0; x < Size; x++)
      {
        for (int y = 0; y < Size; y++)
        {
          world[x, y] = random.Next(2) != 0;
        }
      }
    }
  }
}