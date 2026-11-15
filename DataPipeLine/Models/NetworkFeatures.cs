using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataPipeLine.Models
{
    public class NetworkFeatures
    {
        public override string ToString()
        {
            return $"AvgNetworkLoad={AvgNetworkLoad}, TotalPassengersMoved={TotalPassengersMoved}, OvercrowdedStations={OvercrowdedStations}";
        }
    
        public double AvgNetworkLoad { get; set; }
        public int TotalPassengersMoved { get; set; }

        public int OvercrowdedStations { get; set; }
    }
}
