using Check1st.Models;
using Microsoft.EntityFrameworkCore;

namespace Check1st.Services;

public class AssignmentService
{
    private readonly AppDbContext _db;

    public AssignmentService(AppDbContext db)
    {
        _db = db;
    }

    public Assignment GetAssignment(int id) => _db.Assignments.Where(a => a.Id == id && !a.IsDeleted).FirstOrDefault();

    public List<Assignment> GetAssignments() => _db.Assignments.AsNoTracking().Where(a => !a.IsDeleted)
        .OrderByDescending(a => a.TimeClosed).ThenBy(a => a.Name)
        .ToList();

    public List<Assignment> GetCurrentAssignments() => _db.Assignments.AsNoTracking()
        .Where(a => !a.IsDeleted && a.TimePublished != null && a.TimePublished < DateTime.UtcNow
            && (a.TimeClosed == null || a.TimeClosed > DateTime.UtcNow))
        .OrderBy(a => a.Name)
        .ToList();

    public void AddAssignment(Assignment assignment)
    {
        _db.Assignments.Add(assignment);
        _db.SaveChanges();
    }

    public void DeleteAssignment(Assignment assignment)
    {
        if (assignment.IsDeleted) return;

        if (assignment.IsPublished)
        {
            assignment.IsDeleted = true;
        }
        else
        {
            _db.Assignments.Remove(assignment);
        }

        _db.SaveChanges();
    }

    public void SaveChanges() => _db.SaveChanges();
}
