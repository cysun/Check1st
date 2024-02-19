using Check1st.Models;
using Microsoft.EntityFrameworkCore;
using static Check1st.Security.Constants;

namespace Check1st.Services;

public class ConsultationService
{
    private readonly AppDbContext _db;

    public ConsultationService(AppDbContext db)
    {
        _db = db;
    }

    public Consultation GetConsultation(int id) => _db.Consultations.Where(c => c.Id == id)
        .Include(c => c.Assignment).Include(c => c.Files.OrderBy(f => f.TimeUploaded))
        .FirstOrDefault();

    public List<Consultation> GetRecentConsultations(string userName, Role role, int days = 7)
    {
        var query = role switch
        {
            Role.Admin => _db.Consultations,
            Role.Teacher => _db.Consultations.Where(c => c.Assignment.TeacherName == userName),
            _ => _db.Consultations.Where(c => c.StudentName == userName)
        };

        return query.AsNoTracking().Where(c => c.TimeCreated > DateTime.UtcNow.AddDays(-days))
            .Include(c => c.Assignment).OrderByDescending(c => c.TimeCreated)
            .ToList();
    }

    public List<Consultation> GetConsultations(int assignmentId, string studentName) => _db.Consultations.AsNoTracking()
        .Where(c => c.Assignment.Id == assignmentId && c.StudentName == studentName)
        .OrderByDescending(c => c.TimeCreated)
        .ToList();

    public Consultation GetLastConsultation(int assignmentId, string studentName)
    {
        var consultation = _db.Consultations.Where(c => c.Assignment.Id == assignmentId && c.StudentName == studentName)
            .OrderByDescending(c => c.TimeCreated).FirstOrDefault();
        if (consultation != null)
            _db.Entry(consultation).Collection(c => c.Files).Load();
        return consultation;
    }

    public void AddConsultation(Consultation consultation)
    {
        _db.Consultations.Add(consultation);
        _db.SaveChanges();
    }

    public void SaveChanges() => _db.SaveChanges();
}
