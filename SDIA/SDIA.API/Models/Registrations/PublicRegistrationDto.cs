using System.ComponentModel.DataAnnotations;
using SDIA.SharedKernel.Enums;

namespace SDIA.API.Models.Registrations;

public class PublicRegistrationDto
{
    public Guid Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public RegistrationStatus Status { get; set; }

    [Required(ErrorMessage = "Le prénom est requis")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom est requis")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'email est requis")]
    [EmailAddress(ErrorMessage = "L'adresse email n'est pas valide")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Le numéro de téléphone n'est pas valide")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "La date de naissance est requise")]
    public DateTime BirthDate { get; set; }

    public string? FormData { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ValidatedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }

    public string OrganizationName { get; set; } = string.Empty;
    public string? FormTemplateName { get; set; }
    public bool IsMinor { get; set; }

    // For display
    public List<RegistrationCommentDto>? Comments { get; set; }
    public List<DocumentDto>? Documents { get; set; }
}

public class RegistrationCommentDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
}

public class DocumentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class UpdatePublicRegistrationDto
{
    [Required(ErrorMessage = "Le prénom est requis")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom est requis")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'email est requis")]
    [EmailAddress(ErrorMessage = "L'adresse email n'est pas valide")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Le numéro de téléphone n'est pas valide")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "La date de naissance est requise")]
    public DateTime BirthDate { get; set; }

    public string? FormData { get; set; }

    public bool Submit { get; set; } = false; // If true, changes status to Pending
}