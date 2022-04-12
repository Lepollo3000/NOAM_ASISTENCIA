using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace NOAM_ASISTENCIA.Server.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public bool Lockout { get; set; }
        [Required]
        public bool ForgotPassword { get; set; }
    }
}