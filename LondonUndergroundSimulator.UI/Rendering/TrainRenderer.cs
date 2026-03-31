using LondonUndergroundSimulator.Colours;
using LondonUndergroundSimulator.Engine.Models;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI;
using System;
using System.Numerics;
using Windows.Foundation;

namespace LondonUndergroundSimulator.Rendering
{
    public class TrainRenderer
    {
        public TrainRenderer() { }

        public void Draw(CanvasDrawingSession ds, Train train, Matrix3x2 worldToScreen)
        {
            var line = train.Line;

            // Head carriage uses train.Position / train.Rotation
            var headPosScreen = Vector2.Transform(train.Position, worldToScreen);
            DrawCarriage(ds, headPosScreen, train.Rotation, line.ColorHex);
            DrawTrainTag(ds, headPosScreen, train.Id);

            // Other carriages: offset along the train’s forward direction
            float spacing = train.CarriageSpacing;
            var forward = new Vector2(MathF.Cos(train.Rotation), MathF.Sin(train.Rotation));

            for (int i = 0; i < train.CarriageCount; i++)
            {
                var posScreen = Vector2.Transform(train.CarriagePositions[i], worldToScreen);
                float angle = train.CarriageAngles[i];

                DrawCarriage(ds, posScreen, angle, train.Line.ColorHex);
            }
        }

        private void DrawCarriage(CanvasDrawingSession ds, Vector2 posScreen, float angle, string colorHex)
        {
            float length = 20f;
            float width = 8f;
            float radius = width / 2f;

            var color = ColorExtensions.FromHex(colorHex);

            ds.Transform = Matrix3x2.CreateRotation(angle, posScreen);

            var rect = new Rect(
                posScreen.X - length / 2,
                posScreen.Y - width / 2,
                length,
                width
            );

            ds.FillRoundedRectangle(rect, radius, radius, color);
            ds.DrawRoundedRectangle(rect, radius, radius, Colors.Black, 2);

            ds.Transform = Matrix3x2.Identity;
        }

        public void DrawTime(CanvasDrawingSession ds, Vector2 posScreen, string timeText)
        {
            var format = new CanvasTextFormat
            {
                FontSize = 20,
                FontFamily = "Segoe UI",
                HorizontalAlignment = CanvasHorizontalAlignment.Center,
                VerticalAlignment = CanvasVerticalAlignment.Center
            };

            ds.DrawText(timeText, posScreen, Colors.White, format);
        }

        private void DrawTrainTag(CanvasDrawingSession ds, Vector2 posScreen, string tag)
        {
            var format = new CanvasTextFormat
            {
                FontSize = 12,
                FontFamily = "Segoe UI",
                HorizontalAlignment = CanvasHorizontalAlignment.Center,
                VerticalAlignment = CanvasVerticalAlignment.Bottom
            };

            var tagPos = new Vector2(posScreen.X, posScreen.Y - 15);
            ds.DrawText(tag, tagPos, Colors.White, format);
        }
    }
}