using Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Entities
{
    public class Passager
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string UniqueIdentificationNumber { get; set; }
        public string PhoneNumber { get; set; }
        public Nationalities Nationality { get; set; }
        public TypesOfTicket Type { get; set; }
        public int ReservationId { get; set; }

        public Reservation Reservation { get; set; }
    }
}
