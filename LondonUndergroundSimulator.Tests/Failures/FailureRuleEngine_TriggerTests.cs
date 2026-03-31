using LondonUndergroundSimulator.Engine.Interfaces;
using LondonUndergroundSimulator.Engine.Models;
using LondonUndergroundSimulator.Engine.Services;
using LondonUndergroundSimulator.Tests.TestDoubles;
using Xunit;

namespace LondonUndergroundSimulator.Tests.Failures
{
    public class FailureRuleEngine_TriggerTests
    {
        [Fact]
        public void Rule_Triggers_WhenProbabilityIsOne()
        {
            var rule = new FakeRule
            {
                ShouldTriggerReturn = true,
                DelayDuration = 20
            };

            var engine = new FakeFailureEngine
            {
                RuleToReturn = new FakeRule
                {
                    ShouldTriggerReturn = true,
                    DelayDuration = 20
                }
            };


            var train = new Train();

            var result = engine.Evaluate(train, 1f);

            Assert.NotNull(result);
            Assert.Equal(20, result.GetDelayDuration(train));
        }
    }
}