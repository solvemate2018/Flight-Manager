using Data.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Web.Controllers
{
    public class MailsController
    {
        //Send information, if the reservation was confirmed
        public void SendReservationConfirmation(string email, ICollection<Passager> passagers)
        {
            using (var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true
            })
            {
                client.UseDefaultCredentials = false;

                client.Credentials = new NetworkCredential("georgy.d.2001@gmail.com", "$0rryBate");

                foreach (var passager in passagers)
                {
                    var body = new StringBuilder();

                    body.Append("We want to let you know, that your request was approved! And this is information for each passager.");

                    body.Append("\r\n");
                    body.Append($"First name: {passager.FirstName}, Last name: {passager.LastName}, Type of ticket: {passager.Type}");


                    client.Send("flightmanager@gmail.com", email, "Your reservation request", body.ToString());
                }
            }
        }

        //Send information to each reservation host, when the flight is cancelled
        public void SendReservationCancellation(ICollection<Reservation> reservations)
        {
            using (var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true
            })
            {
                client.UseDefaultCredentials = false;

                client.Credentials = new NetworkCredential("georgy.d.2001@gmail.com", "$0rryBate");

                foreach (var reservation in reservations)
                {
                    var body = new StringBuilder();

                    body.Append($"We are sorry to inform you that your reservation for flight " +
                        $"from {reservation.Flight.LocationFrom} to {reservation.Flight.LocationTo} was cancelled.");

                    client.Send("flightmanager@gmail.com", reservation.Email, "Flight Cancellation", body.ToString());
                }
            }

        }
    }
}
