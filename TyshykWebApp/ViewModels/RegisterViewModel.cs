using Microsoft.Build.Framework;

namespace TyshykWebApp.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string Email {  get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }
    }
}
