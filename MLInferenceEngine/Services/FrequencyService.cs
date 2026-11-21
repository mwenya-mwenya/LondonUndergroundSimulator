using Grpc.Net.Client;
using Inference; // generated proto namespace
using MLInferenceEngine.Abstractions.Models;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MLInferenceEngine.Services
{
    public class FrequencyInferenceClient
    {
        private readonly FrequencyPredictor.FrequencyPredictorClient _client;

        public FrequencyInferenceClient(string address = "http://localhost:50051")
        {
            var channel = GrpcChannel.ForAddress(address);
            _client = new FrequencyPredictor.FrequencyPredictorClient(channel);
        }

        public async Task<double> PredictAsync(string LineName, SnapshotRequest snapshot)
        {
 
            var request = new SnapshotRequest
            {
                LineName = snapshot.LineName,
                // Operational metrics
                AvgWaiting = snapshot.AvgWaiting,
                MaxWaiting = snapshot.MaxWaiting,
                AvgLoadFactor = snapshot.AvgLoadFactor,
                CurrentTrainCount = snapshot.CurrentTrainCount,

                // Raw time-of-day
                DayOfWeek = snapshot.DayOfWeek,
                Hour = snapshot.Hour,
                Minute = snapshot.Minute,
                TimeMinutes = snapshot.TimeMinutes,

                // Cyclical encoding
                HourSin = snapshot.HourSin,
                HourCos = snapshot.HourCos,
                MinuteSin = snapshot.MinuteSin,
                MinuteCos = snapshot.MinuteCos,
                DaySin = snapshot.DaySin,
                DayCos = snapshot.DayCos,

                // Peak flags
                IsMorningPeak = snapshot.IsMorningPeak,
                IsLunchPeak = snapshot.IsLunchPeak,
                IsEveningPeak = snapshot.IsEveningPeak,
                IsWeekend = snapshot.IsWeekend,

                // Trend features
                DeltaWaiting = snapshot.DeltaWaiting,
                DeltaLoadFactor = snapshot.DeltaLoadFactor,

                // Demand multiplier
                ExpectedDemandMultiplier = snapshot.ExpectedDemandMultiplier
            };

            var response = await _client.PredictAsync(request);

            return (int)response.RecommendedFrequency;
        }

        public async Task<bool> PingAsync()
        {
            try
            {
                var response = await _client.PingAsync(new PingRequest());
                return response.IsHealthy;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task ShutdownAsync()
        {
            try
            {
                await _client.ShutdownAsync(new PingRequest());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Shutdown RPC failed: {ex}");
            }
        }


    }
}
