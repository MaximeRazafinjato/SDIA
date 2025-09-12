using System.ComponentModel.DataAnnotations;

namespace SDIA.API.Models.FormTemplates;

public class CreateFormTemplateDto
{
    [Required(ErrorMessage = "Le nom du template est requis")]
    [StringLength(200, ErrorMessage = "Le nom ne peut pas dépasser 200 caractères")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "La description ne peut pas dépasser 1000 caractères")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "La configuration JSON est requise")]
    public string JsonConfiguration { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}