using AutoMapper;
using Check1st.Models;
using Check1st.Security;
using Check1st.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Check1st.Controllers
{
    [Authorize(Policy = Constants.Policy.IsAdminOrTeacher)]
    public class AssignmentController : Controller
    {
        private readonly AssignmentService _assignmentService;

        private readonly IMapper _mapper;
        private readonly ILogger<AssignmentController> _logger;

        public AssignmentController(AssignmentService assignmentService, IMapper mapper, ILogger<AssignmentController> logger)
        {
            _assignmentService = assignmentService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(_assignmentService.GetAssignments());
        }

        public IActionResult View(int id)
        {
            var assignment = _assignmentService.GetAssignment(id);
            if (assignment == null) return NotFound();

            return View(assignment);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View(new AssignmentInputModel());
        }

        [HttpPost]
        public IActionResult Add(AssignmentInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var assignment = _mapper.Map<Assignment>(input);
            assignment.TeacherName = User.Identity.Name;
            _assignmentService.AddAssignment(assignment);
            _logger.LogInformation("{user} created assignment {assignment}", User.Identity.Name, assignment.Id);

            return RedirectToAction("View", new { id = assignment.Id });
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var assignment = _assignmentService.GetAssignment(id);
            if (assignment == null) return NotFound();

            ViewBag.Assignment = assignment;
            return View(_mapper.Map<AssignmentInputModel>(assignment));
        }

        [HttpPost]
        public IActionResult Edit(int id, AssignmentInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var assignment = _assignmentService.GetAssignment(id);
            if (assignment == null) return NotFound();

            _mapper.Map(input, assignment);
            _assignmentService.SaveChanges();
            _logger.LogInformation("{user} edited assignment {assignment}", User.Identity.Name, id);

            return RedirectToAction("View", new { id });
        }

        public IActionResult Delete(int id)
        {
            var assignment = _assignmentService.GetAssignment(id);
            if (assignment == null) return NotFound();

            _assignmentService.DeleteAssignment(assignment);
            _logger.LogInformation("{user} deleted assignment {assignment}", User.Identity.Name, id);

            return RedirectToAction("Index");
        }

        public IActionResult DownloadCsv()
        {
            var assignments = _assignmentService.GetAssignments();

            string[] properties = { "Id", "Name", "TimePublished", "TimeClosed", "TeacherName" };
            var csv = new StringBuilder();
            csv.AppendLine(string.Join(",", properties));
            foreach (var assignment in assignments)
            {
                csv.AppendLine(string.Join(",", properties.Select(p => p switch
                {
                    "TimePublished" => assignment.TimePublished?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                    "TimeClosed" => assignment.TimeClosed?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                    _ => assignment.GetType().GetProperty(p).GetValue(assignment)?.ToString()
                })));
            }

            var filename = $"assignments_{DateTime.Now:yyyyMMdd}.csv";
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", filename);
        }
    }
}

namespace Check1st.Models
{
    public class AssignmentInputModel
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Display(Name = "Allowed File Types")]
        public string AcceptedFileTypes { get; set; } = ".html,.java";

        [Display(Name = "Max File Size")]
        public int MaxFileSize { get; set; } = 4096;

        [Display(Name = "Publish Time")]
        public DateTime? TimePublished { get; set; }

        [Display(Name = "Close Time")]
        public DateTime? TimeClosed { get; set; }

        public bool IsPublished => TimePublished.HasValue && TimePublished < DateTime.UtcNow;
        public bool IsClosed => TimeClosed.HasValue && TimeClosed < DateTime.UtcNow;
    }
}