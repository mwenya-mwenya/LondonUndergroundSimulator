using LondonUndergroundSimulator.Engine.Models;
using LondonUndergroundSimulator.Engine.Services;
using System.Collections.Generic;


namespace LondonUndergroundSimulator.ViewModels
{
    public class MapViewModel
    {
        public List<Line> Lines { get; }
      
        public MapViewModel(List<Line> lines, SimulationEngine engine)
        {
            Lines = lines;
         
        }
    }
}
