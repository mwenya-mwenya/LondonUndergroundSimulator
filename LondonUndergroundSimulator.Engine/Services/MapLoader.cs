using LondonUndergroundSimulator.Engine.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text.Json;

namespace LondonUndergroundSimulator.Engine.Services
{
    public class MapLoader
    {
        public static List<Line> Load(string path)
        {
            var json = File.ReadAllText(path);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var lines = JsonSerializer.Deserialize<List<Line>>(json, options);

            foreach (var line in lines)
            {
                //
                // 1. Build CENTERLINE track directly from station coordinates
                //
                var center = new Track();
                float total = 0f;

                for (int i = 0; i < line.Stations.Count; i++)
                {
                    var s = line.Stations[i];
                    var p = new Vector2((float)s.X, (float)s.Y);

                    center.Points.Add(p);

                    if (i == 0)
                    {
                        center.StationDistances.Add(0f);
                    }
                    else
                    {
                        float seg = Vector2.Distance(center.Points[i - 1], p);
                        total += seg;
                        center.StationDistances.Add(total);
                    }
                }

                center.TotalLength = total;

                //
                // 2. EASTBOUND = offset centerline
                //
                line.Eastbound = Track.GenerateOffsetTrack(center, 0);

                //
                // 3. WESTBOUND = reverse centerline, then offset
                //
                var reversed = Track.Reverse(center);

                // Generate offset westbound track
                var west = Track.GenerateOffsetTrack(reversed, 10);


                // Copy reversed station distances into the offset track
                west.StationDistances = new List<float>(reversed.StationDistances);
                west.TotalLength = reversed.TotalLength;

                line.Westbound = west;
            }

            return lines;
        }
    }
}