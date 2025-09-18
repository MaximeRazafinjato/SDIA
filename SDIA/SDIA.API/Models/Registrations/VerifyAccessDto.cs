using System.ComponentModel.DataAnnotations;

namespace SDIA.API.Models.Registrations;

public class VerifyAccessDto
{
    [Required(ErrorMessage = "Le token est requis")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le code de vérification est requis")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Le code doit contenir 6 chiffres")]
    public string Code { get; set; } = string.Empty;
}