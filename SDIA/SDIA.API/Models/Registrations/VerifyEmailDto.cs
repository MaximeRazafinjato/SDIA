using System.ComponentModel.DataAnnotations;

namespace SDIA.API.Models.Registrations;

public class VerifyEmailDto
{
    [Required(ErrorMessage = "Le token de vérification est requis")]
    public string Token { get; set; } = string.Empty;
}