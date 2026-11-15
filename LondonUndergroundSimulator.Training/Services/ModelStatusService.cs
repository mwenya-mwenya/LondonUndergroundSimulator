using MLInferenceEngine.Abstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LondonUndergroundSimulator.Training.Services
{
    public class ModelStatusService : IModelStatusService
    {
        public bool ModelExists =>
            File.Exists("training_output/model.bin");

        public bool IsDataOptimal =>
            Directory.GetFiles("data", "simulation_*.csv")
                     .SelectMany(File.ReadLines)
                     .Count() > 50000;
    }

}
