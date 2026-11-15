using DataPipeLine.Interfaces;
using DataPipeLine.Models;

namespace DataPipeLine.Aggregators
{
    public class StationAggregator : IAggregator<StationFeatures>
    {
        public IEnumerable<StationFeatures> Aggregate(IEnumerable<LogRecord> records)
        {
            var result = records
                .GroupBy(r => r.Station)
                .Select(g => new StationFeatures
                {
                    Station = g.Key,
                    AvgWaiting = g.Average(r => r.WaitingPassengers),
                    MaxWaiting = g.Max(r => r.WaitingPassengers),
                    BusyPercentage = g.Count(r => r.WaitingPassengers > 100)
                                     / (double)g.Count()
                }).ToList();

            return result;
        }
    }
}
