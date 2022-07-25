using System.ComponentModel.DataAnnotations;

namespace udemy_aspnetcore_identity.Models
{
    public class SignUpViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress,ErrorMessage ="Email Address is missiong or valid.")]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password,ErrorMessage ="Incorrect or missing password.")]
        public string Password { get; set; }

        public string Role { get; set; }
        [Required]
        public string Department { get; set; }
    }
}
