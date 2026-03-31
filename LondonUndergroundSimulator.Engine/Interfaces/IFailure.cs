using LondonUndergroundSimulator.Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LondonUndergroundSimulator.Engine.Interfaces
{
  
        public interface IFailureRule
        {
            bool ShouldTrigger(float deltaTime);
            float GetDelayDuration(Train train);

           string Name { get; }
    }
    
}
