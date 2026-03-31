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
    public class TrainMovement_DwellTests
    {
        [Fact]
        public void Train_Dwells_AndDoesNotMove()
        {
            var train = TestTrainFactory.Create();
            train.IsDwelling = true;
            train.DwellTimer = 5f;

            var engine = new SimulationEngineBuilder().WithTrain(train).Build();
            var movement = new TrainMovementService(engine);

            movement.UpdateMovement(train, 1f);

            Assert.True(train.IsDwelling);
            Assert.Equal(4f, train.DwellTimer);
            Assert.Equal(0f, train.Progress);
        }
    }
}
