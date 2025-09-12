using SDIA.SharedKernel.Models;
using SDIA.SharedKernel.Enums;

namespace SDIA.Core.FormTemplates;

public class FormField : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Placeholder { get; set; } = string.Empty;
    public string HelpText { get; set; } = string.Empty;
    public FormFieldType Type { get; set; }
    public bool IsRequired { get; set; } = false;
    public bool IsReadOnly { get; set; } = false;
    public int Order { get; set; }
    public string ValidationRules { get; set; } = string.Empty; // JSON rules
    public string Options { get; set; } = string.Empty; // JSON for select/radio options
    public string DefaultValue { get; set; } = string.Empty;
    public bool ShowForMinor { get; set; } = true;
    public bool ShowForAdult { get; set; } = true;
    
    // Relations
    public Guid FormSectionId { get; set; }
    public virtual FormSection? FormSection { get; set; }
}