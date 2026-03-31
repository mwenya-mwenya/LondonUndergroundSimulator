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

        public TrainMovementService(SimulationEngine engine)
        {
            _engine = engine;
        }



        // ---------------------------------------------------------
        // MOVEMENT ONLY
        // ---------------------------------------------------------
        public void UpdateMovement(Train train, float dt)
        {
            var track = train.Track;
            var line = train.Line;
            int lastIndex = line.Stations.Count - 1;

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

            float travelSeconds = segmentLength / train.Speed;
            float progressPerSecond = 1f / travelSeconds;

            train.Progress += progressPerSecond * dt;

            // -----------------------------
            // DIRECTION-AGNOSTIC PROGRESS p
            // -----------------------------
            float p = train.Progress;
            if (train.Direction == -1)
                p = 1f - p;

            // -----------------------------
            // LOOPING AT ENDS
            // -----------------------------
            if (train.Direction == 1 &&
                train.CurrentStationIndex == lastIndex &&
                p >= 1f - EPSILON)
            {
                train.CurrentStationIndex = 0;
                p = 0f;
            }
            else if (train.Direction == -1 &&
                     train.CurrentStationIndex == 0 &&
                     p <= EPSILON)
            {
                train.CurrentStationIndex = lastIndex;
                p = 0f;
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
            }

            train.CurrentStationIndex = Math.Clamp(train.CurrentStationIndex, 0, lastIndex);

            // -----------------------------
            // WRITE BACK PROGRESS
            // -----------------------------
            train.Progress = (train.Direction == 1) ? p : 1f - p;

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

            // -----------------------------
            // ROTATION (original engine method)
            // -----------------------------
            float d1 = headDist + train.Direction * 20f;
            float d2 = headDist + train.Direction * 40f;

            if (d1 < 0) d1 += track.TotalLength;
            if (d2 < 0) d2 += track.TotalLength;
            if (d1 > track.TotalLength) d1 -= track.TotalLength;
            if (d2 > track.TotalLength) d2 -= track.TotalLength;

            var p1 = track.GetPointAt(d1 / track.TotalLength);
            var p2 = track.GetPointAt(d2 / track.TotalLength);

            var forward = p2 - p1;
            float targetAngle = MathF.Atan2(forward.Y, forward.X);

            train.Rotation = LerpAngle(train.Rotation, targetAngle, 8f * dt);

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

        private float LerpAngle(float a, float b, float t)
        {
            float diff = MathF.Atan2(MathF.Sin(b - a), MathF.Cos(b - a));
            return a + diff * t;
        }

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