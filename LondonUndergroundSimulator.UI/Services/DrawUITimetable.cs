using LondonUndergroundSimulator.Colours;
using LondonUndergroundSimulator.Engine.Models;
using LondonUndergroundSimulator.Rendering;
using LondonUndergroundSimulator.ViewModels;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace LondonUndergroundSimulator.Services
{
    public class DrawUITimetable
    {


        public void DrawTimetable(CanvasDrawingSession ds, MainViewModel ViewModel, float canvasWidth, float canvasHeight)
        {
            float margin = 20f;

            // Height of the timetable panel
            float panelHeight = 200f;

            // Bottom-left anchoring
            float xStart = margin;
            float yStart = canvasHeight - panelHeight - margin;

            float x = xStart;
            float y = yStart;

            float colStation = 150f;
            float colWaitingPassengers = 70f;
            float colTrainId = 120f;
            float colCountdown = 100f;
            float colStatus = 200f;

            var format = new CanvasTextFormat
            {
                FontSize = 14,
                FontFamily = "Segoe UI",
                HorizontalAlignment = CanvasHorizontalAlignment.Left,
                VerticalAlignment = CanvasVerticalAlignment.Top
            };

            ViewModel.TimetableEntries = new List<TimetableEntry>(ViewModel.Timetable.BuildTimetable());

            // HEADER ROW
            float headerY = y - 20;

            float totalWidth =
                colStation +
                colWaitingPassengers +
                colTrainId +
                colCountdown +
                colStatus;

            ds.FillRectangle(xStart, headerY - 2, totalWidth, 20, Colors.White);

            x = xStart;
            ds.DrawText("Next Station", x, headerY, Colors.Black, format);
            x += colStation;

            ds.DrawText("Waiting", x, headerY, Colors.Black, format);
            x += colWaitingPassengers;

            ds.DrawText("Train", x, headerY, Colors.Black, format);
            x += colTrainId;

            ds.DrawText("Due", x, headerY, Colors.Black, format);
            x += colCountdown;

            ds.DrawText("Status", x, headerY, Colors.Black, format);

            // LINE STATUS SECTION (above timetable)
            float statusY = yStart - 250;

            ds.DrawText("Line Status", xStart, statusY, Colors.White, format);
            ds.DrawText("Service", xStart + 100, statusY, Colors.White, format);

            var lineGroups = ViewModel.TimetableEntries.GroupBy(e => e.LineName);

            foreach (var lineGroup in lineGroups)
            {
                var lineName = lineGroup.Key;
                var lineEntries = lineGroup.ToList();

                bool isDelayed = lineEntries.Any(e => e.IsDelayed);
                bool hasService = lineEntries.Any(e => e.ServiceStatus);

                string statusText = isDelayed ? "Delayed" : !hasService ? "Out of Hours" : "Good Service";
                Color statusColor = isDelayed ? Colors.Orange : hasService ? Colors.Green : Colors.Gray;

                statusY += 20;

                ds.DrawText(lineName, xStart, statusY, Colors.White, format);
                ds.FillRectangle(xStart + 100, statusY, 100, 20, statusColor);
                ds.DrawText(statusText, xStart + 100, statusY, Colors.Black, format);
            }

            // TIMETABLE ROWS
            foreach (var entry in ViewModel.TimetableEntries)
            {
                if (!entry.ServiceStatus || !entry.IsDelayed)
                {
                   
                    continue;
                }

                    x = xStart;

                    ds.FillRectangle(x, y - 2, totalWidth, 20, Colors.Black);

                    var lineColor = !string.IsNullOrWhiteSpace(entry.Color)
                        ? ColorExtensions.FromHex(entry.Color)
                        : Colors.White;

                    ds.DrawText(entry.CurrentStation, x, y, lineColor, format);
                    x += colStation;

                    ds.FillRectangle(x, y, colWaitingPassengers, 20, Colors.White);
                    ds.DrawText(entry.CurrentStationWaitingPassengers.ToString(), x, y, Colors.Black, format);
                    x += colWaitingPassengers;

                    ds.DrawText(entry.TrainId.ToString(), x, y, lineColor, format);
                    x += colTrainId;

                    ds.DrawText(entry.GetCountdown(ViewModel.Simulation.Clock), x, y, lineColor, format);
                    x += colCountdown;

                    string statusText = entry.GetStatus().Text;
                    string textBackgroundColor = entry.GetStatus().Color;

                    ds.FillRectangle(x, y, colStatus, 20, ColorExtensions.FromHex(textBackgroundColor));
                    ds.DrawText(statusText, x, y, Colors.Black, format);

                    y += 20;

            }
        }

    }
}
