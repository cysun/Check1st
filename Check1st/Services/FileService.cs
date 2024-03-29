﻿using Check1st.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Check1st.Services;

public class FileService
{
    private readonly AppDbContext _db;

    private readonly ILogger<FileService> _logger;

    public FileService(AppDbContext db, ILogger<FileService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public Models.File GetFile(int id) => _db.Files
        .Where(f => f.Id == id).Include(f => f.Content)
        .FirstOrDefault();

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

        using (var streamReader = new StreamReader(uploadedFile.OpenReadStream(), Encoding.UTF8))
        {
            file.Content = new FileContent
            {
                Text = await streamReader.ReadToEndAsync()
            };
        }

        if (string.IsNullOrWhiteSpace(file.Content.Text))
        {
            _logger.LogWarning("Ignore {user} uploaded file due to empty content: {file}", ownerName, name);
            return null;
        }

        _db.Files.Add(file);
        _db.SaveChanges();
        _logger.LogInformation("File saved to database: {file}", name);

        return file;
    }

    public void LoadContent(Models.File file)
    {
        _db.Entry(file).Reference(f => f.Content).Load();
    }

    public void SaveChanges() => _db.SaveChanges();
}
