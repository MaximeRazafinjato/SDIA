using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
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
        var template = await _formTemplateRepository.GetAll(cancellationToken)
            .Include(ft => ft.Registrations)
            .FirstOrDefaultAsync(ft => ft.Id == id, cancellationToken);

        if (template == null)
        {
            return Result.NotFound("Modèle de formulaire non trouvé");
        }

        if (template.Registrations.Any())
        {
            return Result.Invalid(new List<ValidationError>
            {
                new ValidationError
                {
                    Identifier = "FormTemplate",
                    ErrorMessage = "Impossible de supprimer un modèle utilisé par des inscriptions"
                }
            });
        }

        await _formTemplateRepository.DeleteAsync(template, cancellationToken);

        return Result.Success();
    }
}