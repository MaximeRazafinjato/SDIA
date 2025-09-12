namespace SDIA.Core.Services;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task<string> UploadFileAsync(byte[] fileContent, string fileName, string contentType);
    Task<Stream> DownloadFileAsync(string filePath);
    Task<byte[]> DownloadFileAsBytesAsync(string filePath);
    Task<bool> DeleteFileAsync(string filePath);
    Task<bool> FileExistsAsync(string filePath);
    Task<FileInfo> GetFileInfoAsync(string filePath);
    Task<IEnumerable<FileInfo>> ListFilesAsync(string directoryPath);
    Task<string> GetFileUrlAsync(string filePath);
    Task<string> GetTemporaryUrlAsync(string filePath, TimeSpan expiration);
    Task<long> GetDirectorySizeAsync(string directoryPath);
    Task<bool> CreateDirectoryAsync(string directoryPath);
    Task<bool> DeleteDirectoryAsync(string directoryPath, bool recursive = false);
}

public class FileInfo
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long Size { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastModified { get; set; }
    public string Checksum { get; set; } = string.Empty;
}