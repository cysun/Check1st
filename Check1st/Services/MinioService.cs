using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace Check1st.Services;

public class MinioSettings
{
    public string Endpoint { get; set; }
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
    public string Bucket { get; set; } // Space for DigitalOcean
    public string PathPrefix { get; set; } // E.g. "mynotes/", or "" if no folder is used

    // Files are treated as plain text unless specified here.
    public HashSet<string> NonTextTypes { get; set; }
}

public class MinioService
{
    private IMinioClient _client;
    private MinioSettings _settings;

    private ILogger<MinioService> _logger;

    public MinioService(IOptions<MinioSettings> settings, ILogger<MinioService> logger)
    {
        _settings = settings.Value;
        _client = new MinioClient()
            .WithEndpoint(_settings.Endpoint)
            .WithCredentials(_settings.AccessKey, _settings.SecretKey)
            .WithSSL()
            .Build();

        _logger = logger;
    }

    public bool IsNonTextType(string fileName)
    {
        return _settings.NonTextTypes.Contains(Path.GetExtension(fileName).ToLower());
    }

    public string GetObjectName(Models.File file) => GetObjectName(file.Id);

    public string GetObjectName(int fileId) => $"{_settings.PathPrefix}{fileId}";

    public async Task UploadFileAsync(Models.File file, IFormFile uploadedFile)
    {
        var objectName = GetObjectName(file);
        using var data = uploadedFile.OpenReadStream();
        var args = new PutObjectArgs()
            .WithBucket(_settings.Bucket)
            .WithObject(objectName)
            .WithStreamData(data)
            .WithObjectSize(file.Size)
            .WithContentType(file.ContentType);

        try
        {
            await _client.PutObjectAsync(args);
        }
        catch (MinioException e)
        {
            _logger.LogError(e, "Failed to upload {object}", objectName);
        }
    }

    public async Task<string> GetDownloadUrlAsync(Models.File file, bool inline = false)
    {
        var reqParams = new Dictionary<string, string> {
            { "response-content-type", IsNonTextType(file.Name)? file.ContentType : "text/plain"  },
            { "response-content-disposition", inline ? "inline" : @$"attachment; filename=""{file.Name}""" }
        };

        var args = new PresignedGetObjectArgs()
            .WithBucket(_settings.Bucket)
            .WithObject(GetObjectName(file))
            .WithExpiry(10) // Download link valid for 10 seconds
            .WithHeaders(reqParams);

        return await _client.PresignedGetObjectAsync(args);
    }

    public async Task DeleteFileAsync(int fileId)
    {
        var objectName = GetObjectName(fileId);
        var args = new RemoveObjectArgs()
            .WithBucket(_settings.Bucket)
            .WithObject(objectName);

        try
        {
            await _client.RemoveObjectAsync(args);
        }
        catch (MinioException e)
        {
            _logger.LogError(e, "Failed to remove {object}", objectName);
        }
    }
}
