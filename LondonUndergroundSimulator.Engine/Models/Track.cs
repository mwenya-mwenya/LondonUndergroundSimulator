using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LondonUndergroundSimulator.Engine.Models
{
    public class Track
    {
        public List<Vector2> Points { get; set; } = new();
        public List<float> StationDistances { get; set; } = new();
        public float TotalLength { get; set; }

        public Vector2 GetPointAt(float t)
        {
            if (Points.Count < 2)
                return Vector2.Zero;

            float targetDist = t * TotalLength;

            for (int i = 0; i < StationDistances.Count - 1; i++)
            {
                float a = StationDistances[i];
                float b = StationDistances[i + 1];

                if (targetDist >= a && targetDist <= b)
                {
                    float segmentT = (targetDist - a) / (b - a);
                    return Vector2.Lerp(Points[i], Points[i + 1], segmentT);
                }
            }

            return Points[^1];
        }

        public static Track GenerateOffsetTrack(Track original, float offset)
        {
            Track t = new();

            for (int i = 0; i < original.Points.Count; i++)
            {
                Vector2 p = original.Points[i];

                Vector2 forward;
                if (i == original.Points.Count - 1)
                    forward = Vector2.Normalize(p - original.Points[i - 1]);
                else
                    forward = Vector2.Normalize(original.Points[i + 1] - p);

                Vector2 perp = new(-forward.Y, forward.X); // left-hand perpendicular
                Vector2 offsetPoint = p + perp * offset;

                t.Points.Add(offsetPoint);
            }

            // Recompute distances
            float total = 0;
            t.StationDistances.Add(0);

            for (int i = 1; i < t.Points.Count; i++)
            {
                total += Vector2.Distance(t.Points[i - 1], t.Points[i]);
                t.StationDistances.Add(total);
            }

            t.TotalLength = total;
            return t;
        }

        public static Track Reverse(Track original)
        {
            var reversed = new Track();

            // 1. Reverse geometry
            reversed.Points = new List<Vector2>(original.Points);
            reversed.Points.Reverse();

            // 2. Reverse station distances properly
            float total = original.TotalLength;

            reversed.StationDistances = original.StationDistances
                .Select(d => total - d)   // invert distance
                .Reverse()                // reverse order
                .ToList();

            reversed.TotalLength = total;

            return reversed;
        }


    }
}
