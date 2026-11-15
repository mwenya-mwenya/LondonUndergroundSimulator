using LondonUndergroundSimulator.Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LondonUndergroundSimulator.Engine.Interfaces
{
    public interface ITrains
    {
        public List<Train> GetTrains();
        public void InitialiseTrains(List<Train> trains);


        public void UpdateTrainMovements(List<Train> trains, float dt);
    }
}
