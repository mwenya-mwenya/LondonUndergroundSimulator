using LondonUndergroundSimulator.Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LondonUndergroundSimulator.Engine.Interfaces
{
    public interface ITrainDelayService
    {
        bool IsTrainDelayed(Train train);
        void UpdateDelay(Train train, float dt, TimeSpan clock);
        void ApplyDelay(Train train, IFailureRule rule, TimeSpan clock);
        bool UpdateDwell(Train train, float dt);
    }


}
