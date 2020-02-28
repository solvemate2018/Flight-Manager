using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Models.Users
{
    public class UsersEditViewModel
    {
        public string UniqueIdentificationNumber { get; set; }

        [Required(ErrorMessage = "Should be filled")]
        [DataType(DataType.Password)]
        public string Password { get; set; }


        [Required(ErrorMessage = "Should be filled")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The Passwords do not match")]
        public string ConfirmPassword { get; set; }


        [Required(ErrorMessage = "Should be filled")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Should be in Email format")]
        public string Email { get; set; }


        [Required(ErrorMessage = "Should be filled")]
        [StringLength(40, ErrorMessage = "First name must be between 2 and 40.", MinimumLength = 2)]
        public string FirstName { get; set; }


        [Required(ErrorMessage = "Should be filled")]
        [StringLength(40, ErrorMessage = "Last name must be between 2 and 40.", MinimumLength = 2)]
        public string LastName { get; set; }


        [Required(ErrorMessage = "Should be filled")]
        public string Adress { get; set; }


        [Required(ErrorMessage = "Should be filled")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "Should be in phone number format")]
        public string PhoneNumber { get; set; }
    }
}
