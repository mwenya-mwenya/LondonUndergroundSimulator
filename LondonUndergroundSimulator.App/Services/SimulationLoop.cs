using LondonUndergroundSimulator.HostServices.Logging;
using LondonUndergroundSimulator.Engine.Models;
using LondonUndergroundSimulator.Engine.Services;

namespace LondonUndergroundSimulator.HostServices
{
    public class SimulationLoop
    {
        private readonly SimulationEngine _engine;
        private readonly SimulationLogger _logger;
        private readonly int _tickMilliseconds;

        public SimulationLoop(
            SimulationEngine engine,
            SimulationLogger logger,
            int tickMilliseconds = 100)
        {
            _engine = engine;
            _logger = logger;
            _tickMilliseconds = tickMilliseconds;
        }

        public void Run()
        {
            Console.WriteLine("=== Simulation Started ===");

            while (true)
            {
                // Update simulation world
                _engine.Update(0.1f);

                // Log train state
                foreach (var train in _engine._trains)
                {
                    _logger.LogTrain(train);
                }

                // Control tick rate
                Thread.Sleep(_tickMilliseconds);
            }
        }
    }
}
