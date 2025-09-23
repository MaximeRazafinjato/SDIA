using Ardalis.Result;
using SDIA.Core.Registrations;
using SDIA.Core.Users;

namespace SDIA.Application.Registrations.Management.AddComment;

public class RegistrationManagementAddCommentService
{
    private readonly RegistrationManagementAddCommentValidator _validator;
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IUserRepository _userRepository;

    public RegistrationManagementAddCommentService(
        RegistrationManagementAddCommentValidator validator,
        IRegistrationRepository registrationRepository,
        IUserRepository userRepository)
    {
        _validator = validator;
        _registrationRepository = registrationRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<RegistrationManagementAddCommentResult>> ExecuteAsync(
        RegistrationManagementAddCommentModel model,
        CancellationToken cancellationToken = default)
    {
        // Validate the model
        var validationResult = await _validator.ValidateAsync(model, cancellationToken);
        if (!validationResult.IsSuccess)
        {
            return Result<RegistrationManagementAddCommentResult>.Invalid(validationResult.ValidationErrors);
        }

        // Get the registration
        var registration = await _registrationRepository.GetByIdAsync(model.RegistrationId, cancellationToken);
        if (registration == null)
        {
            return Result<RegistrationManagementAddCommentResult>.NotFound("Registration not found");
        }

        // Get user information if provided
        User? user = null;
        string? userName = null;
        if (model.UserId.HasValue)
        {
            user = await _userRepository.GetByIdAsync(model.UserId.Value, cancellationToken);
            if (user != null)
            {
                userName = $"{user.FirstName} {user.LastName}";
            }
        }

        // Create the comment
        var comment = new RegistrationComment
        {
            Id = Guid.NewGuid(),
            RegistrationId = model.RegistrationId,
            AuthorId = model.UserId ?? Guid.Empty,
            Content = model.Comment,
            IsInternal = model.IsInternal,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Add comment to registration
        registration.Comments.Add(comment);
        registration.UpdatedAt = DateTime.UtcNow;

        await _registrationRepository.UpdateAsync(registration, cancellationToken);

        var result = new RegistrationManagementAddCommentResult
        {
            CommentId = comment.Id,
            RegistrationId = model.RegistrationId,
            Comment = model.Comment,
            UserName = userName,
            CreatedAt = comment.CreatedAt,
            IsInternal = model.IsInternal,
            Success = true,
            Message = "Comment added successfully"
        };

        return Result<RegistrationManagementAddCommentResult>.Success(result);
    }
}