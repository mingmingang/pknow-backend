using System.ComponentModel.DataAnnotations;

namespace pknow_backend.Controllers
{
    public class RegisterGoogleUserRequest
    {
        [Required(ErrorMessage = "Email wajib diisi.")]
        [EmailAddress(ErrorMessage = "Format email tidak valid.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
        public string NamaLengkap { get; set; }

        public string Gender { get; set; }
        public string NoTelp { get; set; }

        [Required(ErrorMessage = "CreatedBy wajib diisi.")]
        public string CreatedBy { get; set; }
    }
}
