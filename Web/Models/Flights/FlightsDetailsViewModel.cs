using Data.Entities;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Web.Models.Passager;
using Web.Models.Reservations;
using Web.Models.Shared;

namespace Web.Models.Flights
{
    public class FlightsDetailsViewModel
    {
        public PagerViewModel Pager { get; set; }
        public int UniqueNumber { get; set; }
        public string LocationFrom { get; set; }
        public string LocationTo { get; set; }
        public DateTime TakeOffTime { get; set; }
        public TimeSpan FlightDuration { get; set; }
        public TypesOfPlane Type { get; set; }
        public int PassagerCapacity { get; set; }
        public int BussinesClassCapacity { get; set; }
        public virtual ICollection<PassagersViewModel> Passagers { get; set; }
        [TempData]
        public int Id { get; set; }
        [TempData]
        [Range(1, 20, ErrorMessage = "Between 1 and 20")]
        public int NumberOfPassagers { get; set; }
    }
}
