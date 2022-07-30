using System.ComponentModel.DataAnnotations;

namespace udemy_aspnetcore_identity.Models
{
    public class MFAViewModel
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string Code { get; set; }

        public string? QRCodeUrl { get; set; }
    }
}
