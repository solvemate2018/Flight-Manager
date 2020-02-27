using Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Entities
{
    public class Flight
    {
        public string LocationFrom { get; set; }
        public string LocationTo { get; set; }
        public DateTime TakeOffTime { get; set; }
        public DateTime LandingTime { get; set; }
        public TypesOfPlane Type { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UniqueNumber { get; private set; }
        public string PilotName { get; set; }
        public int MaxPassagerCapacity { get; set; }
        public int MaxBussinesCapacity { get; set; }
        public int PassagerCapacity { get; set; }
        public int BussinesClassCapacity { get; set; }
        public ICollection<Reservation> Reservations { get; set; }
        public bool IsCanceled { get; set; }

        public Flight()
        {
            Reservations = new List<Reservation>();
        }

        public bool AddReservation(Reservation reservation)
        {
            if (reservation.NumberOfRegularPassagers <= this.PassagerCapacity 
                && reservation.NumberOfBussinesPassagers <= this.BussinesClassCapacity)
            {
                this.Reservations.Add(reservation);
                this.PassagerCapacity -= reservation.NumberOfRegularPassagers;
                this.BussinesClassCapacity -= reservation.NumberOfBussinesPassagers;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
