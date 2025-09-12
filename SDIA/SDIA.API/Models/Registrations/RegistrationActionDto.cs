using System.ComponentModel.DataAnnotations;

namespace SDIA.API.Models.Registrations;

public class RegistrationActionDto
{
    [MaxLength(1000, ErrorMessage = "Le commentaire ne peut pas dépasser 1000 caractères")]
    public string? Comment { get; set; }
}