using Check1st.Models;
using Microsoft.EntityFrameworkCore;

namespace Check1st.Services;

public class ConsultationService
{
    private readonly AppDbContext _db;

    public ConsultationService(AppDbContext db)
    {
        _db = db;
    }

    public List<Consultation> GetConsultations(int assignmentId, string studentName) => _db.Consultations.AsNoTracking()
        .Where(c => c.Assignment.Id == assignmentId && c.StudentName == studentName)
        .OrderByDescending(c => c.TimeCreated)
        .ToList();

    public Consultation GetLastConsultation(int assignmentId, string studentName) => _db.Consultations.AsNoTracking()
        .Where(c => c.Assignment.Id == assignmentId && c.StudentName == studentName)
        .OrderByDescending(c => c.TimeCreated)
        .FirstOrDefault();

    public void AddConsultation(Consultation consultation)
    {
        consultation.TimeCreated = DateTime.UtcNow;
        _db.Consultations.Add(consultation);
        _db.SaveChanges();
    }

    public void SaveChanges() => _db.SaveChanges();
}
