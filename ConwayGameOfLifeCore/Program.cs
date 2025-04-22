namespace ConwayGameOfLifeCore
{
  using System;
  using System.Threading.Tasks;

  class Program
  {
    static async Task Main(string[] args)
    {
      LifeSimulation sim = new LifeSimulation(40, 20);
      sim.Randomize();
      for (int i = 0; i < 1000; i++)
      {
        await sim.Update();
        await OutputBoard(sim);
        await Task.Delay(100);
      }

      Console.ReadKey();
    }

    private static async Task OutputBoard(LifeSimulation sim)
    {
      var alive = (name: "1", color: ConsoleColor.Red);
      var dead = (name: "0", color: ConsoleColor.White);

      await Task.Run(
        () =>
          {
            for (int y = 0; y < sim.SizeY; y++)
            {
              for (int x = 0; x < sim.SizeX; x++)
              {
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = sim[x, y] ? alive.color : dead.color;
                Console.Write(sim[x, y] ? alive.name : dead.name);
              }
            }
          });
    }
  }
}
