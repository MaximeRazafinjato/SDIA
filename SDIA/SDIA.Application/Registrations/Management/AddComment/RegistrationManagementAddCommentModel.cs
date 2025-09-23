namespace SDIA.Application.Registrations.Management.AddComment;

public class RegistrationManagementAddCommentModel
{
    public Guid RegistrationId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public Guid? UserId { get; set; } // User who added the comment
    public bool IsInternal { get; set; } = true; // Internal comment vs public comment
}

public class RegistrationManagementAddCommentResult
{
    public Guid CommentId { get; set; }
    public Guid RegistrationId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsInternal { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}