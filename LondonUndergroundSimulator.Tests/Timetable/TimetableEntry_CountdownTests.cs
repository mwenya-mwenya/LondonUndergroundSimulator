using LondonUndergroundSimulator.Engine.Models;
using Xunit;
namespace LondonUndergroundSimulator.Tests.Timetable
{
    public class TimetableEntry_CountdownTests
    {
        [Fact]
        public void Countdown_UnderOneMinute_ShowsSeconds()
        {
            var entry = new TimetableEntry
            {
                ExpectedArrival = TimeSpan.FromSeconds(45),
                IsDwelling = false
            };

            var result = entry.GetCountdown(TimeSpan.Zero);

            Assert.Equal("45 sec", result);
        }
    }
}
