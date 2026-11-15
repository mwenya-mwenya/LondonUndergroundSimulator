using MLInferenceEngine.Abstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LondonUndergroundSimulator.Training.Services
{
    public class TrainingService : ITrainingService
    {
        private readonly TrainingHost _host;

        public TrainingService(TrainingHost host)
        {
            _host = host;
        }

        public Task TrainAsync(bool userInitiated)
            => Task.Run(() => _host.Run(userInitiated));
    }

}
