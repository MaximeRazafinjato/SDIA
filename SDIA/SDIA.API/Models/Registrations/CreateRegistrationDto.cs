using System.ComponentModel.DataAnnotations;

namespace SDIA.API.Models.Registrations;

public class CreateRegistrationDto
{
    [Required(ErrorMessage = "Le prénom est requis")]
    [StringLength(100, ErrorMessage = "Le prénom ne peut pas dépasser 100 caractères")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom est requis")]
    [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'email est requis")]
    [EmailAddress(ErrorMessage = "Format d'email invalide")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Format de téléphone invalide")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "La date de naissance est requise")]
    public DateTime BirthDate { get; set; }

    [Required(ErrorMessage = "Le template de formulaire est requis")]
    public Guid FormTemplateId { get; set; }

    public string? FormData { get; set; }
}