using System;
using System.Collections.Generic;
using System.Numerics;

namespace LondonUndergroundSimulator.Engine.Models
{
    public class Line
    {
        public string Name { get; set; }
        public string ColorHex { get; set; }
        public List<Station> Stations { get; set; }

        public List<Vector2> Geometry { get; set; } = new();
        public List<float> SegmentLengths { get; set; } = new();
        public float TotalLength { get; set; }
        public List<float> StationDistances { get; set; } = new();
    
        public Track Eastbound { get; set; }
        public Track Westbound { get; set; }

        public int NumberOfTrains { get; set; } = 5;
        public List<Train> Trains { get; set; } = new();

    }
}
