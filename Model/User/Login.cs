using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Models.User
{
    class Login : ILogin
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public interface ILogin
    {
        string Email { get; set; }
        string Password { get; set; }
    }
}
