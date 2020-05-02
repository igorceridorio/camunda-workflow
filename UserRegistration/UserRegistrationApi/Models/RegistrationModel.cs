using System.ComponentModel.DataAnnotations;

namespace UserRegistrationApi
{
    public class RegistrationModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Password { get; set; }

        [Compare(nameof(Password))]
        public string RepeatPassword { get; set; }
    }
}
