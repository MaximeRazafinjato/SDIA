using SDIA.SharedKernel.Models;

namespace SDIA.Core.FormTemplates;

public class FormSection : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsConditional { get; set; } = false;
    public string ConditionExpression { get; set; } = string.Empty; // JSON expression
    
    // Relations
    public Guid FormTemplateId { get; set; }
    public virtual FormTemplate? FormTemplate { get; set; }
    
    public virtual ICollection<FormField> Fields { get; set; } = new List<FormField>();
}