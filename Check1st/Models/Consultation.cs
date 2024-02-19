using Markdig;
using System.ComponentModel.DataAnnotations;

namespace Check1st.Models;

public class Consultation
{
    public int Id { get; set; }

    public int AssignmentId { get; set; }
    public Assignment Assignment { get; set; }

    public string StudentName { get; set; }

    public List<File> Files { get; set; } = new List<File>();

    public DateTime TimeCreated { get; set; } = DateTime.UtcNow;
    public DateTime? TimeCompleted { get; set; }

    public bool IsCompleted => TimeCompleted != null;

    public string Feedback { get; set; } // Feedback by AI
    public string FeedbackHtml => Feedback != null ? Markdown.ToHtml(Feedback) : "";

    public int? FeedbackRating { get; set; } // student's rating of the feeback: 1-5
    public string FeedbackComments { get; set; } // student's comment of the feedback

    // Service usage properties

    [MaxLength(100)]
    public string Model { get; set; }
    public int? PromptTokens { get; set; }
    public int? CompletionTokens { get; set; }

    public void AddFile(File file)
    {
        Files.RemoveAll(f => f.Name == file.Name);
        Files.Add(file);
    }
}
