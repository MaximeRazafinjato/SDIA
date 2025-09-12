using SDIA.Application.Common.Models;

namespace SDIA.Application.FormTemplates.Management.Delete;

public class DeleteFormTemplateCommand : BaseCommand
{
    public Guid FormTemplateId { get; set; }

    public DeleteFormTemplateCommand(Guid formTemplateId)
    {
        FormTemplateId = formTemplateId;
    }
}