using LondonUndergroundSimulator.Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LondonUndergroundSimulator.Engine.Interfaces
{
    public interface ITrainMovementService
    {
        void UpdateMovement(Train train, float dt);
        void UpdateGeometry(Train train, float dt);

        void InitialiseTrain(Train train);
    }


}
