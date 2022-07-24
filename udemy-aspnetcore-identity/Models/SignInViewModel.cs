using System.ComponentModel.DataAnnotations;

namespace udemy_aspnetcore_identity.Models
{
    public class SignInViewModel
    {
        [Required(ErrorMessage ="User name mus be provided.")]
        [DataType(DataType.EmailAddress)]
        public string Username { get; set; }
        [Required(ErrorMessage ="Password must be provided.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
