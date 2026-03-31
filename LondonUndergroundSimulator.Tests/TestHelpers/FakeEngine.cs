using LondonUndergroundSimulator.Engine.Models;
using LondonUndergroundSimulator.Engine.Services;
using LondonUndergroundSimulator.Tests.TestDoubles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LondonUndergroundSimulator.Tests.TestHelpers
{
    public class FakeEngine : SimulationEngine
    {
        public FakeEngine(Train train)
            : base(new List<Train> { train }, new FakeDelayService(), new FakeFailureEngine(), false)
        {
            // No need to set SimulationTime — it's already 0
        }
    }
}
