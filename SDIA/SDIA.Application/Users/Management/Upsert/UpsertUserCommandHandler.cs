using Ardalis.Result;
using MediatR;
using SDIA.Core.Users;

namespace SDIA.Application.Users.Management.Upsert;

public class UpsertUserCommandHandler : IRequestHandler<UpsertUserCommand, Result<UserUpsertResult>>
{
    private readonly IUserRepository _userRepository;

    public UpsertUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserUpsertResult>> Handle(UpsertUserCommand request, CancellationToken cancellationToken)
    {
        User user;
        bool isCreated;
        
        if (request.IsUpdate)
        {
            user = await _userRepository.GetByIdAsync(request.Id!.Value, cancellationToken);
            if (user == null)
            {
                return Result<UserUpsertResult>.NotFound("User not found");
            }
            isCreated = false;
        }
        else
        {
            // Check if email already exists
            var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingUser != null)
            {
                return Result<UserUpsertResult>.Error("A user with this email already exists");
            }
            
            user = new User();
            isCreated = true;
        }

        // Update user properties
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.Phone = request.Phone;
        user.Role = request.Role;
        user.IsActive = request.IsActive;
        user.OrganizationId = request.OrganizationId;

        // Set password if provided (for new users or password change)
        if (!string.IsNullOrEmpty(request.Password))
        {
            user.SetPassword(request.Password);
        }
        else if (isCreated)
        {
            return Result<UserUpsertResult>.Error("Password is required for new users");
        }

        if (isCreated)
        {
            await _userRepository.AddAsync(user, cancellationToken);
        }
        else
        {
            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        var result = new UserUpsertResult
        {
            UserId = user.Id,
            IsCreated = isCreated,
            Message = isCreated ? "User created successfully" : "User updated successfully"
        };

        return Result<UserUpsertResult>.Success(result);
    }
}