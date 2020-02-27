using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Web.Models.Shared;

namespace Web.Models.Flights
{
    public class FlightsIndexViewModel
    {
        public PagerViewModel Pager { get; set; }

        public ICollection<FlightsViewModel> Items { get; set; }

        [TempData]
        public int NumberOfPassagers { get; set; }
        [TempData]
        public int Id { get; set; }
    }
}
