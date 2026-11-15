using DataPipeLine.Models;
using DataPipeLine.Interfaces;

namespace DataPipeLine.Aggregators
{
    public class TrainAggregator: IAggregator<TrainFeatures>
    {
       
        public IEnumerable<TrainFeatures> Aggregate(IEnumerable<LogRecord> records)
        {
            var result = records
                .GroupBy(r => r.TrainId)
                .Select(g => new TrainFeatures
                {
                    TrainId = g.Key,
                    AvgLoadFactor = g.Average(r => (double)r.CurrentPassengers / r.MaxCapacity),
                    DelayRate = g.Count(r => r.IsDelayed) / (double)g.Count(),
                    AvgDwell = g.Count(r => r.IsDwelling)
                }).ToList();

            return result;


        }
    }

}
