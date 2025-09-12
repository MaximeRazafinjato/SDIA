namespace SDIA.Infrastructure.Settings;

public class FileStorageSettings
{
    public const string SectionName = "FileStorage";
    
    public string Provider { get; set; } = "Local"; // Local, Azure, AWS, etc.
    public string BasePath { get; set; } = "uploads";
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10 MB default
    public string[] AllowedFileTypes { get; set; } = { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };
    public bool EnableVirusScanning { get; set; } = false;
    public bool GenerateChecksum { get; set; } = true;
    public bool EnableCompression { get; set; } = false;
    public int CompressionLevel { get; set; } = 6;
    public TimeSpan TemporaryUrlExpiration { get; set; } = TimeSpan.FromHours(1);
    
    // Local storage specific settings
    public LocalStorageSettings Local { get; set; } = new();
    
    // Azure Blob Storage settings (for future use)
    public AzureBlobSettings AzureBlob { get; set; } = new();
}

public class LocalStorageSettings
{
    public string RootDirectory { get; set; } = "wwwroot/uploads";
    public string BaseUrl { get; set; } = "/uploads";
    public bool CreateDateDirectories { get; set; } = true;
    public string DateDirectoryFormat { get; set; } = "yyyy/MM/dd";
}

public class AzureBlobSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = "uploads";
    public string BaseUrl { get; set; } = string.Empty;
    public bool EnableCdn { get; set; } = false;
    public string CdnUrl { get; set; } = string.Empty;
}