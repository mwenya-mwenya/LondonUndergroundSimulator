using LondonUndergroundSimulator.Engine.Services;
using LondonUndergroundSimulator.Tests.TestDoubles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LondonUndergroundSimulator.Tests.Movement
{
    public class TrainMovement_SegmentTests
    {
        [Fact]
        public void Train_CompletesSegment_EntersDwell()
        {
            var train = TestTrainFactory.Create();
            var engine = new SimulationEngineBuilder().WithTrain(train).Build();
            var movement = new TrainMovementService(engine);

            train.Progress = 0.9999f;

            movement.UpdateMovement(train, 1f);

            Assert.True(train.IsDwelling);
            Assert.Equal(0f, train.Progress);
        }
    }
}
