namespace SDIA.Application.Users.Management.Export;

public class UserManagementExportModel
{
    public string Format { get; set; } = "csv"; // csv, xlsx, json
    public List<string> Fields { get; set; } = new(); // Fields to export
    public string? SearchTerm { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public bool? EmailConfirmed { get; set; }
    public bool? PhoneConfirmed { get; set; }
    public Guid? OrganizationId { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
}

public class UserManagementExportResult
{
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public int RecordCount { get; set; }
    public DateTime ExportedAt { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}