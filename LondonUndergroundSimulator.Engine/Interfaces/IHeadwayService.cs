using LondonUndergroundSimulator.Engine.Models;
using LondonUndergroundSimulator.Engine.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LondonUndergroundSimulator.Engine.Interfaces
{
    public interface IHeadwayService
    {
        Train GetTrainAhead(Train train);
        Train GetTrainBehind(Train train);
        float GetDistanceToTrainAhead(Train train);

        bool CanSpawnTrain(Train train, float minSafeDistance);

        public void setEngine(SimulationEngine engine);
    }

}
