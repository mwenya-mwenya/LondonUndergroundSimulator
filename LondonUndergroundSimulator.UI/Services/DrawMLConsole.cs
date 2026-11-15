using LondonUndergroundSimulator.ViewModels;
using LondonUndergroundSimulator.Training.Services;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LondonUndergroundSimulator.Services
{
    public class CreateMLConsole
    {
        //private readonly SimulationLogStats _logStats;

        //public int SimulationLogRows { get; set; }

        //public string SimulationLogSize { get; set; } = "";

        //public CreateMLConsole(SimulationLogStats logStats)
        //{
        //    _logStats = logStats;
        //    SimulationLogRows = _logStats.GetRowCount();
        //    SimulationLogSize = _logStats.GetFileSize();
        //}
        

        public void DrawMLConsole(CanvasDrawingSession ds, MainViewModel ViewModel, float canvasWidth, float canvasHeight)
        {
            if (ViewModel.MLConsole == null)
                return;

            var ml = ViewModel.MLConsole;

            float panelWidth = 300f;     // width of the ML console
            float margin = 20f;          // margin from edges

            float x = canvasWidth - panelWidth - margin;
            float y = margin;

            var format = new CanvasTextFormat
            {
                FontSize = 14,
                FontFamily = "Segoe UI"
            };

            ds.DrawText("Machine Learning Console", x, y, Colors.Yellow, format);
            y += 30;

            bool MlConnected = ViewModel.IsMlConnected;
            var mlStatusColor = MlConnected ? Colors.LightGreen : Colors.Red;

            string statusText = MlConnected ? "Connected" : "Disconnected";

            int statusTextWidth = statusText.Length;

            ds.DrawText($"Connection Status: {statusText}", x, y, Colors.White, format);
            ds.FillCircle(x + 16 * 8 + statusTextWidth * 8, y + 10, 5, mlStatusColor);
            y += 30;
            ds.DrawText($"Date Model Last Updated: {ml.LastTimeModelUpdated}", x, y, Colors.White, format);
            y += 30;
            // log stats header
            ds.DrawText("Simulation Log Stats:", x, y, Colors.Yellow, format);
            y += 30; 
            //draw log stats
            ds.DrawText($"Simulation Log Rows: {ml.SimulationLogRows}", x, y, Colors.White, format);
            y += 30;
            ds.DrawText($"Simulation Log Size: {ml.SimulationLogSizeDisplay}", x, y, Colors.White, format);
            y += 30;
            ds.DrawText("Passenger Stats:", x, y, Colors.Yellow, format);
            y += 30;

            if (ml.LatestSnapshot != null)
            {
                ds.DrawText($"Avg Waiting: {ml.LatestSnapshot.AvgWaiting:0.0}", x, y, Colors.White, format);
                y += 20;

                ds.DrawText($"Max Waiting: {ml.LatestSnapshot.MaxWaiting}", x, y, Colors.White, format);
                y += 20;

                ds.DrawText($"Avg Load Factor: {ml.LatestSnapshot.AvgLoadFactor:0.00}", x, y, Colors.White, format);
                y += 20;

                ds.DrawText($"Train Count: {ml.LatestSnapshot.CurrentTrainCount}", x, y, Colors.White, format);
                y += 20;

                ds.DrawText($"Time: {ml.LatestSnapshot.Hour:D2}:{ml.LatestSnapshot.Minute:D2}", x, y, Colors.White, format);
                y += 30;
            }

            ds.DrawText($"Recommended Frequency: {ml.LatestRecommendation}", x, y, Colors.Orange, format);
            y += 20;

            ds.DrawText($"Last Applied Frequency: {ml.LastAppliedFrequency}", x, y, Colors.LightGreen, format);
            y += 40;

            ds.DrawText("ML Decision Log:", x, y, Colors.Yellow, format);
            y += 20;

            foreach (var entry in ml.Log.TakeLast(5))
            {
                ds.DrawText($"{entry.Time:HH:mm:ss} — {entry.Message}", x, y, Colors.White, format);
                y += 20;
            }
        }


    }
}
