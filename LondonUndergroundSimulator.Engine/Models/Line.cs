using System;
using System.Collections.Generic;
using System.Numerics;

namespace LondonUndergroundSimulator.Engine.Models
{
    public class Line
    {
        public string Name { get; set; }

        public int NextTrainId { get; set; } = 1;
        public string ColorHex { get; set; }
        public List<Station> Stations { get; set; }

        public int MaxSafeTrainCount { get; set; } = 10;
        public List<Vector2> Geometry { get; set; } = new();
        public List<float> SegmentLengths { get; set; } = new();
        public float TotalLength { get; set; }
        public List<float> StationDistances { get; set; } = new();

        public Track Eastbound { get; set; }
        public Track Westbound { get; set; }

        public int NumberOfTrains { get; set; } = 1;

        public int TargetNumberOfTrains { get; set; } = 4;
        public List<Train> Trains { get; set; } = new();

        public bool IsServiceActive {  get; set; } = true;


    }
}
