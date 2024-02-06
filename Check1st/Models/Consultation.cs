﻿namespace Check1st.Models;

public class Consultation
{
    public int Id { get; set; }

    public int AssignmentId { get; set; }
    public Assignment Assignment { get; set; }

    public string StudentName { get; set; }

    public List<File> Files { get; set; } = new List<File>();

    public DateTime? TimeCreated { get; set; }
    public DateTime? TimeCompleted { get; set; }

    public bool IsCompleted => TimeCompleted != null;

    public string Feedback { get; set; }

    public int? FeedbackRating { get; set; } // student's rating of the feeback: 1-5
    public string FeedbackComment { get; set; } // student's comment of the feedback
}