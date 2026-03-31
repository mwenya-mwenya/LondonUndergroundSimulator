using LondonUndergroundSimulator.Engine.Models;
using LondonUndergroundSimulator.Tests.TestHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LondonUndergroundSimulator.Tests.TestDoubles
{
    public static class TestTrainFactory
    {
        public static Train Create(
            float speed = 10f,
            int currentStationIndex = 0,
            int direction = 1,
            float dwellDuration = 5f)
        {
            var line = LineFactory.Simple();
            var track = TrackFactory.Simple();

            return new Train
            {
                Speed = speed,
                CurrentStationIndex = currentStationIndex,
                Direction = direction,
                DwellDuration = dwellDuration,

                Line = line,
                Track = track,

                // REQUIRED ARRAYS
                ArrivalTimestamps = new TimeSpan[line.Stations.Count],
                CarriageCount = 1,
                CarriagePositions = new Vector2[1],
                CarriageAngles = new float[1],

                // OPTIONAL BUT SAFE DEFAULTS
                CarriageSpacing = 10f,
                Progress = 0f
            };
        }
    }
}
