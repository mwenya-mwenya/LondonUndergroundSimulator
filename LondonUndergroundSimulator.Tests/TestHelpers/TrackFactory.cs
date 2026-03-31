using LondonUndergroundSimulator.Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LondonUndergroundSimulator.Tests.TestHelpers
{
    public static class TrackFactory
    {
        public static Track Simple()
        {
            return new Track
            {
                Points = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(100, 0),
            new Vector2(200, 0)
        },
                TotalLength = 200f,
                StationDistances = new List<float> { 0f, 100f, 200f }
            };
        }
    }
}
