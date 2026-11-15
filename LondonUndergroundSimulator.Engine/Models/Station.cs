using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LondonUndergroundSimulator.Engine.Models
{
    public class Station
    {
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public enum StationType { Quiet, Normal, Busy }
        public StationType Type { get; set; }


        public int WaitingPassengers { get; set; }

    }
}
