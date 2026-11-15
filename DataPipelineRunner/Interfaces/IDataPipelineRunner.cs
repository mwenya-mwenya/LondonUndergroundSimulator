using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LondonUndergroundSimulator.Engine.Models;

namespace LondonUndergroundSimulator.App.Interfaces
{
    public interface IDataPipelineRunner
    {
        public List<Line> LoadLines();
        public List<Train> LoadTrains(List<Line> lines);
    }
}
