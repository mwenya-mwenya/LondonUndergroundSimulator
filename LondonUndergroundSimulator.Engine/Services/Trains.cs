using LondonUndergroundSimulator.Engine.Models;
using LondonUndergroundSimulator.Engine.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;

public static class Trains
{
    public static List<Train> GetTrains()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "lines.json");
        var lines = MapLoader.Load(path);

        var trains = new List<Train>();

        foreach (var line in lines)
        {
            int lastStationIndex = line.Stations.Count - 1;

            for (int i = 0; i < line.NumberOfTrains; i++)
            {
                float startDist = GetStartDistance(line, i, line.NumberOfTrains);
                float t = startDist / line.Eastbound.TotalLength;
                float exactIndex = t * (line.Stations.Count - 1);
                int stationIndex = (int)MathF.Floor(exactIndex);
                float progress = exactIndex - stationIndex;

                int westIndex = stationIndex;
                float westProgress = progress;

                // EASTBOUND
                trains.Add(CreateTrain(
                    id: $"{line.Name}_East_{i + 1}",
                    line: line,
                    track: line.Eastbound,
                    stationIndex: stationIndex,
                    direction: 1,
                    progress: progress
                ));

                // WESTBOUND
                trains.Add(CreateTrain(
                    id: $"{line.Name}_West_{i + 1}",
                    line: line,
                    track: line.Westbound,
                    stationIndex: westIndex,
                    direction: 1,
                    progress: westProgress
                ));
            }
        }

        return trains;
    }

    private static Train CreateTrain(string id, Line line, Track track, int stationIndex, int direction, float progress)
    {
        return new Train
        {
            Id = id,
            Line = line,
            Track = track,
            CurrentStationIndex = stationIndex,
            Direction = direction,
            Progress = progress,
            Speed = 5f,

            // Geometry
            Position = Vector2.Zero,
            Rotation = 0f,

            // Carriages
            CarriageCount = 3,
            CarriageAngles = new float[3],

            // Delay system
            IsDelayed = false,
            DelayDuration = 0f,
            DelayTimer = 0f,
            DelayStartTime = TimeSpan.Zero,
            DelayReasons = new List<string>(),
            LastDelayEndTime = TimeSpan.Zero,
            DelayCooldown = TimeSpan.FromSeconds(30),

            // Dwell system
            IsDwelling = false,
            DwellTimer = 0f,

            CarriagePositions = new Vector2[3],
           
        };
    }

    private static float GetStartDistance(Line line, int trainIndex, int totalTrains)
    {
        float spacing = line.Eastbound.TotalLength / totalTrains;
        return trainIndex * spacing;
    }

}