using System.ComponentModel.DataAnnotations;

namespace Check1st.Models;

public class Assignment
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; }

    public string Description { get; set; }
    public string Prompt { get; set; }

    [Required, MaxLength(100)]
    public string AcceptedFileTypes { get; set; } // a comma-separated list of file extensions, e.g. ".html,.java"
    public int MaxFileSize { get; set; } // in bytes

    public DateTime? TimePublished { get; set; }
    public DateTime? TimeClosed { get; set; }

    public bool IsPublished => TimePublished.HasValue && TimePublished < DateTime.UtcNow;
    public bool IsClosed => TimeClosed.HasValue && TimeClosed < DateTime.UtcNow;

    public string TeacherName { get; set; }

    public bool IsDeleted { get; set; }
}
