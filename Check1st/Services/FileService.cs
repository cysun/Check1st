namespace Check1st.Services;

public class FileService
{
    private readonly AppDbContext _db;
    private readonly MinioService _minioService;

    private readonly ILogger<FileService> _logger;

    public FileService(AppDbContext db, MinioService minioService, ILogger<FileService> logger)
    {
        _db = db;
        _minioService = minioService;
        _logger = logger;
    }

    public Models.File GetFile(int id) => _db.Files.Find(id);

    public async Task<Models.File> UploadFileAsync(IFormFile uploadedFile, string ownerName)
    {
        string name = Path.GetFileName(uploadedFile.FileName);

        var file = new Models.File
        {
            Name = name,
            ContentType = uploadedFile.ContentType,
            Size = uploadedFile.Length,
            OwnerName = ownerName
        };
        _db.Files.Add(file);
        _logger.LogDebug("New file uploaded: {file}", name);


        _db.SaveChanges();
        _logger.LogInformation("File saved to database: {file}", name);

        await _minioService.UploadFileAsync(file, uploadedFile);
        _logger.LogInformation("File saved to object store: {file}", name);

        return file;
    }

    public async Task<string> GetDownloadUrlAsync(Models.File file, bool inline = false) =>
        await _minioService.GetDownloadUrlAsync(file, inline);


    public async Task DeleteFileAsync(int id)
    {
        var file = _db.Files.Find(id);
        await _minioService.DeleteFileAsync(id);
        _db.Files.Remove(file);
        _db.SaveChanges();
    }

    public void SaveChanges() => _db.SaveChanges();
}
