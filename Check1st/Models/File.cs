using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Check1st.Models;

public class File
{
    public int Id { get; set; }

    [Required, MaxLength(1000)]
    public string Name { get; set; }

    public int Version { get; set; } = 1;

    [MaxLength(255)]
    public string ContentType { get; set; }

    public long Size { get; set; }

    public DateTime TimeCreated { get; set; } = DateTime.UtcNow;

    public IdentityUser Owner { get; set; }

    public string GetFormattedSize()
    {
        if (Size < 1024)
            return $"{Size} B";
        else if (Size < 1048576)
            return (Size / 1024.0).ToString("0.#") + " KB";
        else
            return (Size / 1024.0 / 1024.0).ToString("0.#") + " MB";
    }
}

public class FileRevision
{
    public int FileId { get; set; }
    public File File { get; set; }

    [Required, MaxLength(1000)]
    public string Name { get; set; }

    public int Version { get; set; }

    [MaxLength(255)]
    public string ContentType { get; set; }

    public long Size { get; set; }

    public DateTime TimeCreated { get; set; }
}
