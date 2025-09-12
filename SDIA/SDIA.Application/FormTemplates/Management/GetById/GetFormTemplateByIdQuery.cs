using SDIA.Application.Common.Models;

namespace SDIA.Application.FormTemplates.Management.GetById;

public class GetFormTemplateByIdQuery : BaseQuery<FormTemplateDto>
{
    public Guid FormTemplateId { get; set; }

    public GetFormTemplateByIdQuery(Guid formTemplateId)
    {
        FormTemplateId = formTemplateId;
    }
}