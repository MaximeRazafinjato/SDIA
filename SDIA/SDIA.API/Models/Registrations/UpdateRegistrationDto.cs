using System.ComponentModel.DataAnnotations;
using SDIA.SharedKernel.Enums;

namespace SDIA.API.Models.Registrations;

public class UpdateRegistrationDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    [EmailAddress(ErrorMessage = "L'adresse email n'est pas valide")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Le numéro de téléphone n'est pas valide")]
    public string? Phone { get; set; }

    public DateTime? BirthDate { get; set; }
    public string? FormData { get; set; }
    public RegistrationStatus? Status { get; set; }
    public string? RejectionReason { get; set; }
}