﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web.Models.Shared;

namespace Web.Models.Reservations
{
    public class ReservationsIndexViewModel
    {
        public PagerViewModel Pager { get; set; }

        public ICollection<ReservationsViewModel> Items { get; set; }

    }
}
