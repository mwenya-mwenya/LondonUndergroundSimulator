using LondonUndergroundSimulator.Engine.Interfaces;
using LondonUndergroundSimulator.Engine.Models;
using LondonUndergroundSimulator.Engine.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;

public static class Trains 
{
    public static List<Train> GetTrains(List<Line> lines)
    {
       
        var trains = new List<Train>();

        int trainNumber = 1;

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
                    //id: $"{line.Name}_East_{i + 1}",
                    id: $"{line.Name}_{trainNumber++}",
                    line: line,
                    track: line.Eastbound,
                    stationIndex: stationIndex,
                    direction: 1,
                    progress: progress
                ));

                // WESTBOUND
                trains.Add(CreateTrain(
                    //id: $"{line.Name}_West_{i + 1}",
                    id: $"{line.Name}_{trainNumber++}",
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

    private static Train CreateTrain(string id, Line line, Track track, int stationIndex, int direction, float progress, int carriageCount = 3)
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
            CarriageCount = carriageCount,
            CarriageAngles = new float[carriageCount],

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

            MaxCapacity = carriageCount * 100, // Assuming each carriage can hold 100 passengers
            CurrentPassengers = 0,

        };
    }

    private static float GetStartDistance(Line line, int trainIndex, int totalTrains)
    {
        float spacing = line.Eastbound.TotalLength / totalTrains;
        return trainIndex * spacing;
    }

    public static List<Train> CreateAdditionalTrains(Line line, int additionalCount)
    {
        var newTrains = new List<Train>();

        int existing = line.NumberOfTrains;
        int total = existing + additionalCount;
        int newTrainNumber = existing + 1;

        for (int i = existing; i < total; i++)
        {
            float startDist = GetStartDistance(line, i, total);
            float tEast = startDist / line.Eastbound.TotalLength;
            float tWest = startDist / line.Westbound.TotalLength;

            float exactIndexEast = tEast * (line.Stations.Count - 1);
            float exactIndexWest = tWest * (line.Stations.Count - 1);

            int stationIndexEast = (int)MathF.Floor(exactIndexEast);
            int stationIndexWest = (int)MathF.Floor(exactIndexWest);

            float progressEast = exactIndexEast - stationIndexEast;
            float progressWest = exactIndexWest - stationIndexWest;

            // For naming purposes, trains start from 1

            // EASTBOUND
            newTrains.Add(CreateTrain(
                //id: $"{line.Name}_East_{i + 1}",
                id: $"{line.Name}_{newTrainNumber++}",
                line: line,
                track: line.Eastbound,
                stationIndex: stationIndexEast,
                direction: 1,
                progress: progressEast
            ));

            // WESTBOUND
            newTrains.Add(CreateTrain(
                //id: $"{line.Name}_West_{i + 1}",
                id: $"{line.Name}_{newTrainNumber++}",
                line: line,
                track: line.Westbound,
                stationIndex: stationIndexWest,
                direction: 1,
                progress: progressWest
            ));
        }

        return newTrains;
    }

    public static Train CreateSingleTrain(Line line, Track track)
    {
        int trainNum = line.NextTrainId++; // Increment a persistent counter on Line

        return CreateTrain(
            id: $"{line.Name}_{trainNum}",
            line: line,
            track: track,
            stationIndex: 0,
            direction: 1,
            progress: 0f
        );
    }



    //public static List<Train> RemoveLastTrains(List<Train> trains, int count)
    //{
    //    var removed = new List<Train>();

    //    for (int i = 0; i < count; i++)
    //    {
    //        if (trains.Count == 0)
    //            break;

    //        var t = trains[^1];
    //        removed.Add(t);
    //        trains.RemoveAt(trains.Count - 1);


    //    }

    //    return removed;
    //}




}