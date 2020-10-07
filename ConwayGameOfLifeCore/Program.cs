namespace ConwayGameOfLifeCore
{
  using System;
  using System.Threading.Tasks;

  class Program
  {
    static async Task Main(string[] args)
    {
      LifeSimulation sim = new LifeSimulation(40);
      sim.Randomize();
      sim.BeginGeneration();
      for (int i = 0; i < 1000; i++)
      {
        sim.Update();
        await sim.Wait();
        OutputBoard(sim);
        await Task.Delay(500);
      }

      Console.ReadKey();
    }

    private static void OutputBoard(LifeSimulation sim)
    {
      var alive = (name: "1", color: ConsoleColor.Red);
      var dead = (name: "0", color: ConsoleColor.White);
      for (int y = 0; y < sim.Size; y++)
      {
        for (int x = 0; x < sim.Size; x++)
        {
          Console.SetCursorPosition(x, y);
          Console.ForegroundColor = sim[x, y] ? alive.color : dead.color;
          Console.Write(sim[x, y] ? alive.name : dead.name);
        }
      }
    }
  }
}
