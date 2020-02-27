using Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web.Models.Passager;
using Web.Models.Shared;

namespace Web.Models.Reservations
{
    public class ReservationsDetailsViewModel
    {
        public PagerViewModel Pager { get; set; }
        public string Email { get; set; }
        public int Id { get; set; }
        public ICollection<PassagersViewModel> Passagers { get; set; }
        public bool IsConfirmed { get; set; }
        public int TotalNumberOfPassagers { get; set; }
        public int NumberOfRegularPassagers { get; set; }
        public int NumberOfBussinesPassagers { get; set; }
        public string LocationFrom { get; set; }
        public string LocationTo { get; set; }
        public DateTime TakeOffTime { get; set; }
        public DateTime LandingTime { get; set; }
    }
}
