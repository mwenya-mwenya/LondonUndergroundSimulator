using LondonUndergroundSimulator.Engine.Interfaces;
using LondonUndergroundSimulator.Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LondonUndergroundSimulator.Tests.TestDoubles
{
    public class FakeDelayService : ITrainDelayService
    {
        public bool IsTrainDelayed(Train t) => false;
        public void UpdateDelay(Train t, float dt, TimeSpan clock) { }
        public void ApplyDelay(Train t, IFailureRule rule, TimeSpan clock) { }
        public bool UpdateDwell(Train t, float dt) => false;
    }
}
