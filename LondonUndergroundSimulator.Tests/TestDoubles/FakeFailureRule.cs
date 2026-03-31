using LondonUndergroundSimulator.Engine.Interfaces;
using LondonUndergroundSimulator.Engine.Models;

namespace LondonUndergroundSimulator.Tests.TestDoubles
{
    public class FakeRule : IFailureRule
    {
        public bool ShouldTriggerReturn { get; set; }
        public float DelayDuration { get; set; }
        public string Name { get; set; } = "FakeRule";

        public bool ShouldTrigger(float deltaTime) => ShouldTriggerReturn;

        public float GetDelayDuration(Train train) => DelayDuration;
    }
}