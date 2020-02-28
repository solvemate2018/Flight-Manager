using Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Entities
{
    public class Reservation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public ICollection<Passager> Passagers { get; set; }
        public bool IsConfirmed
        {
            get
            {
                if (Flight == null)
                {
                    return false;
                }
                else
                {
                if (Flight.PassagerCapacity >= NumberOfRegularPassagers
                    && Flight.BussinesClassCapacity >= NumberOfBussinesPassagers)
                {
                    return true;
                }
                return false;
                }

            }
            private set { }
        }
        public int TotalNumberOfPassagers 
        {
            get
            {
                return NumberOfBussinesPassagers + NumberOfRegularPassagers;
            }
            private set { }
        }
        public int NumberOfRegularPassagers
        {
            get
            {
                if (Passagers.Count == 0)
                {
                    return 0;
                }
                else
                {
                    int sum = 0;
                    foreach (Passager passager in Passagers)
                    {
                        if (passager.Type == TypesOfTicket.Regular)
                        {
                            sum++;
                        }
                    }
                    return sum;
                }

            }
            private set { }
        }
        public int NumberOfBussinesPassagers
        {
            get
            {
                if (Passagers.Count == 0)
                {
                    return 0;
                }
                else
                {
                    int sum = 0;
                    foreach (Passager passager in Passagers)
                    {
                        if (passager.Type == TypesOfTicket.Bussines)
                        {
                            sum++;
                        }
                    }
                    return sum;
                }
            }
            private set { }
        }
        public string Email { get; set; }
        public bool IsCancelled { get; set; }
        public int FlightId { get; set; }
        public Flight Flight { get; set; }
        public Reservation()
        {
            if (Passagers == null)
            {
                Passagers = new List<Passager>();
            }
        }
    }
}
