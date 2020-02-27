using Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Data.Entities
{
    public class User
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Key]
        public string UniqueIdentificationNumber { get; set; }
        public string Adress { get; set; }
        public string PhoneNumber { get; set; }
        public UserRole Role { get; set; }
    }
}
