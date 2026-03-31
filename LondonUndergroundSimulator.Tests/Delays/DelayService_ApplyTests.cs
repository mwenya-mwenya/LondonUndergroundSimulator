using LondonUndergroundSimulator.Engine.Interfaces;
using LondonUndergroundSimulator.Engine.Services;
using LondonUndergroundSimulator.Tests.TestDoubles;
using Xunit;

namespace LondonUndergroundSimulator.Tests.Delays
{
    public class DelayService_ApplyTests
    {
        [Fact]
        public void ApplyDelay_SetsDelayStateCorrectly()
        {
            var service = new TrainDelayService();
            var train = TestTrainFactory.Create();

            var rule = new FakeRule
            {
                ShouldTriggerReturn = true,
                DelayDuration = 30,
                Name = "Signal Failure"
            };

            service.ApplyDelay(train, rule, TimeSpan.FromSeconds(100));

            Assert.True(train.IsDelayed);
            Assert.Equal(TimeSpan.FromSeconds(100), train.DelayStartTime);
            Assert.Equal(30, train.DelayDuration);
            Assert.Contains("Signal Failure", train.DelayReasons);
        }
    }
}
