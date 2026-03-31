using LondonUndergroundSimulator.Engine.Interfaces;
using LondonUndergroundSimulator.Engine.Models;
using LondonUndergroundSimulator.Engine.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LondonUndergroundSimulator.Tests.TestDoubles
{
    public class SimulationEngineBuilder
    {
        private readonly List<Train> _trains = new();
        private ITrainDelayService _delay = new FakeDelayService();
        private IFailureRuleEngine _failures = new FakeFailureEngine();

        public SimulationEngineBuilder WithTrain(Train train)
        {
            _trains.Add(train);
            return this;
        }

        public SimulationEngineBuilder WithDelayService(ITrainDelayService service)
        {
            _delay = service;
            return this;
        }

        public SimulationEngineBuilder WithFailureEngine(IFailureRuleEngine engine)
        {
            _failures = engine;
            return this;
        }

        public SimulationEngine Build()
        {
            var engine = new SimulationEngine(_trains, _delay, _failures, false);

            return engine;
        }
    }
}
