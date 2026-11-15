using LondonUndergroundSimulator.Engine.Interfaces;
using LondonUndergroundSimulator.Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LondonUndergroundSimulator.Engine.Services
{
    public class TerminalLoopService : ITerminalLoopService
    {
        public ITrainMovementService _movement;

        public IReadOnlyList<Line> _lines;

 
        public void HandleLoop(Train train)
        {
            

            if (train.Track == train.Line.Eastbound)
            {
                var trainId = train.Id;
                train.Track = train.Line.Westbound;
                train.Track.TrackDirection = "Westbound";
                //train.Id = trainId.Replace("East", "West");
            } else
            {
                var trainId = train.Id;
                train.Track = train.Line.Eastbound;
                train.Track.TrackDirection = "Eastbound";
                //train.Id = trainId.Replace("West", "East");
            }
         
            // Reset station index
            train.CurrentStationIndex = 0;

            // Reset progress
            //train.Progress = train.Direction == 1 ? 0f : 1f;
            train.Progress = 0f;

            train.CurrentPassengers = 0;

            // Reset dwell
            train.IsDwelling = false;
            train.DwellTimer = 0f;

            // Reset arrival times
            train.ScheduledArrivalTime = TimeSpan.Zero;
            train.ExpectedArrivalTime = TimeSpan.Zero;

            // Recompute geometry
            _movement.UpdateGeometry(train, 0f);
        }

        public void setMovementService(ITrainMovementService movement)
        {
            _movement = movement;
        }

        public void setLinesService(IReadOnlyList<Line> lines)
        {
            _lines = lines;
        }

    }
}
