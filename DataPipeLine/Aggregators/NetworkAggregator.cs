using DataPipeLine.Interfaces;
using DataPipeLine.Models;


namespace DataPipeLine.Aggregators
{
    public class NetworkAggregator : IAggregator<NetworkFeatures>
    {
        public IEnumerable<NetworkFeatures> Aggregate(IEnumerable<LogRecord> records)
        {
           
            var result = new[]
            {
                new NetworkFeatures
                {
                    AvgNetworkLoad = records.Average(r => (double)r.CurrentPassengers / r.MaxCapacity),
                    TotalPassengersMoved = records.Sum(r => r.CurrentPassengers),
                    OvercrowdedStations = records.Count(r => r.WaitingPassengers > 200)
                }
            };


            return result;

        }
    }

}
