using System.ComponentModel.DataAnnotations;

namespace udemy_aspnetcore_identity.Models
{

    public class MNFACheckViewModel
    {
        [Required]
        public string Code { get; set; }

    }
}
