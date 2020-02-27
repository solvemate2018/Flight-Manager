namespace Web.Models.Reservations
{
    public class ReservationsViewModel
    {
        public int Id { get; set; }
        public bool IsConfirmed { get; set; }
        public int TotalNumberOfPassagers { get; set; }
        public string Email { get; set; }
    }
}