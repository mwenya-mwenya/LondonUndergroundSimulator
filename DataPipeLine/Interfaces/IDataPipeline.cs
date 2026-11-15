using LondonUndergroundSimulator.Engine.Models;

namespace DataPipeline
{
    public interface IDataPipeline
    {
        List<Line> LoadLines();
        List<Train> LoadTrains(List<Line> lines);
    }
}
