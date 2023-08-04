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
        public int PassagerCapacity
        {
            get
            {
                int result = MaxPassagerCapacity;
                if (Reservations != null)
                {
                    foreach (var reservation in Reservations)
                    {
                        result -= reservation.NumberOfRegularPassagers;
                    }
                }
                return result;
            }
        }
        public int BussinesClassCapacity
        {
            get
            {
                int result = MaxBussinesCapacity;
                if (Reservations != null)
                {
                    foreach (var reservation in Reservations)
                    {
                        result -= reservation.NumberOfBussinesPassagers;
                    }
                }
                return result;
            }
        }
        public ICollection<Reservation> Reservations { get; set; }
        public bool IsCanceled { get; set; }
        public bool IsOld { get { return DateTime.Now > TakeOffTime; } private set { } }

        public bool AddReservation(Reservation reservation)
        {
            if (Reservations == null)
            {
                Reservations = new List<Reservation>();
            }
            if (reservation.NumberOfRegularPassagers <= this.PassagerCapacity 
                && reservation.NumberOfBussinesPassagers <= this.BussinesClassCapacity)
            {
                this.Reservations.Add(reservation);
                return true;
            }
            else
            {
                return false;
            }
        }
        public Flight()
        {
            Reservations = new List<Reservation>();
        }
    }
}
