using Ardalis.Result;
using SDIA.Application.Common.Extensions;
using SDIA.Core.Users;
using System.Text;
using System.Text.Json;

namespace SDIA.Application.Users.Management.Export;

public class UserManagementExportService
{
    private readonly IUserRepository _userRepository;

    public UserManagementExportService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserManagementExportResult>> ExecuteAsync(
        UserManagementExportModel model,
        CancellationToken cancellationToken = default)
    {
        // Get filtered users
        var query = _userRepository.GetAll(cancellationToken);

        // Apply filters
        query = query.ApplyTextSearch(model.SearchTerm,
            u => u.FirstName,
            u => u.LastName,
            u => u.Email,
            u => u.Phone);

        if (!string.IsNullOrEmpty(model.Role))
        {
            query = query.Where(u => u.Role == model.Role);
        }

        if (model.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == model.IsActive.Value);
        }

        if (model.EmailConfirmed.HasValue)
        {
            query = query.Where(u => u.EmailConfirmed == model.EmailConfirmed.Value);
        }

        if (model.PhoneConfirmed.HasValue)
        {
            query = query.Where(u => u.PhoneConfirmed == model.PhoneConfirmed.Value);
        }

        if (model.OrganizationId.HasValue)
        {
            query = query.Where(u => u.OrganizationId == model.OrganizationId.Value);
        }

        if (model.CreatedFrom.HasValue)
        {
            query = query.Where(u => u.CreatedAt >= model.CreatedFrom.Value);
        }

        if (model.CreatedTo.HasValue)
        {
            query = query.Where(u => u.CreatedAt <= model.CreatedTo.Value);
        }

        // Execute query
        var users = await Task.Run(() => query.ToList(), cancellationToken);

        // Generate export based on format
        byte[] fileContent;
        string fileName;
        string contentType;
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

        switch (model.Format.ToLower())
        {
            case "json":
                fileContent = GenerateJsonExport(users);
                fileName = $"users_export_{timestamp}.json";
                contentType = "application/json";
                break;
            case "xlsx":
                // TODO: Implement Excel export
                fileContent = GenerateCsvExport(users);
                fileName = $"users_export_{timestamp}.csv";
                contentType = "text/csv";
                break;
            case "csv":
            default:
                fileContent = GenerateCsvExport(users);
                fileName = $"users_export_{timestamp}.csv";
                contentType = "text/csv";
                break;
        }

        var result = new UserManagementExportResult
        {
            FileContent = fileContent,
            FileName = fileName,
            ContentType = contentType,
            RecordCount = users.Count,
            ExportedAt = DateTime.UtcNow,
            Success = true,
            Message = $"Successfully exported {users.Count} users"
        };

        return Result<UserManagementExportResult>.Success(result);
    }

    private static byte[] GenerateCsvExport(List<User> users)
    {
        var csv = new StringBuilder();
        
        // Header
        csv.AppendLine("Id,FirstName,LastName,Email,Phone,Role,IsActive,EmailConfirmed,PhoneConfirmed,LastLoginAt,OrganizationName,CreatedAt");
        
        // Data
        foreach (var user in users)
        {
            csv.AppendLine($"{user.Id},{EscapeCsv(user.FirstName)},{EscapeCsv(user.LastName)},{EscapeCsv(user.Email)},{EscapeCsv(user.Phone)},{EscapeCsv(user.Role)},{user.IsActive},{user.EmailConfirmed},{user.PhoneConfirmed},{user.LastLoginAt:yyyy-MM-dd HH:mm:ss},{EscapeCsv(user.Organization?.Name ?? "")},{user.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }
        
        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    private static byte[] GenerateJsonExport(List<User> users)
    {
        var exportData = users.Select(u => new
        {
            u.Id,
            u.FirstName,
            u.LastName,
            u.Email,
            u.Phone,
            u.Role,
            u.IsActive,
            u.EmailConfirmed,
            u.PhoneConfirmed,
            u.LastLoginAt,
            OrganizationName = u.Organization?.Name,
            u.CreatedAt
        }).ToList();

        var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return Encoding.UTF8.GetBytes(json);
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}