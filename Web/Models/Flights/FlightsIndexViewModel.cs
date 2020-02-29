using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Web.Models.Shared;

namespace Web.Models.Flights
{
    public class FlightsIndexViewModel
    {
        public PagerViewModel Pager { get; set; }

        public int PageSize { get; set; }

        public string Order { get; set; }

        public ICollection<FlightsViewModel> Items { get; set; }

        [TempData]
        [Range(1, 20, ErrorMessage ="Between 1 and 20")]
        public int NumberOfPassagers { get; set; }
        [TempData]
        public int Id { get; set; }
    }
}
