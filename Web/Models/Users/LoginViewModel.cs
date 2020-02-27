using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Models.Users
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Should be filled")]
        public string Username { get; set; }


        [Required(ErrorMessage = "Should be filled")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
