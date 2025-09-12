using System.ComponentModel.DataAnnotations;
using SDIA.SharedKernel.Enums;

namespace SDIA.API.Models.Registrations;

public class UpdateStatusDto
{
    [Required(ErrorMessage = "Le statut est requis")]
    public RegistrationStatus Status { get; set; }

    public string? RejectionReason { get; set; }
}