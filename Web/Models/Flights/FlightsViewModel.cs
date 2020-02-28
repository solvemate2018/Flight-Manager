using Data.Enums;
using System;

namespace Web.Models.Flights
{
    public class FlightsViewModel
    {
        public bool IsCancelled { get; set; }
        public bool IsOld { get; set; }
        public string LocationFrom { get; set; }
        public string LocationTo { get; set; }
        public DateTime TakeOffTime { get; set; }
        public DateTime LandingTime { get; set; }
        public TypesOfPlane Type { get; set; }
        public int UniqueNumber { get; set; }
        public string PilotName { get; set; }
        public int PassagerCapacity { get; set; }
        public int BussinesClassCapacity { get; set; }
    }
}