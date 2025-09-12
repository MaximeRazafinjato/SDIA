namespace SDIA.Application.FormTemplates.Management.Upsert;

public class FormTemplateUpsertResult
{
    public Guid FormTemplateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool IsCreated { get; set; }
    public string Message { get; set; } = string.Empty;
}