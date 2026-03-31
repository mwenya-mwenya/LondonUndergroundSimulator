using LondonUndergroundSimulator.Engine.Models;
using System.Collections.Generic;

namespace LondonUndergroundSimulator.Tests.TestHelpers
{
    public static class LineFactory
    {
        public static Line Simple()
        {
            return new Line
            {
                Name = "TestLine",
                ColorHex = "#FFFFFF",
                Stations = new List<Station>
        {
            new Station { Name = "A" },
            new Station { Name = "B" },
            new Station { Name = "C" }
        },
                StationDistances = new List<float> { 0f, 100f, 200f }
            };
        }   
    }
}