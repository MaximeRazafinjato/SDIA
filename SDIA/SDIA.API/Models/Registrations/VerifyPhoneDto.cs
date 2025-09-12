using System.ComponentModel.DataAnnotations;

namespace SDIA.API.Models.Registrations;

public class VerifyPhoneDto
{
    [Required(ErrorMessage = "Le code de vérification est requis")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Le code de vérification doit contenir 6 chiffres")]
    public string Code { get; set; } = string.Empty;
}