using Ardalis.Result;
using SDIA.Core.FormTemplates;

namespace SDIA.Application.FormTemplates.Management.Delete;

public class FormTemplateManagementDeleteService
{
    private readonly IFormTemplateRepository _formTemplateRepository;

    public FormTemplateManagementDeleteService(IFormTemplateRepository formTemplateRepository)
    {
        _formTemplateRepository = formTemplateRepository;
    }

    public async Task<Result> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var formTemplate = await _formTemplateRepository.GetByIdAsync(id, cancellationToken);

        if (formTemplate == null)
        {
            return Result.NotFound("Form template not found");
        }

        // Check if template has registrations
        var registrationCount = await _formTemplateRepository.GetRegistrationCountAsync(id);
        if (registrationCount > 0)
        {
            return Result.Error("Cannot delete form template with existing registrations");
        }

        await _formTemplateRepository.DeleteAsync(formTemplate, cancellationToken);

        return Result.Success();
    }
}