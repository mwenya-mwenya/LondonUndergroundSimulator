using LondonUndergroundSimulator.Engine.Interfaces;
using LondonUndergroundSimulator.Engine.Models;

namespace LondonUndergroundSimulator.Tests.TestDoubles
{
    public class FakeFailureEngine : IFailureRuleEngine
    {
        public IFailureRule? RuleToReturn { get; set; }

        public IFailureRule Evaluate(Train t, float dt)
            => RuleToReturn;
    }
}