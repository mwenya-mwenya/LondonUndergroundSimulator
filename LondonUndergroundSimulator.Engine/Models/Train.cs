using System;
using System.Collections.Generic;
using System.Numerics;

namespace LondonUndergroundSimulator.Engine.Models
{
    public class Train
    {
        public string Id { get; set; }
        public Line Line { get; set; }

        public int CurrentStationIndex { get; set; } = 0;
        public float Progress { get; set; } = 0f;
        public float Speed { get; set; } = 0.005f;
        public Vector2 Position { get; set; }

        public float Rotation { get; set; }
        public int Direction { get; set; } = 1;
        public float HeadDistance { get; set; } = 0f;

        public int CarriageCount { get; set; } = 3;
        public float CarriageSpacing { get; set; } = 10f;
        public float[] CarriageAngles;

        public Track Track { get; set; }

        public Vector2[] CarriagePositions { get; set; }
        public bool IsDwelling { get; set; } = false;
        public float DwellTimer { get; set; } = 0f;
        public float DwellDuration { get; set; } = 1f;

        public bool IsDelayed { get; set; } = false;
        public float DelayTimer { get; set; } = 0f;
        public float DelayDuration { get; set; } = 0f;
        public List<string> DelayReasons { get; set; } = new();

        public TimeSpan DelayStartTime { get; set; } = TimeSpan.Zero;

        public TimeSpan LastDelayEndTime { get; set; } = TimeSpan.Zero;
        public TimeSpan DelayCooldown { get; set; } = TimeSpan.FromSeconds(30); // or whatever you want

        public TimeSpan ExpectedArrivalTime { get; set; }
        public TimeSpan ScheduledArrivalTime { get; set; }

        public TimeSpan[] ArrivalTimestamps { get; set; }
    }
}