using LondonUndergroundSimulator.Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LondonUndergroundSimulator.Engine.Interfaces
{
    public interface ITerminalLoopService
    {
        void HandleLoop(Train train);

        void setMovementService(ITrainMovementService movement);
    }

}
