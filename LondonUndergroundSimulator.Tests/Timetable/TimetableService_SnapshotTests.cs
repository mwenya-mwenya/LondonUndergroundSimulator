using LondonUndergroundSimulator.Engine.Services;
using LondonUndergroundSimulator.Tests.TestDoubles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LondonUndergroundSimulator.Tests.Timetable
{
    public class TimetableService_SnapshotTests
    {
        [Fact]
        public void Snapshot_UsesTrainTimestamps()
        {
            var train = TestTrainFactory.Create();
            train.ScheduledArrivalTime = TimeSpan.FromSeconds(30);
            train.ExpectedArrivalTime = TimeSpan.FromSeconds(40);

            var engine = new SimulationEngineBuilder().WithTrain(train).Build();
            var service = new TimetableService(engine);

            var entries = service.BuildTimetable();

            Assert.Equal(TimeSpan.FromSeconds(30), entries[0].ScheduledArrival);
            Assert.Equal(TimeSpan.FromSeconds(40), entries[0].ExpectedArrival);
        }
    }
}
