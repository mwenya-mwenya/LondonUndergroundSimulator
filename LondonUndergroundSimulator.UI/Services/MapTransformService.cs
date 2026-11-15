using LondonUndergroundSimulator.ViewModels;
using System;
using System.Linq;
using System.Numerics;

public class MapTransformService
{
    public float MinX { get; private set; }
    public float MinY { get; private set; }
    public float Scale { get; private set; }
    public float OffsetX { get; private set; }
    public float OffsetY { get; private set; }
    public float MapShiftX { get; private set; }
    public float MapShiftY { get; private set; }

    public Matrix3x2 Transform { get; private set; }

    public void Compute(MainViewModel vm, float canvasWidth, float canvasHeight)
    {
        var stations = vm.Map.Lines.SelectMany(l => l.Stations);

        float minX = stations.Min(s => (float)s.X);
        float maxX = stations.Max(s => (float)s.X);
        float minY = stations.Min(s => (float)s.Y);
        float maxY = stations.Max(s => (float)s.Y);

        // Add padding so lines/labels don't overflow
        float padding = 50f;

        minX -= padding;
        minY -= padding;
        maxX += padding;
        maxY += padding;

        float mapWidth = maxX - minX;
        float mapHeight = maxY - minY;

        // Fit map inside canvas
        float scaleX = canvasWidth / mapWidth;
        float scaleY = canvasHeight / mapHeight;

        Scale = Math.Min(scaleX, scaleY);

        float scaledWidth = mapWidth * Scale;
        float scaledHeight = mapHeight * Scale;

        // Center the map
        OffsetX = (canvasWidth - scaledWidth) / 2f;
        OffsetY = (canvasHeight - scaledHeight) / 2f;

        Transform =
            Matrix3x2.CreateTranslation(-minX, -minY) *
            Matrix3x2.CreateScale(Scale) *
            Matrix3x2.CreateTranslation(OffsetX, OffsetY);
    }


}