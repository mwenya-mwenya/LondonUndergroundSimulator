using LondonUndergroundSimulator.Engine.Interfaces;
using LondonUndergroundSimulator.Engine.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LondonUndergroundSimulator.Engine.Services
{
    /// <summary>
    /// High‑level orchestrator responsible ONLY for:
    /// - advancing simulation time
    /// - delegating movement, delays, and failure evaluation
    /// </summary>
    public class SimulationEngine
    {
        public readonly ITrainMovementService _movementService;
        private readonly ITrainDelayService _delayService;
        private readonly IFailureRuleEngine _failureEngine;

        public IReadOnlyList<Train> Trains => _trains;
        public readonly List<Train> _trains;
        public ITrainMovementService MovementService => _movementService;
        public float TimeScale { get; set; } = 4f;
        public float SimulationTime { get; private set; } = 0f;
        public TimeSpan Clock => TimeSpan.FromSeconds(SimulationTime);
      

        public SimulationEngine(
            List<Train> trains,

            ITrainDelayService delayService,
            IFailureRuleEngine failureEngine,
            bool initialiseMovement = true
            )
        {
            _trains = trains;
          
            _delayService = delayService;
            _failureEngine = failureEngine;
            _movementService = new TrainMovementService(this);

            if (initialiseMovement)
            {
                foreach (var train in _trains)
                    _movementService.InitialiseTrain(train);
            }
        }

        

        /// <summary>
        /// Advances the simulation by deltaTime (scaled).
        /// </summary>
        public void Update(float deltaTime)
        {

            SimulationTime += deltaTime * TimeScale;
            float scaledDelta = deltaTime * TimeScale;



            foreach (var train in _trains)
            {
                // 1. Handle active delay window
                if (_delayService.IsTrainDelayed(train))
                {
                    _delayService.UpdateDelay(train, scaledDelta, Clock);
                    if (train.IsDelayed)
                        continue; // still delayed → skip movement
                }

                // 2. Evaluate failure rules (if not delayed)
                var triggeredRule = _failureEngine.Evaluate(train, scaledDelta);
                if (triggeredRule != null)
                {
                    _delayService.ApplyDelay(train, triggeredRule, Clock);
                    continue; // delay begins → skip movement
                }

                // 3. Handle dwell logic
                if (_delayService.UpdateDwell(train, scaledDelta))
                    continue; // still dwelling → skip movement

                // 4. Delegate movement + geometry
                _movementService.UpdateMovement(train, scaledDelta);
                _movementService.UpdateGeometry(train, scaledDelta);

            }
        }

        public string GetFormattedTime()
        {
            var ts = Clock;
            return $"TIME: {ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }
    }
}