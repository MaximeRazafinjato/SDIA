using Ardalis.Result;
using MediatR;
using SDIA.Core.FormTemplates;

namespace SDIA.Application.FormTemplates.Management.Delete;

public class DeleteFormTemplateCommandHandler : IRequestHandler<DeleteFormTemplateCommand, Result>
{
    private readonly IFormTemplateRepository _formTemplateRepository;

    public DeleteFormTemplateCommandHandler(IFormTemplateRepository formTemplateRepository)
    {
        _formTemplateRepository = formTemplateRepository;
    }

    public async Task<Result> Handle(DeleteFormTemplateCommand request, CancellationToken cancellationToken)
    {
        var formTemplate = await _formTemplateRepository.GetByIdAsync(request.FormTemplateId, cancellationToken);
        
        if (formTemplate == null)
        {
            return Result.NotFound("Form template not found");
        }

        // Check if form template is in use (has registrations)
        // This would require a method in the repository to check usage
        // For now, we'll do a soft delete
        formTemplate.IsDeleted = true;
        formTemplate.IsActive = false;
        await _formTemplateRepository.UpdateAsync(formTemplate, cancellationToken);

        return Result.Success();
    }
}