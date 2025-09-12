using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SDIA.Core.Services;
using SDIA.Infrastructure.Settings;
using System.Security.Cryptography;
using System.Text;

namespace SDIA.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly FileStorageSettings _settings;
    private readonly ILogger<FileStorageService> _logger;
    private readonly string _baseDirectory;

    public FileStorageService(
        IOptions<FileStorageSettings> settings,
        ILogger<FileStorageService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        
        // Ensure the base directory exists
        _baseDirectory = Path.IsPathRooted(_settings.Local.RootDirectory) 
            ? _settings.Local.RootDirectory 
            : Path.Combine(Directory.GetCurrentDirectory(), _settings.Local.RootDirectory);
        
        Directory.CreateDirectory(_baseDirectory);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            // Validate file type
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!_settings.AllowedFileTypes.Contains(extension))
            {
                throw new ArgumentException($"File type {extension} is not allowed");
            }

            // Validate file size
            if (fileStream.Length > _settings.MaxFileSize)
            {
                throw new ArgumentException($"File size {fileStream.Length} exceeds maximum allowed size {_settings.MaxFileSize}");
            }

            var relativePath = GenerateFilePath(fileName);
            var fullPath = Path.Combine(_baseDirectory, relativePath);
            
            // Ensure directory exists
            var directory = Path.GetDirectoryName(fullPath)!;
            Directory.CreateDirectory(directory);

            // Copy stream to memory first for processing
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            // Compress if enabled
            if (_settings.EnableCompression && ShouldCompress(contentType))
            {
                fileBytes = await CompressFileAsync(fileBytes);
            }

            // Write file
            await File.WriteAllBytesAsync(fullPath, fileBytes);
            
            _logger.LogInformation("File uploaded successfully: {FileName} -> {RelativePath}", fileName, relativePath);
            
            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName}", fileName);
            throw;
        }
    }

    public async Task<string> UploadFileAsync(byte[] fileContent, string fileName, string contentType)
    {
        using var stream = new MemoryStream(fileContent);
        return await UploadFileAsync(stream, fileName, contentType);
    }

    public async Task<Stream> DownloadFileAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_baseDirectory, filePath);
            
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var fileBytes = await File.ReadAllBytesAsync(fullPath);
            
            // Decompress if needed
            if (_settings.EnableCompression)
            {
                try
                {
                    fileBytes = await DecompressFileAsync(fileBytes);
                }
                catch
                {
                    // If decompression fails, assume file is not compressed
                    fileBytes = await File.ReadAllBytesAsync(fullPath);
                }
            }

            return new MemoryStream(fileBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file {FilePath}", filePath);
            throw;
        }
    }

    public async Task<byte[]> DownloadFileAsBytesAsync(string filePath)
    {
        using var stream = await DownloadFileAsync(filePath);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_baseDirectory, filePath);
            
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FilePath}", filePath);
            throw;
        }
    }

    public async Task<bool> FileExistsAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_baseDirectory, filePath);
            return File.Exists(fullPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence {FilePath}", filePath);
            return false;
        }
    }

    public async Task<Core.Services.FileInfo> GetFileInfoAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_baseDirectory, filePath);
            
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var fileInfo = new System.IO.FileInfo(fullPath);
            var checksum = _settings.GenerateChecksum ? await CalculateChecksumAsync(fullPath) : string.Empty;

            return new Core.Services.FileInfo
            {
                FileName = fileInfo.Name,
                FilePath = filePath,
                Size = fileInfo.Length,
                ContentType = GetContentType(fileInfo.Extension),
                CreatedAt = fileInfo.CreationTimeUtc,
                LastModified = fileInfo.LastWriteTimeUtc,
                Checksum = checksum
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file info {FilePath}", filePath);
            throw;
        }
    }

    public async Task<IEnumerable<Core.Services.FileInfo>> ListFilesAsync(string directoryPath)
    {
        try
        {
            var fullPath = Path.Combine(_baseDirectory, directoryPath);
            
            if (!Directory.Exists(fullPath))
            {
                return new List<Core.Services.FileInfo>();
            }

            var files = Directory.GetFiles(fullPath, "*", SearchOption.TopDirectoryOnly);
            var fileInfos = new List<Core.Services.FileInfo>();

            foreach (var file in files)
            {
                var relativePath = Path.GetRelativePath(_baseDirectory, file);
                var fileInfo = await GetFileInfoAsync(relativePath);
                fileInfos.Add(fileInfo);
            }

            return fileInfos.OrderBy(f => f.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files in directory {DirectoryPath}", directoryPath);
            throw;
        }
    }

    public async Task<string> GetFileUrlAsync(string filePath)
    {
        // For local storage, return a relative URL
        var baseUrl = _settings.Local.BaseUrl.TrimEnd('/');
        var normalizedPath = filePath.Replace('\\', '/');
        return $"{baseUrl}/{normalizedPath}";
    }

    public async Task<string> GetTemporaryUrlAsync(string filePath, TimeSpan expiration)
    {
        // For local storage, temporary URLs are not really temporary
        // In a real implementation with cloud storage, you would generate signed URLs
        var url = await GetFileUrlAsync(filePath);
        var token = GenerateTemporaryToken(filePath, expiration);
        return $"{url}?token={token}&expires={DateTimeOffset.UtcNow.Add(expiration).ToUnixTimeSeconds()}";
    }

    public async Task<long> GetDirectorySizeAsync(string directoryPath)
    {
        try
        {
            var fullPath = Path.Combine(_baseDirectory, directoryPath);
            
            if (!Directory.Exists(fullPath))
            {
                return 0;
            }

            var files = Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories);
            return files.Sum(file => new System.IO.FileInfo(file).Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating directory size {DirectoryPath}", directoryPath);
            throw;
        }
    }

    public async Task<bool> CreateDirectoryAsync(string directoryPath)
    {
        try
        {
            var fullPath = Path.Combine(_baseDirectory, directoryPath);
            Directory.CreateDirectory(fullPath);
            _logger.LogInformation("Directory created: {DirectoryPath}", directoryPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating directory {DirectoryPath}", directoryPath);
            throw;
        }
    }

    public async Task<bool> DeleteDirectoryAsync(string directoryPath, bool recursive = false)
    {
        try
        {
            var fullPath = Path.Combine(_baseDirectory, directoryPath);
            
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, recursive);
                _logger.LogInformation("Directory deleted: {DirectoryPath}", directoryPath);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting directory {DirectoryPath}", directoryPath);
            throw;
        }
    }

    private string GenerateFilePath(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var safeFileName = $"{nameWithoutExtension}_{Guid.NewGuid():N}{extension}";

        if (_settings.Local.CreateDateDirectories)
        {
            var dateDirectory = DateTime.UtcNow.ToString(_settings.Local.DateDirectoryFormat);
            return Path.Combine(dateDirectory, safeFileName);
        }

        return safeFileName;
    }

    private bool ShouldCompress(string contentType)
    {
        var compressibleTypes = new[] { "text/", "application/json", "application/xml", "application/javascript" };
        return compressibleTypes.Any(type => contentType.StartsWith(type, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<byte[]> CompressFileAsync(byte[] data)
    {
        using var output = new MemoryStream();
        using var compressionStream = new System.IO.Compression.GZipStream(output, 
            (System.IO.Compression.CompressionLevel)_settings.CompressionLevel);
        
        await compressionStream.WriteAsync(data);
        await compressionStream.FlushAsync();
        
        return output.ToArray();
    }

    private async Task<byte[]> DecompressFileAsync(byte[] compressedData)
    {
        using var input = new MemoryStream(compressedData);
        using var decompressionStream = new System.IO.Compression.GZipStream(input, 
            System.IO.Compression.CompressionMode.Decompress);
        using var output = new MemoryStream();
        
        await decompressionStream.CopyToAsync(output);
        return output.ToArray();
    }

    private async Task<string> CalculateChecksumAsync(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var sha256 = SHA256.Create();
        var hash = await Task.Run(() => sha256.ComputeHash(stream));
        return Convert.ToBase64String(hash);
    }

    private string GetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".txt" => "text/plain",
            ".html" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".json" => "application/json",
            ".xml" => "application/xml",
            _ => "application/octet-stream"
        };
    }

    private string GenerateTemporaryToken(string filePath, TimeSpan expiration)
    {
        var data = $"{filePath}:{DateTimeOffset.UtcNow.Add(expiration).ToUnixTimeSeconds()}";
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash)[..16]; // Take first 16 characters
    }
}