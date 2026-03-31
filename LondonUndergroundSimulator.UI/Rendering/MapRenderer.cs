using LondonUndergroundSimulator.Colours;
using LondonUndergroundSimulator.Engine.Models;
using LondonUndergroundSimulator.ViewModels;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI;
using System.Numerics;
using Windows.Foundation;

namespace LondonUndergroundSimulator.Rendering
{
    public class MapRenderer
    {
        public void Draw(CanvasDrawingSession ds, MainViewModel vm, MapTransformService t)
        {
            var mouseScreen = new Vector2((float)vm.MouseX, (float)vm.MouseY);
            var mouseWorld = Vector2.Transform(mouseScreen,
                Matrix3x2.Invert(t.Transform, out var inv) ? inv : Matrix3x2.Identity);

            vm.HoveredStation = null;

            foreach (var line in vm.Map.Lines)
            {
                var colorHex = line.ColorHex;

                //
                // DRAW BOTH TRACKS
                //
                DrawTrack(ds, line.Eastbound, colorHex, t.Transform);
                DrawTrack(ds, line.Westbound, colorHex, t.Transform);

                //
                // DRAW STATIONS + HOVER DETECTION
                //
                foreach (var station in line.Stations)
                {
                    var stationScreen = Vector2.Transform(
                        new Vector2((float)station.X, (float)station.Y),
                        t.Transform);

                    float dx = (float)vm.MouseX - stationScreen.X;
                    float dy = (float)vm.MouseY - stationScreen.Y;

                    bool isHover = dx * dx + dy * dy < 8f * 8f;
                    if (isHover)
                        vm.HoveredStation = station;

                    ds.FillCircle(stationScreen, 5, Colors.White);
                    ds.DrawCircle(stationScreen, 5, Colors.Black, 2);
                }
            }

            //
            // DRAW LABEL LAST
            //
            if (vm.HoveredStation != null)
                DrawHoverLabel(ds, vm.HoveredStation, t);
        }

        private void DrawTrack(CanvasDrawingSession ds, Track track, string hex, Matrix3x2 transform)
        {
            if (track?.Points == null || track.Points.Count < 2)
                return;

            var color = ColorExtensions.FromHex(hex);

            for (int i = 0; i < track.Points.Count - 1; i++)
            {
                var p1 = Vector2.Transform(track.Points[i], transform);
                var p2 = Vector2.Transform(track.Points[i + 1], transform);

                ds.DrawLine(p1, p2, color, 3);
            }
        }

        private void DrawHoverLabel(CanvasDrawingSession ds, Station station, MapTransformService t)
        {
            var format = new CanvasTextFormat
            {
                FontSize = 14,
                FontFamily = "Segoe UI Semibold"
            };

            var world = new Vector2((float)station.X, (float)station.Y);
            var screen = Vector2.Transform(world, t.Transform);

            float offset = 12 * t.Scale;
            float x = screen.X + offset;
            float y = screen.Y - offset;

            var textLayout = new CanvasTextLayout(ds, station.Name, format, 200, 50);

            var rect = new Rect(
                x - 4,
                y - 4,
                textLayout.LayoutBounds.Width + 8,
                textLayout.LayoutBounds.Height + 8
            );

            ds.FillRoundedRectangle(rect, 4, 4, Colors.White);
            ds.DrawRoundedRectangle(rect, 4, 4, Colors.Black);

            ds.DrawTextLayout(textLayout, x, y, Colors.Black);
        }
    }
}