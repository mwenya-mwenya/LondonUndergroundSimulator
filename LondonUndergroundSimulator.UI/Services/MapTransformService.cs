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
        float scaleXDivider = 1000f;
        float scaleYDivider = 1000f;

        float scaleX = canvasWidth / scaleXDivider;
        float scaleY = canvasHeight / scaleYDivider;

        Scale = Math.Max(scaleX, scaleY);

        float scaledWidth = scaleXDivider * Scale;
        float scaledHeight = scaleYDivider * Scale;

        OffsetX = (canvasWidth - scaledWidth) / 2f;
        OffsetY = (canvasHeight - scaledHeight) / 2f;

        MinX = vm.Map.Lines.SelectMany(l => l.Stations).Min(s => (float)s.X);
        MinY = vm.Map.Lines.SelectMany(l => l.Stations).Min(s => (float)s.Y);

        MapShiftX = 200;
        MapShiftY = 200;

        Transform =
            Matrix3x2.CreateTranslation(-MinX, -MinY) *
            Matrix3x2.CreateTranslation(MapShiftX, MapShiftY) *
            Matrix3x2.CreateScale(Scale) *
            Matrix3x2.CreateTranslation(OffsetX, OffsetY);
    }
}