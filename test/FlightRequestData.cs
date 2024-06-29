using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Airfrance_bot
{
    public class FlightRequestData
    {
        public Data data { get; set; }
    }
    public class Data
    {
        public string originIata { get; set; }
        public string originIataType { get; set; }
        public string DestinationIata { get; set; }
        public string DestinationIataType { get; set; }
        public int Adults { get; set; }
        public string CabinClass { get; set; }
        public string DepartureDate { get; set; }
        public string browser { get; set; }
    }
}
