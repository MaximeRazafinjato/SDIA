namespace SDIA.Application.FormTemplates.Management.Upsert;

public class FormTemplateManagementUpsertResult
{
    public Guid FormTemplateId { get; set; }
    public bool IsCreated { get; set; }
    public string Message { get; set; } = string.Empty;
}