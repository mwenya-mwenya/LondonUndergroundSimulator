using DataPipeLine;
using DataPipelineRunner.Interfaces;
using LondonUndergroundSimulator.App.Interfaces;
using LondonUndergroundSimulator.Engine.Models;


namespace DataPipelineRunner
{
    public class DataPipelineRunner : IDataPipelineRunner
    {
        private readonly IDataPipeline _pipeline;

        public DataPipelineRunner(IDataPipeline pipeline)
        {
            _pipeline = pipeline;
        }

        public List<Line> LoadLines()
        {
            return _pipeline.LoadLines();
        }

        public List<Train> LoadTrains(List<Line> lines)
        {
            return _pipeline.LoadTrains(lines);
        }
    }


}
