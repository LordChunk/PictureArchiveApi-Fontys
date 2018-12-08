using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class MUser
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
