using Inference;
using MLInferenceEngine.Abstractions.Interfaces;
using MLInferenceEngine.Abstractions.Models;
using MLInferenceEngine.Services;

namespace MLInferenceEngine.Orchestration.Services
{
    public class FrequencyAdvisor : IFrequencyAdvisor
    {
        private readonly FrequencyInferenceClient _client;

        public FrequencyAdvisor(FrequencyInferenceClient client)
        {
            _client = client;
        }

        public async Task<int> GetRecommendedFrequencyAsync(SimulationSnapshot snapshot)
        {
            Console.WriteLine("ADVISOR: Received snapshot, forwarding to MLInferenceEngine...");

            Console.WriteLine(
                $"ADVISOR Snapshot -> " +
                $"AvgWaiting={snapshot.AvgWaiting}, " +
                $"MaxWaiting={snapshot.MaxWaiting}, " +
                $"AvgLoadFactor={snapshot.AvgLoadFactor}, " +
                $"TrainCount={snapshot.CurrentTrainCount}, " +
                $"TimeMinutes={snapshot.TimeMinutes}, " +
                $"Hour={snapshot.Hour}, " +
                $"DayOfWeek={snapshot.DayOfWeek}, " +
                $"ExpectedDemand={snapshot.ExpectedDemandMultiplier}"
            );

            // Build the proto request
            var request = new SnapshotRequest
            {
                AvgWaiting = snapshot.AvgWaiting,
                MaxWaiting = snapshot.MaxWaiting,
                AvgLoadFactor = snapshot.AvgLoadFactor,
                CurrentTrainCount = snapshot.CurrentTrainCount,

                DayOfWeek = snapshot.DayOfWeek,
                Hour = snapshot.Hour,
                Minute = snapshot.Minute,
                TimeMinutes = snapshot.TimeMinutes,

                HourSin = snapshot.HourSin,
                HourCos = snapshot.HourCos,
                MinuteSin = snapshot.MinuteSin,
                MinuteCos = snapshot.MinuteCos,
                DaySin = snapshot.DaySin,
                DayCos = snapshot.DayCos,

                IsMorningPeak = snapshot.IsMorningPeak,
                IsLunchPeak = snapshot.IsLunchPeak,
                IsEveningPeak = snapshot.IsEveningPeak,
                IsWeekend = snapshot.IsWeekend,

                DeltaWaiting = snapshot.DeltaWaiting,
                DeltaLoadFactor = snapshot.DeltaLoadFactor,

                ExpectedDemandMultiplier = snapshot.ExpectedDemandMultiplier
            };

            var response = await _client.PredictAsync(request);

            Console.WriteLine($"ADVISOR: MLInferenceEngine returned frequency = {response}");

            return (int)response;
        }

    }
}
