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
        public bool IsConfirmed { get; set; }
        public int TotalNumberOfPassagers { get; set; }
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
            set { }
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
            set { }
        }
        public string Email { get; set; }
        public bool IsCancelled { get; set; }
        public int FlightId { get; set; }
        public Flight Flight { get; set; }
        public Reservation()
        {
            Passagers = new List<Passager>();
        }
    }
}
