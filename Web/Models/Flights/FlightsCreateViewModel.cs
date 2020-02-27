using Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Models.Flights
{
    public class FlightsCreateViewModel
    {
        [Required]
        [StringLength(90, ErrorMessage = "Location name must be between 1 and 90.", MinimumLength = 1)]
        public string LocationFrom { get; set; }
        [Required]
        [StringLength(90, ErrorMessage = "Location name must be between 1 and 90.", MinimumLength = 1)]
        public string LocationTo { get; set; }
        [Required(ErrorMessage = "Should be filled")]
        [DataType(DataType.DateTime)]
        public DateTime TakeOffTime { get; set; }
        [Required(ErrorMessage = "Should be filled")]
        [DataType(DataType.DateTime)]
        public DateTime LandingTime { get; set; }
        [Required(ErrorMessage = "Should be filled")]
        [EnumDataType(typeof(TypesOfPlane))]
        public TypesOfPlane Type { get; set; }
        [Required(ErrorMessage = "Should be filled")]
        [StringLength(40, ErrorMessage = "Pilot name must be between 2 and 40.", MinimumLength = 2)]
        public string PilotName { get; set; }
        [Required(ErrorMessage = "Should be filled")]
        public int MaxPassagerCapacity { get; set; }
        [Required(ErrorMessage = "Should be filled")]
        public int MaxBussinesCapacity { get; set; }
    }
}
