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

    public Assignment GetAssignment(int id) => _db.Assignments.Find(id);

    public List<Assignment> GetAssignments() => _db.Assignments.AsNoTracking()
        .OrderByDescending(a => a.TimeClosed).ThenBy(a => a.Name)
        .ToList();

    public void SaveChanges() => _db.SaveChanges();
}
