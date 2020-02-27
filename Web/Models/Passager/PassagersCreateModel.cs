using Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Models.Passager
{
    public class PassagersCreateModel
    {
        [Required(ErrorMessage = "Must Fill")]
        [StringLength(40, ErrorMessage = "Passager name must be between 2 and 40.", MinimumLength = 2)]
        public string FirstName { get; set; }


        [Required(ErrorMessage = "Must Fill")]
        [StringLength(40, ErrorMessage = "Passager name must be between 2 and 40.", MinimumLength = 2)]
        public string MiddleName { get; set; }


        [Required(ErrorMessage = "Must Fill")]
        [StringLength(40, ErrorMessage = "Passager name must be between 2 and 40.", MinimumLength = 2)]
        public string LastName { get; set; }


        [Required(ErrorMessage = "Must Fill")]
        [StringLength(10, ErrorMessage = "UIN length must be 10", MinimumLength = 10)]
        public string UniqueIdentificationNumber { get; set; }


        [Required(ErrorMessage = "Must Fill")]
        [DataType(DataType.PhoneNumber, ErrorMessage ="Should be in phone number format")]
        public string PhoneNumber { get; set; }


        [Required(ErrorMessage ="Must Fill")]
        [EnumDataType(typeof(Nationalities))]
        public Nationalities Nationality { get; set; }


        [Required(ErrorMessage = "Must Fill")]
        [EnumDataType(typeof(TypesOfTicket))]
        public TypesOfTicket Type { get; set; }
    }
}
