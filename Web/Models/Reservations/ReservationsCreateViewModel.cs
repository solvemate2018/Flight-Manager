using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Web.Models.Passager;

namespace Web.Models.Reservations
{
    public class ReservationsCreateViewModel
    {
        [Required]
        [Range(1, 20)]
        public int TotalNumberOfPassagers { get; set; }

        [Required(ErrorMessage ="Should be filled")]
        [DataType(DataType.EmailAddress, ErrorMessage ="Should be in Email format")]
        public string Email { get; set; }
        [Required]
        [HiddenInput]
        public virtual int FlightId { get; set; }

        public PassagersCreateModel[] Passagers { get; set; }
    }
}
