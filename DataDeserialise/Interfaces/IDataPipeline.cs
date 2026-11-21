using LondonUndergroundSimulator.Engine.Models;

namespace DataDeserialise.Interfaces
{
    public interface IDataPipeline
    {
        List<Line> LoadLines();
        List<Train> LoadTrains(List<Line> lines);
    }
}
