namespace ConwayGameOfLifeCore
{
  using System;
  using System.Threading.Tasks;

  public class LifeSimulation
  {
    private bool[,] world;
    private bool[,] nextGeneration;
    private Task processTask;

    public LifeSimulation(int size)
    {
      if (size < 0) throw new ArgumentOutOfRangeException("Size must be greater than zero");
      Size = size;
      world = new bool[size, size];
      nextGeneration = new bool[size, size];
    }

    public int Size { get; private set; }
    public int Generation { get; private set; }

    public Action<bool[,]> NextGenerationCompleted;

    public bool this[int x, int y]
    {
      get { return world[x, y]; }
      set { world[x, y] = value; }
    }

    public bool ToggleCell(int x, int y)
    {
      bool currentValue = world[x, y];
      return world[x, y] = !currentValue;
    }

    public void Update()
    {
      if (processTask != null && processTask.IsCompleted)
      {
        // when a generation has completed
        // now flip the back buffer so we can start processing on the next generation
        var flip = nextGeneration;
        nextGeneration = world;
        world = flip;
        Generation++;

        // begin the next generation's processing asynchronously
        processTask = ProcessGeneration();

        NextGenerationCompleted?.Invoke(world);
      }
    }

    public void BeginGeneration()
    {
      if (processTask == null || (processTask != null && processTask.IsCompleted))
      {
        // only begin the generation if the previous process was completed
        processTask = ProcessGeneration();
      }
    }

    public async Task Wait()
    {
      await processTask;
    }

    private Task ProcessGeneration()
    {
      return Task.Factory.StartNew(() =>
        {
          Parallel.For(0, Size, x =>
            {
              Parallel.For(0, Size, y =>
                {
                  int numberOfNeighbors = IsNeighborAlive(world, Size, x, y, -1, 0)
                                          + IsNeighborAlive(world, Size, x, y, -1, 1)
                                          + IsNeighborAlive(world, Size, x, y, 0, 1)
                                          + IsNeighborAlive(world, Size, x, y, 1, 1)
                                          + IsNeighborAlive(world, Size, x, y, 1, 0)
                                          + IsNeighborAlive(world, Size, x, y, 1, -1)
                                          + IsNeighborAlive(world, Size, x, y, 0, -1)
                                          + IsNeighborAlive(world, Size, x, y, -1, -1);

                  bool shouldLive = false;
                  bool isAlive = world[x, y];

                  if (isAlive && (numberOfNeighbors == 2 || numberOfNeighbors == 3))
                  {
                    shouldLive = true;
                  }
                  else if (!isAlive && numberOfNeighbors == 3) // zombification
                  {
                    shouldLive = true;
                  }

                  nextGeneration[x, y] = shouldLive;

                });
            });
        });
    }

    private static int IsNeighborAlive(bool[,] world, int size, int x, int y, int offsetx, int offsety)
    {
      int result = 0;

      int proposedOffsetX = x + offsetx;
      int proposedOffsetY = y + offsety;
      bool outOfBounds = proposedOffsetX < 0 || proposedOffsetX >= size | proposedOffsetY < 0 || proposedOffsetY >= size;
      if (!outOfBounds)
      {
        result = world[x + offsetx, y + offsety] ? 1 : 0;
      }
      return result;
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