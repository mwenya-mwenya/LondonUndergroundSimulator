using LondonUndergroundSimulator.Engine.Interfaces;
using LondonUndergroundSimulator.Engine.Models;
using System;
using System.Diagnostics;
using System.Numerics;

namespace LondonUndergroundSimulator.Engine.Services
{
    public class TrainMovementService : ITrainMovementService
    {
        private const float EPSILON = 0.0001f;

        private readonly SimulationEngine _engine;

        private readonly ITerminalLoopService _terminalLoopService;

        public TrainMovementService(SimulationEngine engine, ITerminalLoopService terminalLoopService)
        {
            _engine = engine;
            _terminalLoopService = terminalLoopService;
        }



        // ---------------------------------------------------------
        // MOVEMENT ONLY
        // ---------------------------------------------------------
        public void UpdateMovement(Train train, float dt)
        {

           
            var track = train.Track;
            var line = train.Line;
            int lastIndex = line.Stations.Count - 1;
            var checkWaitingStationPassengers = line.Stations.Aggregate(false, (acc, station) => acc || station.WaitingPassengers > 0);
            //if (train.Id == "Central_4")
            //    Debug.WriteLine($"Movement Update - before active check: Train {train.Id} is moving on line {train.Line.Name}, current station index: {train.CurrentStationIndex}, direction: {train.Direction}, progress: {train.Progress:F2}, dwell timer: {train.DwellTimer:F2}, waiting passengers: {checkWaitingStationPassengers}");
            if (train.Line.IsServiceActive != true)
                return;
            //if (train.Id == "Central_4")
            //    Debug.WriteLine($"Movement Update: Train {train.Id} is moving on line {train.Line.Name}, current station index: {train.CurrentStationIndex}, direction: {train.Direction}, progress: {train.Progress:F2}, dwell timer: {train.DwellTimer:F2}, waiting passengers: {checkWaitingStationPassengers}");
            // -----------------------------
            // DWELL HANDLING
            // -----------------------------
            if (train.IsDwelling)
            {
                train.DwellTimer -= dt;
                if (train.DwellTimer <= 0f)
                {
                    train.IsDwelling = false;
                    train.DwellTimer = 0f;
                }
                else
                {
                    return; // do not move while dwelling
                }
            }

            // -----------------------------
            // ADVANCE PROGRESS ALONG SEGMENT
            // -----------------------------
            int nextIndexActual = Math.Clamp(train.CurrentStationIndex + train.Direction, 0, lastIndex);

            float startDist = track.StationDistances[train.CurrentStationIndex];
            float endDist = track.StationDistances[nextIndexActual];
            float segmentLength = MathF.Abs(endDist - startDist);

            //// 🚨 Guard against bad geometry or speed
            //if (segmentLength < 0.5f || train.Speed <= 0f)
            //{
            //    // Don’t advance progress on a degenerate segment
            //    // You can log this to find bad data:
            //    Debug.WriteLine($"[Movement] Degenerate segment for {train.Id}: " +
            //                    $"startDist={startDist}, endDist={endDist}, segmentLength={segmentLength}");
            //    return;
            //}

            bool degenerateSegment = segmentLength < 0.5f || train.Speed <= 0f;

            // Compute p BEFORE deciding what to do
            float p = train.Progress;
            if (train.Direction == -1)
                p = 1f - p;

            // If at terminal, let loop logic handle it
            bool atTerminal =
                (train.Direction == 1 && train.CurrentStationIndex == lastIndex) ||
                (train.Direction == -1 && train.CurrentStationIndex == 0);

            if (degenerateSegment)
            {
                if (atTerminal)
                {
                    p = (train.Direction == 1) ? 1f : 0f;

                    if (train.Direction == 1 && p >= 1f - EPSILON)
                    {
                        _terminalLoopService.HandleLoop(train);
                        return;
                    }
                    if (train.Direction == -1 && p <= EPSILON)
                    {
                        _terminalLoopService.HandleLoop(train);
                        return;
                    }
                }

                return;
            }



            float travelSeconds = segmentLength / train.Speed;
            float progressPerSecond = 1f / travelSeconds;

            train.Progress += progressPerSecond * dt;

            // -----------------------------
            // DIRECTION-AGNOSTIC PROGRESS p
            // -----------------------------
            p = train.Progress;
            if (train.Direction == -1)
                p = 1f - p;

            // -----------------------------
            // LOOPING AT ENDS
            // -----------------------------
            if (train.Direction == 1 &&
                train.CurrentStationIndex == lastIndex &&
                p >= 1f - EPSILON)
            {
                //train.CurrentStationIndex = 0;
                //p = 0f;
       

                _terminalLoopService.HandleLoop(train);
                return;
            }
            else if (train.Direction == -1 &&
                     train.CurrentStationIndex == 0 &&
                     p <= EPSILON)
            {

                _terminalLoopService.HandleLoop(train);
                return;
            }

            // -----------------------------
            // SEGMENT COMPLETION
            // -----------------------------
            if (p >= 1f - EPSILON)
            {
                p = 0f;
                train.CurrentStationIndex += train.Direction;

                train.IsDwelling = true;
                train.DwellTimer = train.DwellDuration;
                train.HasProcessedDwell = false;


                // Compute next segment arrival time
                int nextIndex = Math.Clamp(train.CurrentStationIndex + train.Direction, 0, lastIndex);

                float nextStart = track.StationDistances[train.CurrentStationIndex];
                float nextEnd = track.StationDistances[nextIndex];
                float nextSegmentLength = MathF.Abs(nextEnd - nextStart);

                travelSeconds = nextSegmentLength / train.Speed;

                train.ScheduledArrivalTime = _engine.Clock + TimeSpan.FromSeconds(travelSeconds);
                train.ExpectedArrivalTime =
                    train.ScheduledArrivalTime + TimeSpan.FromSeconds(train.DelayDuration);             
            }
            else if (p <= EPSILON)
            {
                p = 1f;
                train.CurrentStationIndex += train.Direction;

                train.IsDwelling = true;
                train.DwellTimer = train.DwellDuration;
                train.HasProcessedDwell = false;
            }

            train.CurrentStationIndex = Math.Clamp(train.CurrentStationIndex, 0, lastIndex);

            // -----------------------------
            // WRITE BACK PROGRESS
            // -----------------------------
            train.Progress = (train.Direction == 1) ? p : 1f - p;

            //Check if Central_4 train is moving and log its state
            //if (train.Id == "Central_4" || train.Id == "Central_1")
            //{
            //    Debug.WriteLine($"Movement Update - after update: Train {train.Id} is moving on line {train.Line.Name}, current station index: {train.CurrentStationIndex}, direction: {train.Direction}, progress: {train.Progress:F2}, dwell timer: {train.DwellTimer:F2}, waiting passengers: {checkWaitingStationPassengers}");
            //}

        }

        // ---------------------------------------------------------
        // GEOMETRY ONLY
        // ---------------------------------------------------------
        public void UpdateGeometry(Train train, float dt)
        {
            var track = train.Track;
            var line = train.Line;
            int lastIndex = line.Stations.Count - 1;

            // Geometry uses the SAME nextIndex logic as movement
            int nextIndex = Math.Clamp(train.CurrentStationIndex + train.Direction, 0, lastIndex); 

            float startDist = track.StationDistances[train.CurrentStationIndex];
            float endDist = track.StationDistances[nextIndex];

            float p = train.Progress;
            if (train.Direction == -1)
                p = 1f - p;

            float headDist = Lerp(startDist, endDist, p);
            float t = headDist / track.TotalLength;

            train.Position = track.GetPointAt(t);
            //compare position with t to make sure they are consistent
            //if (train.Id == "Central_4")
            //    Debug.WriteLine($"Geometry Update: Train {train.Id} has head position {train.Position}, t={t:F2}, headDist={headDist:F2}, startDist={startDist:F2}, endDist={endDist:F2}, progress={train.Progress:F2}");


            float tNorm = headDist / track.TotalLength;
            Vector2 tangent = track.GetTangentAt(tNorm);
            train.Rotation = MathF.Atan2(tangent.Y, tangent.X);
            //if (train.Id == "Central_4")
            //    Debug.WriteLine($"Geometry Update: Train {train.Id} has head position {train.Position}, rotation {train.Rotation:F2}, and carriage positions: {string.Join(", ", train.CarriagePositions)}");

            // -----------------------------
            // CARRIAGES
            // -----------------------------
            train.CarriagePositions[0] = train.Position;
            train.CarriageAngles[0] = train.Rotation;

            float spacing = train.CarriageSpacing;

            for (int i = 1; i < train.CarriageCount; i++)
            {
                float distBehind = spacing * i;
                float carriageDist = headDist - train.Direction * distBehind;

                if (carriageDist < 0) carriageDist += track.TotalLength;
                if (carriageDist > track.TotalLength) carriageDist -= track.TotalLength;

                float tCar = carriageDist / track.TotalLength;
                Vector2 pos = track.GetPointAt(tCar);

                float aheadDistCar = carriageDist + train.Direction * 5f;

                if (aheadDistCar < 0) aheadDistCar += track.TotalLength;
                if (aheadDistCar > track.TotalLength) aheadDistCar -= track.TotalLength;

                float aheadTCar = aheadDistCar / track.TotalLength;
                Vector2 aheadCar = track.GetPointAt(aheadTCar);

                float angle = MathF.Atan2(aheadCar.Y - pos.Y, aheadCar.X - pos.X);

                train.CarriagePositions[i] = pos;
                train.CarriageAngles[i] = angle;
            }
          

        }

        private float Lerp(float a, float b, float t)
            => a + (b - a) * t;

        public void InitialiseTrain(Train train)
        {

            var track = train.Track;
            var line = train.Line;
            int lastIndex = line.Stations.Count - 1;

            int nextIndex = Math.Clamp(train.CurrentStationIndex + train.Direction, 0, lastIndex);

            float start = track.StationDistances[train.CurrentStationIndex];
            float end = track.StationDistances[nextIndex];
            float segmentLength = MathF.Abs(end - start);

            float travelSeconds = segmentLength / train.Speed;

            train.ScheduledArrivalTime = _engine.Clock + TimeSpan.FromSeconds(travelSeconds);
            train.ExpectedArrivalTime =
                train.ScheduledArrivalTime + TimeSpan.FromSeconds(train.DelayDuration);
        }

        
    }
}