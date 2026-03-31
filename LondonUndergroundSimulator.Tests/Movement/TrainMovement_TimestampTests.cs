using LondonUndergroundSimulator.Engine.Services;
using LondonUndergroundSimulator.Tests.TestDoubles;
using LondonUndergroundSimulator.Tests.TestHelpers;

public class TrainMovement_TimestampTests
{
    [Fact]
    public void CompletingSegment_SetsArrivalTimestamps()
    {
        var train = TestTrainFactory.Create(
            speed: 10f,
            currentStationIndex: 0
        );

        var engine = new FakeEngine(train);
        var movement = engine.MovementService;

        movement.InitialiseTrain(train);

        // Simulate enough time to reach station B
        movement.UpdateMovement(train, 10f);

        Assert.True(train.ScheduledArrivalTime > TimeSpan.Zero);
        Assert.True(train.ExpectedArrivalTime >= train.ScheduledArrivalTime);
    }
}